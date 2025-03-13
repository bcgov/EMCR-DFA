using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Linq;

namespace DFA.CRM.CustomWorkflow
{
    /// <summary>
    /// This is called when Project status is changing.
    /// To calculate Reovery Claims Total Paid after Percentage Cut
    /// And to calculate Recovery Claims True Forecast
    /// </summary>
    public class RollupProjectRecoveryClaimsPaidandForecast : CodeActivity
    {
        [RequiredArgument]
        [Input("Project")]
        [ReferenceTarget("dfa_project")]
        public InArgument<EntityReference> ProjectInput { get; set; }

        [Output("True Forecast")]
        public OutArgument<decimal> TrueForecastOutput { get; set; }

        [Output("TotalPaidAfterPercent")]
        public OutArgument<decimal> TotalPaidAfterPercentOutput { get; set; }

        CodeActivityContext Activity;
        ITracingService Tracing;
        IWorkflowContext Workflow;
        IOrganizationServiceFactory ServiceFactory;
        IOrganizationService Service;

        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="activityContext"></param>
        protected override void Execute(CodeActivityContext activityContext)
        {
            try
            {
                Activity = activityContext;
                Tracing = Activity.GetExtension<ITracingService>();
                Workflow = Activity.GetExtension<IWorkflowContext>();
                ServiceFactory = Activity.GetExtension<IOrganizationServiceFactory>();
                Service = ServiceFactory.CreateOrganizationService(Workflow.UserId);

                Tracing.Trace(
                "Entered WorkflowActivityTest.Execute(), Activity Instance Id: {0}, Workflow Instance Id: {1}",
                activityContext.ActivityInstanceId,
                activityContext.WorkflowInstanceId);

                Tracing.Trace("WorkflowActivityTest.Execute(), Correlation Id: {0}, Initiating User: {1}",
                Workflow.CorrelationId,
                Workflow.InitiatingUserId);
                var projectER = ProjectInput.Get(activityContext);
                TrueForecastOutput.Set(activityContext, 0m);
                TotalPaidAfterPercentOutput.Set(activityContext, 0m);
                if (projectER == null)
                {
                    // Take Early Exit
                    return;
                }
                var trueForecast = 0m;
                var totalPaid = 0m;
                var projectRead = Service.Retrieve("dfa_project", projectER.Id, new ColumnSet(true));
                var statusReason = projectRead.GetAttributeValue<OptionSetValue>("statuscode");
                // Approved Status are: Approved (222,710,001), Pending Adjudication (222,710,000), Closed - Project Completed (2)
                var approvedStatusReason = statusReason.Value == 222710001 ||
                    statusReason.Value == 222710000 ||
                    statusReason.Value == 2;
                if (!approvedStatusReason)
                {
                    // This only rollup approved projects.
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Exit because of Project Not Yet Approved. {statusReason.Value}");
                    return;
                }
                else
                {
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Project Status. {statusReason.Value}");
                    var approvedPercentageNullableDecimal = projectRead.GetAttributeValue<Nullable<decimal>>("dfa_approvedpercentages");
                    var projectApprovedCostMoney = projectRead.GetAttributeValue<Money>("dfa_approvedcost");
                    var projectAmendedApprovedCostMoney = projectRead.GetAttributeValue<Money>("dfa_approvedamendedprojectcost");

                    var projectApprovedCost = 0m;
                    
                    if (projectApprovedCostMoney != null && approvedStatusReason == true)
                    {
                        projectApprovedCost = projectApprovedCostMoney.Value;
                    }
                    //IF there is amended value overwrite the previous value
                    if (projectAmendedApprovedCostMoney != null && approvedStatusReason == true)
                    {
                        projectApprovedCost = projectAmendedApprovedCostMoney.Value;
                    }
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Total Project Approved Cost. {projectApprovedCost}");
                    var approvedPercentage = 80m; // Default to 80%
                    if (approvedPercentageNullableDecimal != null)
                    {
                        // Otherwise, use the default percentage
                        approvedPercentage = approvedPercentageNullableDecimal.Value;
                    }
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Project Approved Percentage. {approvedPercentage}%");
                    var recoveryClaims = GetRecoveryClaimsByProject(Service, Tracing, projectER);
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Recovery Claim Count. {recoveryClaims.Entities.Count}");
                    foreach (var claim in recoveryClaims.Entities)
                    {
                        var recordClaimAmount = claim.GetAttributeValue<Money>("dfa_claimamount");
                        var isFirstClaim = claim.GetAttributeValue<bool>("dfa_isfirstclaim");

                        if (recordClaimAmount != null)
                        {
                            var fullAmount = recordClaimAmount.Value;
                            if (isFirstClaim)
                            {
                                fullAmount = fullAmount - 1000m;
                            }
                            var paidAmountAfterPercentDeduction = fullAmount * approvedPercentage / 100m;
                            totalPaid += paidAmountAfterPercentDeduction;
                        }
                    }
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Total Project Paid. {totalPaid}");
                    // There should be no negative with total paid
                    if (totalPaid < 0)
                    {
                        totalPaid = 0m;
                    }

                    trueForecast = projectApprovedCost - totalPaid;
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: True Forecast. {trueForecast}");
                    // There should be no negative with total forecast
                    if (trueForecast < 0)
                    {
                        trueForecast = 0m;
                    }
                }
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Setting output");
                TrueForecastOutput.Set(activityContext, trueForecast);
                TotalPaidAfterPercentOutput.Set(activityContext, totalPaid);
            }
            catch (InvalidPluginExecutionException) { throw; }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException($"Unexpected error. {ex.Message}", ex);
            }
        }

        public EntityCollection GetRecoveryClaimsByProject(IOrganizationService Service, ITracingService Tracing, EntityReference projectER)
        {
            var query = new QueryExpression("dfa_projectclaim") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0); // Active
            query.Criteria.AddCondition("dfa_recoveryplanid", ConditionOperator.Equal, projectER.Id);
            query.Criteria.AddCondition("dfa_claimpaiddate", ConditionOperator.NotNull);
            var results = Service.RetrieveMultiple(query);
            return results;
        }
    }
}

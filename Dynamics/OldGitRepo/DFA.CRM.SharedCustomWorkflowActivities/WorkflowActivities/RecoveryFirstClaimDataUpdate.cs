using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Linq;

namespace DFA.CRM.CustomWorkflow
{
    /// <summary>
    /// This is called from Recovery Claim and only when Is First Claim is set to TRUE
    /// It updates fellow recovery claim under the same case to ensure the Is First Claim is NOT True and the One Time Deduct value is set to ZERO
    /// It updates all project status report files to represent the correct data at corresponding fields.
    /// Workflow steps should determine if this Recovery Claim has indeed changes its Is First Claim value into TRUE
    /// </summary>
    public class RecoveryFirstClaimDataUpdate : CodeActivity
    {
        [RequiredArgument]
        [Input("Recovery Claim")]
        [ReferenceTarget("dfa_projectclaim")]
        public InArgument<EntityReference> ClaimInput { get; set; }

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
                var claimER = ClaimInput.Get(activityContext);
                if (claimER == null)
                {
                    // Take Early Exit
                    return;
                }

                var claim = Service.Retrieve("dfa_projectclaim", claimER.Id, new ColumnSet(true));
                var isFirstClaim = claim.GetAttributeValue<bool>("dfa_isfirstclaim");
                var caseER = claim.GetAttributeValue<EntityReference>("dfa_caseid");
                var psrER = claim.GetAttributeValue<EntityReference>("dfa_projectstatusreportid");
                
                if (caseER == null)
                {
                    // Take Early Exit
                    return;
                }

                // Deal with other claims under the same case that was flagged as Is First Claim previously....
                // There should only be 0 or 1 during any given time
                var claimQuery = new QueryExpression("dfa_projectclaim") { ColumnSet = new ColumnSet(true) };
                claimQuery.Criteria.AddCondition("dfa_projectclaimid", ConditionOperator.NotEqual, claimER.Id); // Not this one
                claimQuery.Criteria.AddCondition("dfa_caseid", ConditionOperator.Equal, caseER.Id); // Of the same case
                claimQuery.Criteria.AddCondition("dfa_isfirstclaim", ConditionOperator.Equal, true); // Was stated as First Claim previously
                var claims = Service.RetrieveMultiple(claimQuery);
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Process other Claims of the same Case that has Is First Claim set as TRUE: {claims.Entities.Count}");
                foreach (var claimRecord in claims.Entities)
                {
                    var isActive = (claimRecord.GetAttributeValue<OptionSetValue>("statecode").Value == 0);
                    if (!isActive)
                    {
                        // If Not Active.
                        var claimActivation = new Entity("dfa_projectclaim");
                        claimActivation["dfa_projectclaimid"] = claimRecord.Id;
                        claimActivation["statecode"] = new OptionSetValue(0);
                        claimActivation["statuscode"] = new OptionSetValue(1);
                        Service.Update(claimActivation); // Activate the record 
                    }

                    var claimWrite = new Entity("dfa_projectclaim");
                    claimWrite["dfa_projectclaimid"] = claimRecord.Id;
                    claimWrite["dfa_isfirstclaim"] = false;
                    claimWrite["dfa_onetimedeductionamount"] = null;
                    Service.Update(claimWrite);

                    if (!isActive)
                    {
                        // If Not Active.
                        var claimDeactivation = new Entity("dfa_projectclaim");
                        claimDeactivation["dfa_projectclaimid"] = claimRecord.Id;
                        claimDeactivation["statecode"] = new OptionSetValue(1);
                        claimDeactivation["statuscode"] = new OptionSetValue(2);
                        Service.Update(claimDeactivation); // Activate the record 
                    }
                }

                // Deal with the Corresponding PSR record

                if (psrER != null)
                {
                    decimal atPercentage;
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Process the Recovery Claim's PSR");
                    var psr = Service.Retrieve("dfa_projectstatusreport", psrER.Id, new ColumnSet(true));
                    var projectER = psr.GetAttributeValue<EntityReference>("dfa_projectid");
                    var project = Service.Retrieve("dfa_project", projectER.Id, new ColumnSet(true));
                    var projectApprovedCost = project.GetAttributeValue<Money>("dfa_approvedcost");
                    var atPercentageNullable = project.GetAttributeValue<Nullable<decimal>>("dfa_approvedpercentages");
                    if (atPercentageNullable == null)
                    {
                        atPercentage = 80m;
                    }
                    else
                    {
                        atPercentage = atPercentageNullable.Value;
                    }
                    var amount = psr.GetAttributeValue<Money>("dfa_amount");
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: PSR Amount {amount}");
                    var subTotal = amount.Value;
                    if (isFirstClaim)
                    {
                        subTotal -= 1000m;
                    }
                    if (subTotal < 0)
                    {
                        subTotal = 0m;
                    }
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Subtotal Amount {subTotal}");
                    var totalAfterPercentCut = subTotal * atPercentage / 100m;
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Total (Subtotal @ {atPercentage}%: {totalAfterPercentCut}");
                    var psrWrite = new Entity("dfa_projectstatusreport");
                    psrWrite["dfa_projectstatusreportid"] = psrER.Id;
                    if (isFirstClaim)
                    {
                        psrWrite["dfa_less1000"] = new Money(1000);
                    }
                    else
                    {
                        psrWrite["dfa_less1000"] = null;
                    }
                    psrWrite["dfa_subtotal"] = new Money(subTotal);
                    psrWrite["dfa_atpercent"] = atPercentage;
                    psrWrite["dfa_totalafterpercentcut"] = new Money(totalAfterPercentCut);
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()} - PSR: {psrER.Id} - {psrER.Name}");
                    Service.Update(psrWrite);
                }

                if (!isFirstClaim)
                {
                    // If is not first claim, then don't have to update others
                    return;
                }
                // Deal with those that are not the first one.
                // Now typically, there won't be any unless user keep changing the Is First Claim flag
                if (psrER != null)
                {
                    var psrQuery = new QueryExpression("dfa_projectstatusreport") { ColumnSet = new ColumnSet(true) };
                    psrQuery.Criteria.AddCondition("dfa_caseid", ConditionOperator.Equal, caseER.Id); // Same Case
                    psrQuery.Criteria.AddCondition("dfa_projectstatusreportid", ConditionOperator.NotEqual, psrER.Id); // Not the one corresponding to the triggering claim
                    psrQuery.AddOrder("dfa_projectid", OrderType.Ascending);
                    var psrRecords = Service.RetrieveMultiple(psrQuery);
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Process other PSR of the same Case: {psrRecords.Entities.Count}");
                    var projectId = Guid.Empty;
                    Entity project = null;
                    Money projectApprovedCost = null;
                    string projectName = null;
                    string projectNumber = null;
                    decimal atPercentage = 0m;

                    foreach(var psr in psrRecords.Entities)
                    {
                        var projectER = psr.GetAttributeValue<EntityReference>("dfa_projectid");
                        if (projectER != null && projectER.Id != projectId)
                        {
                            projectId = projectER.Id;
                            project = Service.Retrieve("dfa_project", projectER.Id, new ColumnSet(true));
                            projectName = project.GetAttributeValue<string>("dfa_projectname");
                            projectNumber = project.GetAttributeValue<string>("dfa_projectnumber");
                            projectApprovedCost = project.GetAttributeValue<Money>("dfa_approvedcost");
                            var atPercentageNullable = project.GetAttributeValue<Nullable<decimal>>("dfa_approvedpercentages");
                            if (atPercentageNullable == null)
                            {
                                atPercentage = 80m;
                            }
                            else
                            {
                                atPercentage = atPercentageNullable.Value;
                            }
                            Tracing.Trace($"{CustomCodeHelper.LineNumber()}: {projectNumber} - {projectName} at {atPercentage}% of Total Approved {projectApprovedCost}");
                        }

                        var amount = psr.GetAttributeValue<Money>("dfa_amount");
                        Tracing.Trace($"{CustomCodeHelper.LineNumber()}: PSR Amount {amount}");
                        var subTotal = amount.Value;
                        if (subTotal < 0)
                        {
                            subTotal = 0m;
                        }
                        Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Subtotal Amount {subTotal}");
                        var totalAfterPercentCut = subTotal * atPercentage / 100m;
                        Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Total (Subtotal @ {atPercentage}%: {totalAfterPercentCut}");
                        var psrWrite = new Entity("dfa_projectstatusreport");
                        psrWrite["dfa_projectstatusreportid"] = psr.Id;
                        psrWrite["dfa_less1000"] = null;
                        psrWrite["dfa_subtotal"] = new Money(subTotal);
                        psrWrite["dfa_atpercent"] = atPercentage;
                        psrWrite["dfa_totalafterpercentcut"] = new Money(totalAfterPercentCut);
                        Tracing.Trace($"{CustomCodeHelper.LineNumber()} - PSR: {psr.Id} - {psr.GetAttributeValue<string>("dfa_name")}");
                        Service.Update(psrWrite);
                    }

                }
            }
            catch (InvalidPluginExecutionException) { throw; }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException($"Unexpected error. {ex.Message}", ex);
            }
        }
    }
}

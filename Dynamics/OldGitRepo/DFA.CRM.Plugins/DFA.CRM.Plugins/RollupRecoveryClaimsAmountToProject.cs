using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DFA.CRM.Plugins
{
    /// <summary>
    /// Rollup Claim Amount, Approved Amounts to Project
    /// </summary>
    public class RollupRecoveryClaimsAmountToProject : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            //Extract the tracing service for use in debugging sandboxed plug-ins.
            var Tracing =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            // Obtain the execution context from the service provider.
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            Tracing.Trace("context");
            // Obtain the organization service reference.
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            Tracing.Trace("servicefactory");
            var Service = serviceFactory.CreateOrganizationService(context.UserId);
            Tracing.Trace("contextuser" + context.UserId.ToString());
            Tracing.Trace("service" + Service.ToString());
            Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Message Name: {context.MessageName}");
            try
            {
                EntityCollection recoveryClaims = null;
                EntityReference projectER = null;

                if (context.MessageName == "Delete")
                {
                    Entity recordBefore = (Entity)context.PreEntityImages["preImage"];
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Deleting Recovery Claims");
                    if (recordBefore == null)
                    {
                        Tracing.Trace($"{CustomCodeHelper.LineNumber()}: preImage is missing");
                        return;
                    }
                    if (!recordBefore.Attributes.Contains("dfa_recoveryplanid"))
                    {
                        Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Record does not contains dfa_recoveryplanid");
                        return; // Nothing can be done if there is no Case to update
                    }
                    projectER = recordBefore.GetAttributeValue<EntityReference>("dfa_recoveryplanid");
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Project ER: {projectER.Id} - {projectER.Name}");
                    recoveryClaims = GetRecoveryClaimsByProject(Service, Tracing, projectER, recordBefore.ToEntityReference());
                }
                else
                {
                    Entity record = (Entity)context.PostEntityImages["postImage"];
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Creating or Updating Applicants");
                    if (record == null)
                    {
                        Tracing.Trace($"{CustomCodeHelper.LineNumber()}: postImage is missing");
                        return;
                    }
                    if (!record.Attributes.Contains("dfa_recoveryplanid"))
                    {
                        Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Record does not contains dfa_recoveryplanid");
                        return;
                    }
                    projectER = record.GetAttributeValue<EntityReference>("dfa_recoveryplanid");
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: projectER ==> {projectER.Id} - {projectER.Name}");
                    recoveryClaims = GetRecoveryClaimsByProject(Service, Tracing, projectER, null);
                }

                // Check Project Status.
                var projectRead = Service.Retrieve("dfa_project", projectER.Id, new ColumnSet(true));
                var projectStateCodeOS = projectRead.GetAttributeValue<OptionSetValue>("statecode");
                if (projectStateCodeOS != null && projectStateCodeOS.Value != 0) // Not Active
                {
                    return; // Won't be able to update Project because of its status.
                    // There is a workflow that does the same thing on Project Status changes.
                }

                var totalPaid = 0m;
                var approvedPercentageNullableDecimal = projectRead.GetAttributeValue<Nullable<decimal>>("dfa_approvedpercentages");
                var projectApprovedCostMoney = projectRead.GetAttributeValue<Money>("dfa_approvedcost");
                var projectApprovedCost = 0m;
                if (projectApprovedCostMoney != null)
                {
                    projectApprovedCost = projectApprovedCostMoney.Value;   
                }
                var approvedPercentage = 80m; // Default to 80%
                if (approvedPercentageNullableDecimal != null)
                {
                    // Otherwise, use the default percentage
                    approvedPercentage = approvedPercentageNullableDecimal.Value;
                }
                var claimAmount = 0m;
                //var actualInvoiceAmount = 0m;
                //var embcApprovedAmount = 0m;
                foreach(var claim in recoveryClaims.Entities)
                {
                    var recordClaimAmount = claim.GetAttributeValue<Money>("dfa_claimamount");
                    var isFirstClaim = claim.GetAttributeValue<bool>("dfa_isfirstclaim");
                    
                    if (recordClaimAmount != null)
                    {
                        claimAmount += recordClaimAmount.Value;
                        var fullAmount = recordClaimAmount.Value;
                        if (isFirstClaim)
                        {
                            fullAmount = fullAmount - 1000m;
                        }
                        var paidAmountAfterPercentDeduction = fullAmount * approvedPercentage / 100m;
                        totalPaid += paidAmountAfterPercentDeduction;
                    }
                }

                // There should be no negative with total paid
                if (totalPaid < 0)
                {
                    totalPaid = 0m;
                }

                var trueForecast = projectApprovedCost - totalPaid;

                // There should be no negative with total forecast
                if (trueForecast < 0)
                {
                    trueForecast = 0m;
                }
                
                Entity projectRecord = new Entity("dfa_project");
                projectRecord["dfa_projectid"] = projectER.Id;
                projectRecord["dfa_actualcostwithtax"] = claimAmount;
                projectRecord["dfa_totalpaidafterpercent"] = new Money(totalPaid);
                projectRecord["dfa_trueforecast"] = new Money(trueForecast);
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Updating Trace Record");
                Service.Update(projectRecord);
            }
            catch (Exception ex)
            {
                var error = "{\"error\":\"" + ex.Message + "|" + ex.Source + "|" + ex.StackTrace + "\"}";
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: {error}");
            }

        }

        public EntityCollection GetRecoveryClaimsByProject(IOrganizationService Service, ITracingService Tracing, EntityReference projectER, EntityReference excludedRecordER)
        {
            var query = new QueryExpression("dfa_projectclaim") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0); // Active
            query.Criteria.AddCondition("dfa_recoveryplanid", ConditionOperator.Equal, projectER.Id);
            query.Criteria.AddCondition("dfa_claimpaiddate", ConditionOperator.NotNull);
            if (excludedRecordER != null)
            {
                query.Criteria.AddCondition("dfa_projectclaimid", ConditionOperator.NotEqual, excludedRecordER.Id);
            }
            var results = Service.RetrieveMultiple(query);
            return results;
        }
    }
}
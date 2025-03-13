using System;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Security.Principal;
using System.Text;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DFA.CRM.Plugins
{
    /// <summary>
    /// DFA - Populate Total Projects and Claims Approved Amounts in Loc Gov Case
    /// </summary>
    public class PopulateTotalProjectsAndClaimsApprovedAmountsInCase : IPlugin
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
            var service = serviceFactory.CreateOrganizationService(context.UserId);
            Tracing.Trace("contextuser" + context.UserId.ToString());
            Tracing.Trace("service" + service.ToString());
            Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Message Name: {context.MessageName}");

            try
            {
                // The InputParameters collection contains all the data passed in the message request.  
                if (context.InputParameters.Contains("Target") &&
                    (context.InputParameters["Target"] is Entity) &&
                    (context.MessageName == "Update"))
                {

                    Guid regardingCaseId = Guid.Empty;
                    Entity parentEntity = (Entity)context.InputParameters["Target"];

                    Money totalEMBCApprovedAmount = new Money(0);
                    Money totalProvincialEligibleAmount = new Money(0);
                    Money less1000 = new Money(0);
                    Money claimsTotalPayable = new Money(0);

                    //Only consider three fields changed scenarios: status code, approved cost, actual cost
                    if ((parentEntity != null) &&
                            (parentEntity.Attributes.Contains("statuscode") ||
                            parentEntity.Attributes.Contains("dfa_approvedcost") ||
                            parentEntity.Attributes.Contains("dfa_actualcostwithtax") ||
                            parentEntity.Attributes.Contains("dfa_approvedpercentages")))
                    {
                        Entity entityProject = service.Retrieve("dfa_project", parentEntity.Id, new ColumnSet("dfa_caseid"));
                        if (entityProject.Attributes.Contains("dfa_caseid"))
                        {
                            regardingCaseId = ((EntityReference)entityProject["dfa_caseid"]).Id;

                            // Retrieve all approved projects with regarding case is current case
                            var queryExpression = new QueryExpression("dfa_project");
                            var qeFilter = new FilterExpression(LogicalOperator.And);
                            qeFilter.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, 222710001));//Approved status
                            qeFilter.AddCondition(new ConditionExpression("dfa_caseid", ConditionOperator.Like, regardingCaseId));
                            queryExpression.Criteria = qeFilter;
                            queryExpression.ColumnSet = new ColumnSet("dfa_approvedcost", "dfa_actualcostwithtax", "dfa_approvedpercentages");

                            //Get results:
                            var result = service.RetrieveMultiple(queryExpression);

                            //If no approved project, set all total amount to 0.
                            if (result.Entities.Count == 0)
                            {
                                totalEMBCApprovedAmount.Value = 0;
                                totalProvincialEligibleAmount.Value = 0;
                                less1000.Value = 0;
                                claimsTotalPayable.Value = 0;
                            }

                            //If there is/are approved project(s), calc sum amount
                            else
                            {
                                foreach (var prj in result.Entities)
                                {
                                    if (prj.Attributes.Contains("dfa_approvedcost")) totalEMBCApprovedAmount.Value += ((Money)prj["dfa_approvedcost"]).Value;
                                    if (prj.Attributes.Contains("dfa_actualcostwithtax")) totalProvincialEligibleAmount.Value += ((Money)prj["dfa_actualcostwithtax"]).Value;
                                    if (prj.Attributes.Contains("dfa_actualcostwithtax") && ((Money)prj["dfa_actualcostwithtax"]).Value >= 1000)
                                        less1000.Value += ((Money)prj["dfa_actualcostwithtax"]).Value - 1000;
                                    if (prj.Attributes.Contains("dfa_approvedpercentages") && ((Money)prj["dfa_actualcostwithtax"]).Value >= 1000)
                                        claimsTotalPayable.Value += (((Money)prj["dfa_actualcostwithtax"]).Value - 1000) * (((Decimal)prj["dfa_approvedpercentages"]) / 100);
                                }
                            }                            
                        }
                    }
                    else
                    {
                        Tracing.Trace($"{CustomCodeHelper.LineNumber()}");
                        return;
                    }

                    var caseRead = service.Retrieve("incident", regardingCaseId, new ColumnSet("statecode"));
                    if (caseRead == null)
                    {
                        return; // If Case not found, then nothing to update
                    }

                    var statecode = caseRead.GetAttributeValue<OptionSetValue>("statecode").Value;
                    if (statecode != 0)
                    {
                        return; // If Case is already closed, then nothing to update
                    }

                    //Update Total Approved Cost
                    Entity caseRecord = new Entity("incident");
                    caseRecord.Id = regardingCaseId;
                    caseRecord["dfa_embcapprovedamount"] = totalEMBCApprovedAmount;
                    caseRecord["dfa_provincialeligibleamounttotal"] = totalProvincialEligibleAmount;
                    caseRecord["dfa_less1000deductable"] = less1000;
                    caseRecord["dfa_claimstotalpayable80"] = claimsTotalPayable;
                    service.Update(caseRecord);
                }
                else
                {
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}");
                    return;
                }
            }
            catch (Exception ex)
            {
                var error = "{\"error\":\"" + ex.Message + "|" + ex.Source + "|" + ex.StackTrace + "\"}";
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: {error}");
            }

        }
    }
}
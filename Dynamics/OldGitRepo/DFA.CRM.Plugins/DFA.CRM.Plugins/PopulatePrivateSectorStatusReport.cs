using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DFA.CRM.Plugins
{
    /// <summary>
    /// This is to populate Private Sector Weekly Report Number
    /// </summary>
    public class PopulatePrivateSectorStatusReport : IPlugin
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
                if (context.MessageName != "Create")
                {
                    // Do Nothing
                    return;
                }

                var entTarget = (Entity)context.InputParameters["Target"];

                var record = Service.Retrieve(entTarget.LogicalName, entTarget.Id, new ColumnSet("dfa_eventid"));


                var eventER = record.GetAttributeValue<EntityReference>("dfa_eventid");

                // App Received to Date in System
                // Count all active cases for private sector where applicant type is NOT indigenous/local government body
                var activeCaseQuery = new QueryExpression("incident") { ColumnSet = new ColumnSet(true) };
                activeCaseQuery.Criteria.AddCondition("dfa_applicanttype", ConditionOperator.NotEqual, 222710005);
                activeCaseQuery.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0); // Active Cases
                if (eventER != null)
                {
                    activeCaseQuery.Criteria.AddCondition("dfa_eventid", ConditionOperator.Equal, eventER.Id);
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Active Case Filter By Event: {eventER.Id} - {eventER.Name}");
                }
                var activeCaseResults = Service.RetrieveMultiple(activeCaseQuery);
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Total Active Cases: {activeCaseResults.Entities.Count}");
                var assessingElibility = 0; // Date Assigned to FM (dfa_dateassignedtofm) does not contain data
                var assignedToFieldManager = 0; // Date Assigned to FM (dfa_dateassignedtofm) contains data, AND Date Report Completed (dfa_datereportcompleted) does not contain data
                var fieldManagerCompletedWork = 0; // Date Report Completed (dfa_datereportcompleted) contains data
                
                var totalActiveCases = activeCaseResults.Entities.Count;
                var totalPaid = 0m;
                foreach (var application in activeCaseResults.Entities)
                {
                    if (application.GetAttributeValue<Nullable<DateTime>>("dfa_dateassignedtofm") == null)
                    {
                        assessingElibility++;
                    }
                    else if (application.GetAttributeValue<Nullable<DateTime>>("dfa_dateassignedtofm") != null)
                    {
                        assignedToFieldManager++;
                    }
                    
                    if (application.GetAttributeValue<Nullable<DateTime>>("dfa_datereportcompleted") != null)
                    {
                        fieldManagerCompletedWork++;
                    }
                    var totalPaidActive = application.GetAttributeValue<Money>("dfa_totalpaid");
                    if (totalPaidActive != null)
                    {
                        totalPaid += totalPaidActive.Value;
                    }
                }

                // Count all inactive for private sector where applicant type is NOT indigenous/local government body
                var inactiveCaseQuery = new QueryExpression("incident") { ColumnSet = new ColumnSet(true) };
                inactiveCaseQuery.Criteria.AddCondition("dfa_applicanttype", ConditionOperator.NotEqual, 222710005);
                inactiveCaseQuery.Criteria.AddCondition("statecode", ConditionOperator.NotEqual, 0); // Closed Cases
                if (eventER != null)
                {
                    inactiveCaseQuery.Criteria.AddCondition("dfa_eventid", ConditionOperator.Equal, eventER.Id);
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Inctive Case Filter By Event: {eventER.Id} - {eventER.Name}");
                }
                var inactiveCaseResults = Service.RetrieveMultiple(inactiveCaseQuery);
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Total Inactive Cases: {inactiveCaseResults.Entities.Count}");

                var totalInactiveCases = inactiveCaseResults.Entities.Count;
                var fileClosedWithNoPayment = 0; // statecode is NOT 0 (not active) AND Total PAID (dfa_totalpaid) == 0
                var fileClosedWithPayment = 0; // statcode is NOT 0 (not active) AND Total PAID != null
                foreach (var application in inactiveCaseResults.Entities)
                {
                    var totalPaidInactive = application.GetAttributeValue<Money>("dfa_totalpaid");
                    if (totalPaidInactive == null || totalPaidInactive.Value <= 0)
                    {
                        fileClosedWithNoPayment++;
                    }
                    if (totalPaidInactive != null && totalPaidInactive.Value > 0)
                    {
                        fileClosedWithPayment++;
                        totalPaid += totalPaidInactive.Value;
                    }
                }


                
                record["dfa_totalapplications"] = totalActiveCases + totalInactiveCases;
                record["dfa_totalapplicationinprogress"] = totalActiveCases;
                record["dfa_totalpaid"] = new Money(totalPaid);
                record["dfa_assessingeligibility"] = assessingElibility;
                record["dfa_assignedtofm"] = assignedToFieldManager;
                record["dfa_receivedfmreport"] = fieldManagerCompletedWork;
                record["dfa_fileclosedwithoutpayment"] = fileClosedWithNoPayment;
                record["dfa_fileclosedwithpayment"] = fileClosedWithPayment;
                record["dfa_totalapplicationinprogressembc"] = totalActiveCases - assessingElibility - assignedToFieldManager + fieldManagerCompletedWork;
                record["dfa_datereported"] = DateTime.Now;
                record["dfa_name"] = "Private Sector Stats - " + DateTime.Now.ToString("MMM d, yyyy");

                Service.Update(record);
            }
            catch (Exception ex)
            {
                var error = "{\"error\":\"" + ex.Message + "|" + ex.Source + "|" + ex.StackTrace + "\"}";
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: {error}");
            }

        }
    }
}
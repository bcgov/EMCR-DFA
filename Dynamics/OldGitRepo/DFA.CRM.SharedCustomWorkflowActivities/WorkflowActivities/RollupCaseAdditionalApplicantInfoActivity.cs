using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using Microsoft.Crm.Sdk.Messages;

namespace DFA.CRM.CustomWorkflow
{
    /// <summary>
    /// To have cases to rollup all their additional application information.
    /// There is already a plugin for ongoing cases, 
    /// This workflow activity is targeting for those case that are already closed
    /// </summary>
    public class RollupCaseAdditionalApplicantInfoActivity : CodeActivity
    {
        [RequiredArgument]
        [Input("Case")]
        [ReferenceTarget("incident")]
        public InArgument<EntityReference> CaseInput { get; set; }

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
                var caseER = CaseInput.Get(activityContext);
                if (caseER == null)
                {
                    // Take Early Exit
                    return;
                }

                var caseRead = Service.Retrieve("incident", caseER.Id, new ColumnSet("statecode", "statuscode", "dfa_togglecontactinfopopulation", "title"));
                var currentStateCode = caseRead.GetAttributeValue<OptionSetValue>("statecode").Value;
                var currentStatusCode = caseRead.GetAttributeValue<OptionSetValue>("statuscode").Value;
                var caseTitle = caseRead.GetAttributeValue<string>("title");
                var toggleContactInfoFlag = caseRead.GetAttributeValue<bool>("dfa_togglecontactinfopopulation");
                var newToggleFlag = !toggleContactInfoFlag;
                var isInactive = false;
                var incidentToUpdate = new Entity("incident");
                incidentToUpdate["incidentid"] = caseER.Id;
                if (currentStateCode != 0) // NOT Active
                {
                    isInactive = true;
                    // Need to change status to become Active first.
                    // SetStateRequest has been deprecated.  Not sure if it still needed to be activated first before setting the toggle field.
                    incidentToUpdate["statecode"] = new OptionSetValue(0); // Active
                    incidentToUpdate["statuscode"] = new OptionSetValue(1); // Open
                }
                incidentToUpdate["dfa_togglecontactinfopopulation"] = newToggleFlag; // Toggle the flag.  This will trigger the plugin to update accordingly.
                Service.Update(incidentToUpdate);

                // If inactive, then we need to change the status back to whatever
                if (isInactive)
                {

                    // Create the incident's resolution.
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Generate a Incident Resolution entity record?  Don't have to save?");
                    var incidentResolution = new Entity("incidentresolution");

                    incidentResolution["subject"] = "Case Closed - " + caseTitle;
                    incidentResolution["incidentid"] = caseER;
                    incidentResolution["timespent"] = 0;
                    incidentResolution["description"] = "Data Fix for rolling up additional applicant contact info";

                    // Close the incident with the resolution.
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Generate a Close Incident Request");
                    var closeIncidentRequest = new CloseIncidentRequest
                    {
                        IncidentResolution = incidentResolution,
                        Status = new OptionSetValue(currentStatusCode),
                    };
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Execute the Close Incident Request");
                    Service.Execute(closeIncidentRequest);
                }
            }
            catch (InvalidPluginExecutionException) { throw; }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException($"Unexpected error. {ex.Message}|{ex.StackTrace}|{ex.Source}", ex);
            }
        }
    }
}

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Linq;

namespace DFA.CRM.CustomWorkflow
{
    /// <summary>
    /// Dynamics Workflow can only access information up to the next relationship
    /// This will allow any child entity of Case to access information from the related Event
    /// </summary>
    public class GetEventByCase : CodeActivity
    {
        [RequiredArgument]
        [Input("Case")]
        [ReferenceTarget("incident")]
        public InArgument<EntityReference> CaseInput { get; set; }

        [Output("Event")]
        [ReferenceTarget("dfa_event")]
        public OutArgument<EntityReference> EventOutput { get; set; }

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
                EventOutput.Set(activityContext, null);
                var caseER = CaseInput.Get(activityContext);
                if (caseER == null)
                {
                    // Take Early Exit
                    return;
                }

                var caseEntity = Service.Retrieve(caseER.LogicalName, caseER.Id, new ColumnSet("dfa_eventid"));
                if (caseEntity == null)
                {
                    return;
                }

                var eventER = caseEntity.GetAttributeValue<EntityReference>("dfa_eventid");
                EventOutput.Set(activityContext, eventER);
            }
            catch (InvalidPluginExecutionException) { throw; }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException($"Unexpected error. {ex.Message}", ex);
            }
        }
    }
}

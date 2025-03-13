using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Linq;

namespace DFA.CRM.CustomWorkflow
{
    /// <summary>
    /// 
    /// </summary>
    public class FormatHistoryComments : CodeActivity
    {
        [RequiredArgument]
        [Input("History Comments")]
        public InArgument<string> OldComments { get; set; }

        [Output("Formatted Comments")]
        public OutArgument<string> NewComments { get; set; }

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

                string historyComments = OldComments.Get(activityContext);

                string correctString = historyComments.Replace("Jan", "#Jan").Replace("Feb", "#Feb").Replace("Mar", "#Mar").Replace("Apr", "#Apr").Replace("May", "#May").Replace("Jun", "#Jun").Replace("Jul", "#Jul").Replace("Aug", "#Aug").Replace("Sep", "#Sep").Replace("Oct", "#Oct").Replace("Nov", "#Nov").Replace("Dec", "#Dec");
                string[] words = correctString.Split('#');
                string finalString = "";

                foreach (var word in words)
                {
                    finalString += $"{word}";
                    finalString += "\n";
                }

                NewComments.Set(activityContext, finalString);
            }
            catch (InvalidPluginExecutionException) { throw; }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException($"Unexpected error. {ex.Message}", ex);
            }
        }
    }
}
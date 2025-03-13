using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Linq;

namespace DFA.CRM.CustomWorkflow
{
    /// <summary>
    /// Queue are Org basis and cannot be copied from one Org to another, 
    /// which means Queue defined in DEV environment does not equal to the Queue defined in TEST or PROD environment
    /// Despite having the same name.  This custom workflow activity is to obtain the correct Queue Entity Reference
    /// </summary>
    public class GetQueueByNameActivity : CodeActivity
    {
        [RequiredArgument]
        [Input("Queue Name")]
        public InArgument<string> QueueName { get; set; }

        [Output("Queue")]
        [ReferenceTarget("queue")]
        public OutArgument<EntityReference> Queue { get; set; }

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

                var queueName = QueueName.Get(activityContext);
                var query = new QueryExpression("queue") { ColumnSet = new ColumnSet(true) };
                query.Criteria.AddCondition("name", ConditionOperator.Equal, queueName);
                var results = Service.RetrieveMultiple(query);

                if (results.Entities.Any())
                {
                    Queue.Set(activityContext, results.Entities[0].ToEntityReference());
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

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Linq;

namespace DFA.CRM.CustomWorkflow
{
    /// <summary>
    /// Queue Items may not be closed properly at times.
    /// This activity enforced Queue Items is removed
    /// </summary>
    public class RemoveItemFromQueueActivity : CodeActivity
    {
        [RequiredArgument]
        [Input("Queue Name")]
        public InArgument<string> QueueName { get; set; }

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
                var primaryEntityId = Workflow.PrimaryEntityId;
                var primaryEntityName = Workflow.PrimaryEntityName;

                var query = new QueryExpression("queue") { ColumnSet = new ColumnSet(true) };
                query.Criteria.AddCondition("name", ConditionOperator.Equal, queueName);
                var results = Service.RetrieveMultiple(query);
                if (results == null || !results.Entities.Any())
                {
                    Tracing.Trace("No Queues returned");
                    return;
                }
                var queue = results.Entities[0].ToEntityReference();

                if (queue == null)
                {
                    Tracing.Trace("Queue Entity Reference undefined");
                    return;
                }

                var itemQuery = new QueryExpression("queueitem") { ColumnSet = new ColumnSet(false) };
                itemQuery.Criteria.AddCondition("objectid", ConditionOperator.Equal, primaryEntityId);
                itemQuery.Criteria.AddCondition("queueid", ConditionOperator.Equal, queue.Id);
                var resultItems = Service.RetrieveMultiple(itemQuery);
                Tracing.Trace("Retrieved Queue Item");
                foreach (var item in resultItems.Entities)
                {
                    Tracing.Trace($"Delete Queue Item ID: {item.Id}");
                    Service.Delete("queueitem", item.Id);
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
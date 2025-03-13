using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Linq;

namespace DFA.CRM.ApplicationConversions
{
    /// <summary>
    /// Get Document location by Application
    /// </summary>
    public class GetDocumentLocationByAppApplication : CodeActivity
    {
        [RequiredArgument]
        [Input("Application")]
        [ReferenceTarget("dfa_appapplication")]
        public InArgument<EntityReference> ApplicationInput { get; set; }

        [Output("Document Location")]
        [ReferenceTarget("sharepointdocumentlocation")]
        public OutArgument<EntityReference> DocumentLocationOutput { get; set; }

        [Output("Has Found Result")]
        public OutArgument<bool> HasFoundResultOutput { get; set; }

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
                DocumentLocationOutput.Set(activityContext, null);
                HasFoundResultOutput.Set(activityContext, false);
                var applicationER = ApplicationInput.Get(activityContext);
                if (applicationER == null)
                {
                    // Take Early Exit
                    return;
                }

                var query = new QueryExpression("sharepointdocumentlocation") { ColumnSet = new ColumnSet(false) };
                query.TopCount = 1;
                query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0); // Active
                query.Criteria.AddCondition("regardingobjectid", ConditionOperator.Equal, applicationER.Id);
                query.AddOrder("modifiedon", OrderType.Descending); // Last Modified
                var results = Service.RetrieveMultiple(query);
                if (results.Entities.Count > 0)
                {
                    DocumentLocationOutput.Set(activityContext, results.Entities[0].ToEntityReference());
                    HasFoundResultOutput.Set(activityContext, true);
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

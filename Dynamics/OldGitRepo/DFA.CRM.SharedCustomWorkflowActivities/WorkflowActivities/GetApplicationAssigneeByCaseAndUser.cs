using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Linq;

namespace DFA.CRM.CustomWorkflow
{
    /// <summary>
    /// Get Application Assignee By Case and User
    /// </summary>
    public class GetApplicationAssigneeByCaseAndUser : CodeActivity
    {
        [RequiredArgument]
        [Input("Case")]
        [ReferenceTarget("incident")]
        public InArgument<EntityReference> CaseInput { get; set; }

        [RequiredArgument]
        [Input("User")]
        [ReferenceTarget("systemuser")]
        public InArgument<EntityReference> UserInput { get; set; }

        [Input("Exclude Assignee")]
        [ReferenceTarget("dfa_applicationassignee")]
        public InArgument<EntityReference> AssigneeInput { get; set; }

        [Output("Application Assignee")]
        [ReferenceTarget("dfa_applicationassignee")]
        public OutArgument<EntityReference> AssigneeOutput { get; set; }

        [Output("Record Exists")]
        public OutArgument<bool> RecordExistsOutput { get; set; }

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
                RecordExistsOutput.Set(activityContext, false);
                AssigneeOutput.Set(activityContext, null);
                var caseER = CaseInput.Get(activityContext);
                var userER = UserInput.Get(activityContext);
                var excludeAssigneeER = AssigneeInput.Get(activityContext);
                if (caseER == null || userER == null)
                {
                    // Take Early Exit
                    return;
                }

                var query = new QueryExpression("dfa_applicationassignee") { ColumnSet = new ColumnSet(false) };
                query.Criteria.AddCondition("dfa_caseid", ConditionOperator.Equal, caseER.Id);
                query.Criteria.AddCondition("dfa_assignedtoid", ConditionOperator.Equal, userER.Id);
                if (excludeAssigneeER != null)
                {
                    query.Criteria.AddCondition("dfa_applicationassigneeid", ConditionOperator.NotEqual, excludeAssigneeER.Id);
                }
                query.TopCount = 1;
                query.AddOrder("createdon", OrderType.Descending); // Last created
                var results = Service.RetrieveMultiple(query);
                if (results.Entities.Count > 0)
                {
                    RecordExistsOutput.Set(activityContext, true);
                    var entityReference = results.Entities[0].ToEntityReference();
                    AssigneeOutput.Set(activityContext, entityReference);
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

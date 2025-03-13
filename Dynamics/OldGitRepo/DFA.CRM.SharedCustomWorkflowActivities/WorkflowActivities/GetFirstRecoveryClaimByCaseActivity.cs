using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Linq;

namespace DFA.CRM.CustomWorkflow
{
    /// <summary>
    /// Get First Recovery Claim By Case
    /// </summary>
    public class GetFirstRecoveryClaimByCaseActivity : CodeActivity
    {
        [RequiredArgument]
        [Input("Case")]
        [ReferenceTarget("incident")]
        public InArgument<EntityReference> CaseInput { get; set; }

        [Output("First Claim")]
        [ReferenceTarget("dfa_projectclaim")]
        public OutArgument<EntityReference> RecoveryClaimOutput { get; set; }

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
                RecoveryClaimOutput.Set(activityContext, null);
                var caseER = CaseInput.Get(activityContext);
                if (caseER == null)
                {
                    // Take Early Exit
                    return;
                }

                var query = new QueryExpression("dfa_projectclaim") { ColumnSet = new ColumnSet(false) };
                query.Criteria.AddCondition("dfa_caseid", ConditionOperator.Equal, caseER.Id);
                query.Criteria.AddCondition("dfa_isfirstclaim", ConditionOperator.Equal, true);
                query.AddOrder("modifiedon", OrderType.Descending);
                var results = Service.RetrieveMultiple(query);
                if (results.Entities.Count > 0)
                {
                    RecordExistsOutput.Set(activityContext, true);
                    var entityReference = results.Entities[0].ToEntityReference();
                    RecoveryClaimOutput.Set(activityContext, entityReference);
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

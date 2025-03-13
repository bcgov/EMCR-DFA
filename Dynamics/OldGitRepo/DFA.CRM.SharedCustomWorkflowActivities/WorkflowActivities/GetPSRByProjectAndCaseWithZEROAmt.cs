using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Linq;

namespace DFA.CRM.CustomWorkflow
{
    /// <summary>
    /// Get PSR By Project and Case with $0 Amt - Placeholder PSR for the Project
    /// </summary>
    public class GetPSRByProjectAndCaseWithZEROAmt : CodeActivity
    {
        [RequiredArgument]
        [Input("Case")]
        [ReferenceTarget("incident")]
        public InArgument<EntityReference> CaseInput { get; set; }

        [RequiredArgument]
        [Input("Project")]
        [ReferenceTarget("dfa_project")]
        public InArgument<EntityReference> ProjectInput { get; set; }

        [Output("PSR")]
        [ReferenceTarget("dfa_projectstatusreport")]
        public OutArgument<EntityReference> PSROutput { get; set; }

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
                PSROutput.Set(activityContext, null);
                var caseER = CaseInput.Get(activityContext);
                var projectER = ProjectInput.Get(activityContext);
                if (caseER == null || projectER == null)
                {
                    // Take Early Exit
                    return;
                }

                var query = new QueryExpression("dfa_projectstatusreport") { ColumnSet = new ColumnSet(false) };
                query.Criteria.AddCondition("dfa_amount", ConditionOperator.Equal, 0m);
                query.Criteria.AddCondition("dfa_caseid", ConditionOperator.Equal, caseER.Id);
                query.Criteria.AddCondition("dfa_projectid", ConditionOperator.Equal, projectER.Id);
                query.TopCount = 1;
                query.AddOrder("createdon", OrderType.Descending); // Last created
                var results = Service.RetrieveMultiple(query);
                if (results.Entities.Count > 0)
                {
                    var psrER = results.Entities[0].ToEntityReference();
                    PSROutput.Set(activityContext, psrER);
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

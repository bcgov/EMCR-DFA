using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;

namespace DFA.CRM.CustomWorkflow
{
    /// <summary>
    /// Cancel Waiting Process for this record
    /// </summary>
    public class CancelWaitingProcessActivity : CodeActivity
    {
        [RequiredArgument]
        [Input("Process Name")]
        public InArgument<string> ProcessNameInput { get; set; }

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

                var entityLogicalName = Workflow.PrimaryEntityName;
                var primaryEntityId = Workflow.PrimaryEntityId;
                var processName = ProcessNameInput.Get(activityContext);

                var query = new QueryExpression("asyncoperation") { ColumnSet = new ColumnSet(true) };
                query.Criteria.AddCondition("regardingobjectid", ConditionOperator.Equal, primaryEntityId);
                query.Criteria.AddCondition("name", ConditionOperator.Equal, processName);
                query.Criteria.AddCondition("statuscode", ConditionOperator.Equal, 10); // Waiting

                var results = Service.RetrieveMultiple(query);

                foreach(var job in results.Entities)
                {
                    job["statecode"] = new OptionSetValue(3);
                    job["statuscode"] = new OptionSetValue(32);       // Cancelled
                    Service.Update(job);
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
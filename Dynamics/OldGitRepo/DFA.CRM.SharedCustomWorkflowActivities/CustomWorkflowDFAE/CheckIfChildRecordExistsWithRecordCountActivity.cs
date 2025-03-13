using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;

namespace DFA.CRM.CustomWorkflow
{
    /// <summary>
    /// Search to check if records exists
    /// </summary>
    public class CheckIfChildRecordExistsWithRecordCountActivity : CodeActivity
    {
        [RequiredArgument]
        [Input("Child Entity Logical Name")]
        public InArgument<string> EntityNameInput { get; set; }

        [RequiredArgument]
        [Input("Search Attribute Key")]
        public InArgument<string> AttributeKeyInput { get; set; }

        [Input("Include Inactive Record in Search")]
        public InArgument<bool> IncludeInactiveInput { get; set; }

        // true if no records in the entity are found given that field name and value
        [Output("Record Exists")]
        public OutArgument<bool> RecordExistsOutput { get; set; }

        [Output("Record Count")]
        public OutArgument<int> RecordCountOutput { get; set; }

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

                string entityName = EntityNameInput.Get(activityContext).ToString();
                string fieldName = AttributeKeyInput.Get(activityContext).ToString();
                var includeInactive = IncludeInactiveInput.Get(activityContext);
                var primaryEntityId = Workflow.PrimaryEntityId;
                RecordCountOutput.Set(activityContext, 0);
                if (!string.IsNullOrEmpty(entityName) && !string.IsNullOrEmpty(fieldName))
                {
                    QueryExpression query = new QueryExpression(entityName);
                    query.ColumnSet = new ColumnSet(true);
                    query.Criteria.AddCondition(fieldName, ConditionOperator.Equal, primaryEntityId);
                    if (!includeInactive)
                    {
                        query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0); // Active Only
                    }

                    var entities = Service.RetrieveMultiple(query);

                    if (entities != null && entities.Entities.Count > 0)
                    {
                        RecordExistsOutput.Set(activityContext, true);
                        Tracing.Trace("Record found");
                        RecordCountOutput.Set(activityContext, entities.Entities.Count);
                    }

                    if (entities == null || entities.Entities.Count == 0)
                    {
                        RecordExistsOutput.Set(activityContext, false);
                        Tracing.Trace("Record not found");
                    }
                    RecordCountOutput.Set(activityContext, entities.Entities.Count);
                }
                else
                {
                    RecordExistsOutput.Set(activityContext, false);
                    throw new Exception("Invalid entity or field name");
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
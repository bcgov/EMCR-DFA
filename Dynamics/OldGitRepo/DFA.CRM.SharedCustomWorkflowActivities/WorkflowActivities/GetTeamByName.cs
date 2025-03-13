using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Linq;

namespace DFA.CRM.CustomWorkflow
{
    /// <summary>
    /// Team are Org basis and cannot be copied from one Org to another, 
    /// which means Team defined in DEV environment does not equal to the Team defined in TEST or PROD environment
    /// Despite having the same name.  This custom workflow activity is to obtain the correct Team Entity Reference
    /// </summary>
    public class GetTeamByNameActivity : CodeActivity
    {
        [RequiredArgument]
        [Input("Team Name")]
        public InArgument<string> TeamNameInput { get; set; }

        [RequiredArgument]
        [AttributeTarget("team", "teamtype")]
        [Input("Team Type")]
        public InArgument<OptionSetValue> TeamTypeInput { get; set; }

        [Input("Business Unit Name")]
        public InArgument<string> BusinessUnitNameInput { get; set; }

        [Output("Team")]
        [ReferenceTarget("team")]
        public OutArgument<EntityReference> TeamOutput { get; set; }

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

                var teamName = TeamNameInput.Get(activityContext);
                var teamType = TeamTypeInput.Get(activityContext);
                var businessUnitName = BusinessUnitNameInput.Get(activityContext);

                EntityReference businessUnitER = null;

                if (!string.IsNullOrEmpty(businessUnitName))
                {
                    var query = new QueryExpression("businessunit") { ColumnSet = new ColumnSet(false) };
                    query.Criteria.AddCondition("name", ConditionOperator.Equal, businessUnitName);
                    query.TopCount = 1;
                    var results = Service.RetrieveMultiple(query);

                    if (results.Entities.Any())
                    {
                        businessUnitER = results.Entities[0].ToEntityReference();
                    }
                }

                var teamQuery = new QueryExpression("team") { ColumnSet = new ColumnSet(false) };
                teamQuery.Criteria.AddCondition("teamtype", ConditionOperator.Equal, teamType.Value);
                teamQuery.Criteria.AddCondition("name", ConditionOperator.Equal, teamName);
                if (businessUnitER != null)
                {
                    teamQuery.Criteria.AddCondition("businessunitid", ConditionOperator.Equal, businessUnitER.Id);
                }
                teamQuery.TopCount = 1;
                var teamResults = Service.RetrieveMultiple(teamQuery);
                if (teamResults.Entities.Any())
                {
                    TeamOutput.Set(activityContext, teamResults.Entities[0].ToEntityReference());
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

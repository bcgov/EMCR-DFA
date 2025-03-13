using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Linq;

namespace DFA.CRM.CustomWorkflow2
{
    /// <summary>
    /// Dynamics Workflow to get first sharepointdocumentlocation by any fetch
    /// </summary>
    public class GetEffectedregioncommunityByFetch : CodeActivity
    {
 
        [RequiredArgument]
        [Input("FetchXML Query")]
        public InArgument<string> FetchXmlQuery { get; set; }

        [Input("Query Argument 1")]
        public InArgument<string> FetchXmlQueryFormatArg1 { get; set; }

        [Input("Query Argument 2")]
        public InArgument<string> FetchXmlQueryFormatArg2 { get; set; }

        [Input("Query Argument 3")]
        public InArgument<string> FetchXmlQueryFormatArg3 { get; set; }

        [Input("Query Argument 4")]
        public InArgument<string> FetchXmlQueryFormatArg4 { get; set; }

        [Input("Query Argument 5")]
        public InArgument<string> FetchXmlQueryFormatArg5 { get; set; }

        [Input("Query Argument 6")]
        public InArgument<string> FetchXmlQueryFormatArg6 { get; set; }

        [Output("Record Count")]
        public OutArgument<int> RecordCount { get; set; }

        [Output("EffectedregioncommunityFound")]
        [ReferenceTarget("dfa_effectedregioncommunity")]
        public OutArgument<EntityReference> AreaFound { get; set; }

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

                string formattedFetchXml = GetFormattedFetchQuery(activityContext);
                Tracing.Trace("formattedFetchXml: " + formattedFetchXml);
                var recordsToProcess = Service.RetrieveMultiple(new FetchExpression(formattedFetchXml));
                this.RecordCount.Set(activityContext, recordsToProcess.Entities.Count);
                if (recordsToProcess.Entities.Count > 0)
                {
                    EntityReference erreference = new EntityReference("dfa_effectedregioncommunity", recordsToProcess.Entities[0].Id);
                    this.AreaFound.Set(activityContext, erreference);
                }
            }
            catch
            {
                this.RecordCount.Set(activityContext, -1);
            }
        }


        private string GetFormattedFetchQuery(CodeActivityContext context)
        {
            var query = this.FetchXmlQuery.Get(context);
            return String.Format(query,
                this.FetchXmlQueryFormatArg1.Get(context),
                this.FetchXmlQueryFormatArg2.Get(context),
                this.FetchXmlQueryFormatArg3.Get(context),
                this.FetchXmlQueryFormatArg4.Get(context),
                this.FetchXmlQueryFormatArg5.Get(context),
                this.FetchXmlQueryFormatArg6.Get(context));
        }

    }
}

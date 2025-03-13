using DFA.Portal.Custom.Actions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk.Workflow.Activities;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;

namespace DFA.Portal.CustomActivitiesDFAE
{
    public class DFADeleteSpDocumentByFetch : CodeActivity
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

        [Output("SP Document")]
        [ReferenceTarget("sharepointdocument")]
        public OutArgument<EntityReference> SpDocument { get; set; }

        [Output("Doc Guid")]
        public OutArgument<string> DocGuid { get; set; }


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
                Tracing.Trace("userid: " + Workflow.UserId.ToString());
                Tracing.Trace("formattedFetchXml: " + formattedFetchXml);
                var recordsToProcess = Service.RetrieveMultiple(new FetchExpression(formattedFetchXml));
                Tracing.Trace("count: " + recordsToProcess.Entities.Count.ToString());
                this.RecordCount.Set(activityContext, recordsToProcess.Entities.Count);
                if (recordsToProcess.Entities.Count > 0)
                {
                    List<Entity> sharePointConfigs = Helper.GetSystemConfigurations(Service, Constants.SharePoint.GROUP_NAME, null);
                    string userName = Helper.GetConfigKeyValue(sharePointConfigs, Constants.SharePoint.USERNAME, Constants.SharePoint.GROUP_NAME);
                    string password = Helper.GetSecureConfigKeyValue(sharePointConfigs, Constants.SharePoint.PASSWORD, Constants.SharePoint.GROUP_NAME);
                    string stsUrl = Helper.GetConfigKeyValue(sharePointConfigs, Constants.SharePoint.STS_URL, Constants.SharePoint.GROUP_NAME);
                    string relyingPartyId = Helper.GetConfigKeyValue(sharePointConfigs, Constants.SharePoint.RELYING_PARTY_ID, Constants.SharePoint.GROUP_NAME);
                    Tracing.Trace("got the username and password");

                    var applicationDocumentLocation = Helper.RetrieveEntitySharePointLocation(Service, AppApplication.ENTITY_NAME.ToLowerInvariant());
                    RetrieveAbsoluteAndSiteCollectionUrlRequest req1 = new RetrieveAbsoluteAndSiteCollectionUrlRequest
                    {
                        Target = applicationDocumentLocation
                    };

                    var applicationSiteCollection = (RetrieveAbsoluteAndSiteCollectionUrlResponse)Service.Execute(req1);

                    var sp = new SPService(stsUrl, applicationSiteCollection.SiteCollectionUrl, relyingPartyId, userName, password);
                    Tracing.Trace("sp got created");
                    sp.DeleteFile(recordsToProcess.Entities[0]["relativelocation"].ToString(), Tracing);

                    EntityReference erDoclocation = new EntityReference("sharepointdocument", recordsToProcess.Entities[0].Id);
                    this.SpDocument.Set(activityContext, erDoclocation);
                    this.DocGuid.Set(activityContext, recordsToProcess.Entities[0].Id.ToString());
                }
            }
            catch (Exception ex)
            {
                Tracing.Trace("ex: " + ex);
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


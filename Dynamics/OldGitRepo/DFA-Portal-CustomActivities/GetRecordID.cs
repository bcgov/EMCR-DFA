using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Linq;

namespace DFA.CRM.CustomWorkflow
{
    /// <summary>
    /// Dynamics Workflow to get first sharepointdocumentlocation by any fetch
    /// </summary>
    public class GetRecordID : CodeActivity
    {

        [RequiredArgument]
        [Input("Record URL")]
        [Default("")]
        public InArgument<String> RecordURL { get; set; }


        [Output("Record ID")]
        public OutArgument<string> RecordID { get; set; }

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

                String recordURL = this.RecordURL.Get(activityContext);

                string recordID = GetRecordIDmethod(recordURL);
                this.RecordID.Set(activityContext, recordID);
             
            }
            catch
            {  }
       
        }

        public string GetRecordIDmethod(string recordURL)
        {

            if (recordURL == null || recordURL == "")
            {
                return "";
            }
            string[] urlParts = recordURL.Split("?".ToArray());
            string[] urlParams = urlParts[1].Split("&".ToCharArray());
            string objectTypeCode = urlParams[0].Replace("etc=", "");
            //  entityName =  sGetEntityNameFromCode(objectTypeCode, service);
            string objectId = urlParams[1].Replace("id=", "");
            return objectId;
        }

    }
}

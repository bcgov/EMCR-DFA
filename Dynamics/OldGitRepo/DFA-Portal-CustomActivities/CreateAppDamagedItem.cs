using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Linq;


namespace DFA_Portal_CustomActivities
{
    public class CreateAppDamagedItem : CodeActivity
    {
        [Input("dfa_applicationid")]
        public InArgument<string> Application { get; set; }

        [Input("dfa_damagedescription")]
        public InArgument<string> DamagedDescription { get; set; }

        [Input("dfa_roomname")]
        public InArgument<string> RoomName { get; set; }
        [Input("dfa_comments")]
        public InArgument<string> Comments { get; set; }

        [Input("dfa_appdamageditemid")]

        public InArgument<string> AppDamagedItem { get; set; }
        [Input("delete")]

        public InArgument<bool> Delete { get; set; }
        [Input("dfa_name")]

        public InArgument<string> Name { get; set; }
        [Output("output")]
        public OutArgument<string> Output { get; set; }
        CodeActivityContext Activity;
        ITracingService Tracing;
        IWorkflowContext Workflow;
        IOrganizationServiceFactory ServiceFactory;
        IOrganizationService Service;


        protected override void Execute(CodeActivityContext context)
        {

            Activity = context;
            Tracing = Activity.GetExtension<ITracingService>();
            Workflow = Activity.GetExtension<IWorkflowContext>();
            ServiceFactory = Activity.GetExtension<IOrganizationServiceFactory>();
            Service = ServiceFactory.CreateOrganizationService(Workflow.UserId);
            // Get the input values
            string application = Application.Get(context);
            Tracing.Trace("application from GUID : " + application);
            string roomName = RoomName.Get(context);
            string name = Name.Get(context);
            string comments = Comments.Get(context);
            string appDamagedId = AppDamagedItem.Get(context);
            Tracing.Trace("appDamagedId from GUID : " + appDamagedId);
            string damagedDescription = DamagedDescription.Get(context);
            bool delete = Delete.Get(context);

            if (delete) 
            {
                Tracing.Trace("deleting the guid id : " + appDamagedId);
                Service.Delete("dfa_appdamageditem", new Guid(appDamagedId));
                Output.Set(context, "deleted");
                Tracing.Trace("deleted the guid id : " + Output);
                return;
            }

            if (!string.IsNullOrEmpty(appDamagedId))
            {
                Tracing.Trace("appDamagedItem id is provide and update is in progress");
                Entity appDamagedItem = new Entity("dfa_appdamageditem");
                appDamagedItem.Id = new Guid(appDamagedId);

                if (!string.IsNullOrEmpty(damagedDescription))
                {
                    appDamagedItem["dfa_damagedescription"] = damagedDescription;
                }
                if (!string.IsNullOrEmpty(name))
                {
                    appDamagedItem["dfa_name"] = name;
                }
                if (!string.IsNullOrEmpty(roomName))
                    appDamagedItem["dfa_roomname"] = roomName;
                if (!string.IsNullOrEmpty(comments))
                    appDamagedItem["dfa_comments"] = comments;

                if (!string.IsNullOrEmpty(application))
                {
                    appDamagedItem["dfa_applicationid"] = new EntityReference("dfa_appapplication", new Guid(application));
                }
                Service.Update(appDamagedItem);
                Output.Set(context, "Updated Successfully");
                Tracing.Trace("appDamagedItem is updated with guid:");

            }
            else
            {
                // Create
                Entity appDamagedItem = new Entity("dfa_appdamageditem");
                appDamagedItem["dfa_name"] = name;

                if (!string.IsNullOrEmpty(roomName))
                    appDamagedItem["dfa_roomname"] = roomName;
                if (!string.IsNullOrEmpty(damagedDescription))
                    appDamagedItem["dfa_damagedescription"] = damagedDescription;
                if (!string.IsNullOrEmpty(comments))
                    appDamagedItem["dfa_comments"] = comments;
                if (!string.IsNullOrEmpty(application))
                    appDamagedItem["dfa_applicationid"] = new EntityReference("dfa_appapplication", new Guid(application));

                var appDamagedItemId = Service.Create(appDamagedItem);
                Tracing.Trace("appDamagedItem created is : " + appDamagedItemId.ToString());
                Output.Set(context, appDamagedItemId.ToString());
            }
        }

    }
}

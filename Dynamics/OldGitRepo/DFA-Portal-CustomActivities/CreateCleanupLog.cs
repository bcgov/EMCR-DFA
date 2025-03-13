using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Linq;

namespace DFA_Portal_CustomActivities
{
    public class CreateCleanupLog : CodeActivity
    {
        [Input("dfa_applicationid")]
        public InArgument<string> Application { get; set; }

        [Input("dfa_contactid")] // name of the contact
        public InArgument<string> Contact { get; set; }
        [Input("dfa_name")]
        public InArgument<string> Name { get; set; }

        [Input("dfa_date")]
        public InArgument<DateTime> Date { get; set; }
        [Input("dfa_hoursworked")]
        public InArgument<int> HoursWorked { get; set; }

        [Input("dfa_appcleanuplogid")]
      
        public InArgument<string> AppCleanupLogId { get; set; }
        [Input("delete")]

        public InArgument<bool> Delete { get; set; }
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
            string contact = Contact.Get(context);
            Tracing.Trace("appcontactname " + contact);
            DateTime date = Date.Get(context);
            Tracing.Trace("date: " + date.ToString());
            decimal hoursWorked = HoursWorked.Get(context);
            string appCleanupLogId = AppCleanupLogId.Get(context);
            string name = Name.Get(context);
            bool delete = Delete.Get(context);

            if (delete)
            {
                Tracing.Trace("deleting the guid id : " + appCleanupLogId);
                var retrievedAppCleanupLog = Service.Retrieve("dfa_appcleanuplog", new Guid(appCleanupLogId), new ColumnSet("dfa_contactid"));
                if (retrievedAppCleanupLog.GetAttributeValue<EntityReference>("dfa_contactid") != null)
                {
                   var appContactGuid = retrievedAppCleanupLog.GetAttributeValue<EntityReference>("dfa_contactid").Id;
                   Service.Delete("dfa_appcontact", appContactGuid);
                }
                Service.Delete("dfa_appcleanuplog", new Guid(appCleanupLogId));
                Output.Set(context, "deleted");
                Tracing.Trace("deleted the guid id : " + Output);
                return;
            }


            if (!string.IsNullOrEmpty(appCleanupLogId))
            {
                Tracing.Trace("appCleanupLog id is provide and update is in progress");
                var retrievedAppCleanupLog = Service.Retrieve("dfa_appcleanuplog", new Guid(appCleanupLogId), new ColumnSet("dfa_contactid"));
                Entity appCleanupLog = new Entity("dfa_appcleanuplog");
                Entity appContact = new Entity("dfa_appcontact");
                appCleanupLog.Id = new Guid(appCleanupLogId);
              
                if (!string.IsNullOrEmpty(name))
                {
                    appCleanupLog["dfa_name"] = name;
                }

                if (date != null)
                    appCleanupLog["dfa_date"] = date;
               
                if (hoursWorked != null)
                {
                    appCleanupLog["dfa_hoursworked"] = hoursWorked;
                }
                if (!string.IsNullOrEmpty(contact) && retrievedAppCleanupLog.GetAttributeValue<EntityReference>("dfa_contactid") != null)
                {
                    appContact["dfa_name"] = contact;
                    appContact.Id = retrievedAppCleanupLog.GetAttributeValue<EntityReference>("dfa_contactid").Id;
                    Service.Update(appContact);
                    Tracing.Trace("appcontact is Updated");
                }
                if (!string.IsNullOrEmpty(application))
                {
                    appCleanupLog["dfa_applicationid"] = new EntityReference("dfa_appapplication", new Guid(application));
                }
                Service.Update(appCleanupLog);
                Output.Set(context, "Updated Successfully");
                Tracing.Trace("appCleanupLog is updated with guid:");

            }
            else
            {
                // Create
                Entity appCleanupLog = new Entity("dfa_appcleanuplog");
                Entity appContact = new Entity("dfa_appcontact");
               // var retrievedAppCleanupLog = Service.Retrieve("dfa_appcleanuplog", new Guid(appCleanupLogId), new ColumnSet("dfa_contactid"));
                appCleanupLog["dfa_name"] = name;
                appCleanupLog["dfa_hoursworked"] = hoursWorked;
                if (date != null)
                    appCleanupLog["dfa_date"] = date;
                if(!string.IsNullOrEmpty(application))
                appCleanupLog["dfa_applicationid"] = new EntityReference("dfa_appapplication", new Guid(application));
                if (!string.IsNullOrEmpty(contact))
                {
                    appContact["dfa_name"] = contact;
                    var appContactGuid = Service.Create(appContact);
                    appCleanupLog["dfa_contactid"] = new EntityReference("dfa_appcontact", appContactGuid);
                    Tracing.Trace("appcontact created is : " + appContactGuid.ToString());
                }

                var appCleanupLogGuid = Service.Create(appCleanupLog);
                Tracing.Trace("appCleanupLog created is : " + appCleanupLogGuid.ToString());
                Output.Set(context, appCleanupLogGuid.ToString());
            }
        }

    }
}

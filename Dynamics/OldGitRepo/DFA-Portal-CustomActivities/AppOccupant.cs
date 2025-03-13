using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Linq;

namespace DFA_Portal_CustomActivities
{
    public class AppOccupant : CodeActivity
    {
        [Input("dfa_appapplicationid")]
        public InArgument<string> Application { get; set; }
        [Input("dfa_name")]
        public InArgument<string> Name { get; set; }
        [Input("dfa_title")]
        public InArgument<string> Title { get; set; }
        [Input("dfa_firstname")]
        public InArgument<string> FirstName { get; set; }
        [Input("dfa_lastname")]
        public InArgument<string> LastName { get; set; }

        [Input("dfa_appoccupantid")]

        public InArgument<string> AppOccupantId { get; set; }
        [Input("dfa_contactid")]
        public InArgument<string> AppContactId { get; set; }
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
            string appContactId = AppContactId.Get(context);
            string firstName = FirstName.Get(context);
            var lastName = LastName.Get(context);
            var name = Name.Get(context);
            var title = Title.Get(context);

            var appOccupantId = AppOccupantId.Get(context);
            bool delete = Delete.Get(context);

            if (delete)
            {
                Tracing.Trace("deleting the guid id : " + appOccupantId);
                var retrievedAppOccupant = Service.Retrieve("dfa_appoccupant", new Guid(appOccupantId), new ColumnSet("dfa_contactid"));
                if (retrievedAppOccupant != null && retrievedAppOccupant.GetAttributeValue<EntityReference>("dfa_contactid").Id != null)
                {
                    Service.Delete("dfa_appcontact", retrievedAppOccupant.GetAttributeValue<EntityReference>("dfa_contactid").Id);
                    Tracing.Trace("dfa_appcontact attached is deleted with guid : " + retrievedAppOccupant.GetAttributeValue<EntityReference>("dfa_contactid").Id);
                }
                Service.Delete("dfa_appoccupant", new Guid(appOccupantId));
                Output.Set(context, "deleted");
                Tracing.Trace("deleted the record");
                return;
            }


            if (!string.IsNullOrEmpty(appOccupantId))
            {
                Tracing.Trace("appOccupant id is provide and update is in progress");
                Entity appOccupant = new Entity("dfa_appoccupant");
                appOccupant.Id = new Guid(appOccupantId);
                var retrievedAppOccupant = Service.Retrieve("dfa_appoccupant", appOccupant.Id, new ColumnSet("dfa_contactid"));
                Entity appContact = new Entity("dfa_appcontact");
                if (!string.IsNullOrEmpty(firstName))
                {
                      appContact["dfa_firstname"] = firstName;
                }

                if (!string.IsNullOrEmpty(lastName))
                    appContact["dfa_lastname"] = lastName;

                if (!string.IsNullOrEmpty(name))
                {
                    appOccupant["dfa_name"] = name;
                }
                if (!string.IsNullOrEmpty(title))
                {
                    appContact["dfa_title"] = title;
                }

                if (!string.IsNullOrEmpty(application))
                {
                    appOccupant["dfa_applicationid"] = new EntityReference("dfa_appapplication", new Guid(application));
                }
                if (!string.IsNullOrEmpty(appContactId))
                {
                    appOccupant["dfa_contactid"] = new EntityReference("dfa_appcontact", new Guid(appContactId));
                }
                if (retrievedAppOccupant != null && retrievedAppOccupant.GetAttributeValue<EntityReference>("dfa_contactid") != null)
                {
                    appContact.Id = retrievedAppOccupant.GetAttributeValue<EntityReference>("dfa_contactid").Id;
                    Service.Update(appContact);
                    Tracing.Trace("dfa_appcontact is updated");
                }
                Service.Update(appOccupant);
                Output.Set(context, "Updated Successfully");
                Tracing.Trace("dfa_othercontact is updated");

            }
            else
            {
                // Create
                Entity appOccupant = new Entity("dfa_appoccupant");
                Entity appContact = new Entity("dfa_appcontact");
                  appContact["dfa_firstname"] = firstName;
                  appContact["dfa_lastname"] = lastName;
                  appOccupant["dfa_name"] = name;
                  appContact["dfa_title"] = title;
                if (!string.IsNullOrEmpty(application))
                    appOccupant["dfa_applicationid"] = new EntityReference("dfa_appapplication", new Guid(application));
                //creating appContact
               var newlyAppContact = Service.Create(appContact);
                if (newlyAppContact != null)
                {
                    appOccupant["dfa_contactid"] = new EntityReference("dfa_appcontact", newlyAppContact);
                }
                var appOccupantGuid = Service.Create(appOccupant);
                Tracing.Trace("dfa_appoccupant created is : " + appOccupantGuid.ToString() + "dfa_appcontact is created in GUID: " + newlyAppContact);
                Output.Set(context, appOccupantGuid.ToString());
            }
        }

    }
}

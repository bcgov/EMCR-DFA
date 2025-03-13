using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Linq;

namespace DFA_Portal_CustomActivities
{
    public class OtherContact : CodeActivity
    {
        [Input("dfa_appapplicationid")]
        public InArgument<string> Application { get; set; }

        [Input("dfa_firstname")]
        public InArgument<string> FirstName { get; set; }
        [Input("dfa_lastname")]
        public InArgument<string> LastName { get; set; }

        [Input("dfa_phonenumber")]
        public InArgument<string> PhoneNumber { get; set; }
        [Input("dfa_emailaddress")]
        public InArgument<string> EmailAddress { get; set; }

        [Input("dfa_appothercontactid")]

        public InArgument<string> AppOtherContactId { get; set; }

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
            string firstName = FirstName.Get(context);
            var lastName = LastName.Get(context);
            var phoneNumber = PhoneNumber.Get(context);
            var emailAddress = EmailAddress.Get(context);
            
            var appOtherContactId = AppOtherContactId.Get(context);
            bool delete = Delete.Get(context);

            if (delete)
            {
                Tracing.Trace("deleting the guid id : " + appOtherContactId);
                Service.Delete("dfa_appothercontact", new Guid(appOtherContactId));
                Output.Set(context, "deleted");
                Tracing.Trace("deleted the record");
                return;
            }


            if (!string.IsNullOrEmpty(appOtherContactId))
            {
                Tracing.Trace("appOtherContactId id is provide and update is in progress");
                Entity appOtherContact = new Entity("dfa_appothercontact");
                appOtherContact.Id = new Guid(appOtherContactId);

                if (!string.IsNullOrEmpty(firstName))
                {
                    appOtherContact["dfa_firstname"] = firstName;
                }

                if (!string.IsNullOrEmpty(lastName))
                    appOtherContact["dfa_lastname"] = lastName;

                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    appOtherContact["dfa_phonenumber"] = phoneNumber;
                }
                if (!string.IsNullOrEmpty(emailAddress))
                {
                    appOtherContact["dfa_emailaddress"] = emailAddress;
                }

                if (!string.IsNullOrEmpty(application))
                {
                    appOtherContact["dfa_appapplicationid"] = new EntityReference("dfa_appapplication", new Guid(application));
                }
                Service.Update(appOtherContact);
                Output.Set(context, "Updated Successfully");
                Tracing.Trace("dfa_othercontact is updated");

            }
            else
            {
                // Create
                Entity appOtherContact = new Entity("dfa_appothercontact");
                appOtherContact["dfa_firstname"] = firstName;
                appOtherContact["dfa_lastname"] = lastName;
                appOtherContact["dfa_phonenumber"] = phoneNumber;
                appOtherContact["dfa_emailaddress"] = emailAddress;
                if (!string.IsNullOrEmpty(application))
                    appOtherContact["dfa_appapplicationid"] = new EntityReference("dfa_appapplication", new Guid(application));
               
                var appOtherContactGuid = Service.Create(appOtherContact);
                Tracing.Trace("dfa_othercontact created is : " + appOtherContactGuid.ToString());
                Output.Set(context, appOtherContactGuid.ToString());
            }
        }

    }
}

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Linq;

namespace DFA_Portal_CustomActivities
{
    public class AppSecondaryApplicant : CodeActivity
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
        [Input("dfa_applicanttype")]
        public InArgument<int> ApplicantType { get; set; }

        [Input("dfa_appsecondaryapplicantid")]

        public InArgument<string> AppSecondaryApplicantId { get; set; }
        
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
            var applicantType = ApplicantType.Get(context);
            var appSecondaryApplicantId = AppSecondaryApplicantId.Get(context);
            bool delete = Delete.Get(context);

            if (delete)
            {
                Tracing.Trace("deleting the guid id : " + appSecondaryApplicantId);
                Service.Delete("dfa_appsecondaryapplicant", new Guid(appSecondaryApplicantId));
                Output.Set(context, "deleted");
                Tracing.Trace("record deleted");
                return;
            }


            if (!string.IsNullOrEmpty(appSecondaryApplicantId))
            {
                Tracing.Trace("appSecondaryApplicantId id is provide and update is in progress");
                Entity appSecondaryApplicant = new Entity("dfa_appsecondaryapplicant");
                appSecondaryApplicant.Id = new Guid(appSecondaryApplicantId);

                if (!string.IsNullOrEmpty(firstName))
                {
                    appSecondaryApplicant["dfa_firstname"] = firstName;
                }

                if (!string.IsNullOrEmpty(lastName))
                    appSecondaryApplicant["dfa_lastname"] = lastName;

                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    appSecondaryApplicant["dfa_phonenumber"] = phoneNumber;
                }
                if (!string.IsNullOrEmpty(emailAddress))
                {
                    appSecondaryApplicant["dfa_emailaddress"] = emailAddress;
                }
                if (applicantType == 222710000 || applicantType == 222710006)
                    appSecondaryApplicant["dfa_applicanttype"] = new OptionSetValue(applicantType);
                
                if (!string.IsNullOrEmpty(application))
                {
                    appSecondaryApplicant["dfa_appapplicationid"] = new EntityReference("dfa_appapplication", new Guid(application));
                }
                Service.Update(appSecondaryApplicant);
                Output.Set(context, "Updated Successfully");
                Tracing.Trace("appSecondaryApplicant is updated");

            }
            else
            {
                // Create
                Entity appSecondaryApplicant = new Entity("dfa_appsecondaryapplicant");
                appSecondaryApplicant["dfa_firstname"] = firstName;
                appSecondaryApplicant["dfa_lastname"] = lastName;
                appSecondaryApplicant["dfa_phonenumber"] = phoneNumber;
                appSecondaryApplicant["dfa_emailaddress"] = emailAddress;
                if (!string.IsNullOrEmpty(application))
                    appSecondaryApplicant["dfa_appapplicationid"] = new EntityReference("dfa_appapplication", new Guid(application));
                if (applicantType == 222710000 || applicantType == 222710006)
                    appSecondaryApplicant["dfa_applicanttype"] = new OptionSetValue(applicantType);
                var appSecondaryGuid = Service.Create(appSecondaryApplicant);
                Tracing.Trace("dfa_appsecondaryapplicant created is : " + appSecondaryGuid.ToString());
                Output.Set(context, appSecondaryGuid.ToString());
            }
        }

    }
}

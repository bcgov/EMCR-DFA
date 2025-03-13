using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Linq;

namespace DFA_Portal_CustomActivities
{
    /// <summary>
    /// Dynamics Workflow can only access information up to the next relationship
    /// This will allow any child entity of Case to access information from the related Event
    /// </summary>
    using System;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Workflow;
    using Microsoft.Xrm.Sdk.Workflow.Activities;

    namespace CustomActivities
    {
        public class CreateProfile : CodeActivity
        {
            [Input("dfa_firstname")]
            public InArgument<string> FirstName { get; set; }

            [Input("dfa_lastname")]
            public InArgument<string> LastName { get; set; }
            [Input("dfa_alternatephonenumber")]
            public InArgument<string> AlternatePhoneNumber { get; set; }

            [Input("dfa_bcservicecardid")]
            public InArgument<string> BcServiceCardId { get; set; }
            [Input("dfa_cellphonenumber")]
            public InArgument<string> CellPhoneNumber { get; set; }

            [Input("dfa_emailaddress")]
            public InArgument<string> Email { get; set; }
            [Input("dfa_initial")]
            public InArgument<string> Initial { get; set; }


            [Input("dfa_isindigenous")]
            public InArgument<bool> IsIndigenous { get; set; }

            [Input("dfa_isprimaryandsecondaryaddresssame")]
            public InArgument<int> IsPrimaryOrSecondaryAddressSame { get; set; }
            [Input("dfa_primaryaddressline1")]
            public InArgument<string> PrimaryAddressLine1 { get; set; }

            [Input("dfa_primaryaddressline2")]
            public InArgument<string> PrimaryAddressLine2 { get; set; }
            [Input("dfa_primarycity")]
            public InArgument<string> PrimaryCity { get; set; }

            [Input("dfa_primarypostalcode")]
            public InArgument<string> PrimaryPostalCode { get; set; }
            [Input("dfa_primarystateprovince")]
            public InArgument<string> PrimaryStateProvince { get; set; }

            [Input("dfa_residencetelephonenumber")]
            public InArgument<string> ResidenceTelephoneNumber { get; set; }
            [Input("dfa_secondaryaddressline1")]
            public InArgument<string> SecondaryAddressLine1 { get; set; }

            [Input("dfa_secondaryaddressline2")]
            public InArgument<string> SecondaryAddressLine2 { get; set; }
            [Input("dfa_secondarycity")]
            public InArgument<string> SecondaryCity { get; set; }

            [Input("dfa_secondarypostalcode")]
            public InArgument<string> SecondaryPostalCode { get; set; }
            [Input("dfa_secondarystateprovince")]
            public InArgument<string> SecondaryStateProvince { get; set; }
            [Input("dfa_title")]
            public InArgument<string> Title { get; set; }
            [Input("dfa_appcontactid")]
            public InArgument<string> appContactId { get; set; }
            [Input("delete")]

            public InArgument<bool> Delete { get; set; }
            [Output("output")]
            public OutArgument<string> AppContact { get; set; }
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
                string firstName = FirstName.Get(context);
                string lastName = LastName.Get(context);
                string alternatePhoneNumber = AlternatePhoneNumber.Get(context);
                string bcServiceCardId = BcServiceCardId.Get(context);
                string cellPhoneNumber = CellPhoneNumber.Get(context);
                string email = Email.Get(context);
                string initial = Initial.Get(context);
                bool isIndigenous = IsIndigenous.Get(context);
                int isPrimaryOrSecondaryAddressSame = IsPrimaryOrSecondaryAddressSame.Get(context);
                string primaryAddressLine1 = PrimaryAddressLine1.Get(context);
                string primaryAddressLine2 = PrimaryAddressLine2.Get(context);
                string primaryCity = PrimaryCity.Get(context);
                string primaryPostalCode = PrimaryPostalCode.Get(context);
                string primaryStateProvince = PrimaryStateProvince.Get(context);
                string residenceTelephoneNumber = ResidenceTelephoneNumber.Get(context);
                string secondaryAddressLine1 = SecondaryAddressLine1.Get(context);
                string secondaryAddressLine2 = SecondaryAddressLine2.Get(context);
                string secondaryCity = SecondaryCity.Get(context);
                string secondaryPostalCode = SecondaryPostalCode.Get(context);
                string secondaryStateProvince = SecondaryStateProvince.Get(context);
                string appcontactId = appContactId.Get(context);
                string title = Title.Get(context);

                bool delete = Delete.Get(context);

                if (delete)
                {
                    Tracing.Trace("deleting the guid id : " + appcontactId);
                    Service.Delete("dfa_appcontact", new Guid(appcontactId));
                    AppContact.Set(context, "deleted");
                    Tracing.Trace("deleted the guid id : " + appcontactId);
                    return;
                }
                Tracing.Trace(appcontactId + "value is here: ");
                    
                if (!string.IsNullOrEmpty(appcontactId))
                {
                    Entity appContact = new Entity("dfa_appcontact");
                    appContact.Id = new Guid(appcontactId);
                    if(!string.IsNullOrEmpty(firstName))
                        appContact["dfa_firstname"] = firstName;
                    if (!string.IsNullOrEmpty(lastName))
                        appContact["dfa_lastname"] = lastName;
                    if (!string.IsNullOrEmpty(alternatePhoneNumber))
                        appContact["dfa_alternatephonenumber"] = alternatePhoneNumber;
                    if (!string.IsNullOrEmpty(bcServiceCardId))
                        appContact["dfa_bcservicecardid"] = bcServiceCardId;
                    if (!string.IsNullOrEmpty(cellPhoneNumber))
                        appContact["dfa_cellphonenumber"] = cellPhoneNumber;
                    if (!string.IsNullOrEmpty(email))
                        appContact["dfa_emailaddress"] = email;
                    if (!string.IsNullOrEmpty(initial))
                        appContact["dfa_initial"] = initial;
                    if (isIndigenous)
                        appContact["dfa_isindigenous"] = isIndigenous;
                    if (isPrimaryOrSecondaryAddressSame == 222710000 || isPrimaryOrSecondaryAddressSame == 222710001 || isPrimaryOrSecondaryAddressSame == 222710002)
                    {
                        appContact["dfa_isprimaryandsecondaryaddresssame"] = new OptionSetValue(isPrimaryOrSecondaryAddressSame);
                      
                    }
                    if (!string.IsNullOrEmpty(primaryAddressLine1))
                        appContact["dfa_primaryaddressline1"] = primaryAddressLine1;
                    if (!string.IsNullOrEmpty(primaryAddressLine2))
                        appContact["dfa_primaryaddressline2"] = primaryAddressLine2;
                    if (!string.IsNullOrEmpty(primaryCity))
                        appContact["dfa_primarycity"] = primaryCity;
                    if (!string.IsNullOrEmpty(primaryPostalCode))
                        appContact["dfa_primarypostalcode"] = primaryPostalCode;
                    if (!string.IsNullOrEmpty(primaryStateProvince))
                        appContact["dfa_primarystateprovince"] = primaryStateProvince;
                    if (!string.IsNullOrEmpty(residenceTelephoneNumber))
                        appContact["dfa_residencetelephonenumber"] = residenceTelephoneNumber;
                    if (!string.IsNullOrEmpty(secondaryAddressLine1))
                        appContact["dfa_secondaryaddressline1"] = secondaryAddressLine1;
                    if (!string.IsNullOrEmpty(secondaryAddressLine2))
                        appContact["dfa_secondaryaddressline2"] = secondaryAddressLine2;
                    if (!string.IsNullOrEmpty(secondaryCity))
                        appContact["dfa_secondarycity"] = secondaryCity;
                    if (!string.IsNullOrEmpty(secondaryPostalCode))
                        appContact["dfa_secondarypostalcode"] = secondaryPostalCode;
                    if (!string.IsNullOrEmpty(secondaryStateProvince))
                        appContact["dfa_secondarystateprovince"] = secondaryStateProvince;
                    if (!string.IsNullOrEmpty(title))
                        appContact["dfa_title"] = title;
                     Service.Update(appContact);
                    AppContact.Set(context, "Successfully update");
                    Tracing.Trace("AppContact is updated with guid:");

                }
                else
                {
                    // Create the app contact record
                    Entity appContact = new Entity("dfa_appcontact");
                    appContact["dfa_firstname"] = firstName;
                    appContact["dfa_lastname"] = lastName;
                    appContact["dfa_alternatephonenumber"] = alternatePhoneNumber;
                    appContact["dfa_bcservicecardid"] = bcServiceCardId;
                    appContact["dfa_cellphonenumber"] = cellPhoneNumber;
                    appContact["dfa_emailaddress"] = email;
                    appContact["dfa_initial"] = initial;
                    appContact["dfa_isindigenous"] = isIndigenous;
                    if (isPrimaryOrSecondaryAddressSame == 222710000 || isPrimaryOrSecondaryAddressSame == 222710001 || isPrimaryOrSecondaryAddressSame == 222710002)
                    {
                        appContact["dfa_isprimaryandsecondaryaddresssame"] = new OptionSetValue(isPrimaryOrSecondaryAddressSame);
                        Tracing.Trace("appcontactGuid created is : " + isPrimaryOrSecondaryAddressSame.ToString());
                    }
                    appContact["dfa_primaryaddressline1"] = primaryAddressLine1;
                    appContact["dfa_primaryaddressline2"] = primaryAddressLine2;
                    appContact["dfa_primarycity"] = primaryCity;
                    appContact["dfa_primarypostalcode"] = primaryPostalCode;
                    appContact["dfa_primarystateprovince"] = primaryStateProvince;
                    appContact["dfa_residencetelephonenumber"] = residenceTelephoneNumber;
                    appContact["dfa_secondaryaddressline1"] = secondaryAddressLine1;
                    appContact["dfa_secondaryaddressline2"] = secondaryAddressLine2;
                    appContact["dfa_secondarycity"] = secondaryCity;
                    appContact["dfa_secondarypostalcode"] = secondaryPostalCode;
                    appContact["dfa_secondarystateprovince"] = secondaryStateProvince;
                    appContact["dfa_title"] = title;
                    var appContactGuid = Service.Create(appContact);
                    Tracing.Trace("appcontactGuid created is : " + appContactGuid.ToString());
                    AppContact.Set(context, appContactGuid.ToString());


                }
              
              


                // Save the app contact record in CRM or perform any other desired actions

                // Set the output value
               
            }
            
        }
    }

}

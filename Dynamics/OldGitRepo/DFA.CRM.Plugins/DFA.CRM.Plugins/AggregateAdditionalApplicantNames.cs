using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DFA.CRM.Plugins
{
    /// <summary>
    /// For cares_AddApprovalItems Custom Action
    /// </summary>
    public class AggregateAdditionalApplicantNames : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            //Extract the tracing service for use in debugging sandboxed plug-ins.
            var Tracing =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            // Obtain the execution context from the service provider.
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            Tracing.Trace("context");
            // Obtain the organization service reference.
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            Tracing.Trace("servicefactory");
            var Service = serviceFactory.CreateOrganizationService(context.UserId);
            Tracing.Trace("contextuser" + context.UserId.ToString());
            Tracing.Trace("service" + Service.ToString());
            Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Message Name: {context.MessageName}");
            try
            {
                EntityCollection applicants = null;
                EntityReference caseER = null;
                var entityLogicalName = context.PrimaryEntityName;
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: {context.PrimaryEntityName}");
                if (entityLogicalName == "incident")
                {
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Obtain Applicants from Case update");
                    Entity caseEntity = (Entity)context.PostEntityImages["postImage"];
                    if (caseEntity == null)
                    {
                        Tracing.Trace($"{CustomCodeHelper.LineNumber()}: postImage is missing");
                        return;
                    }
                    caseER = caseEntity?.ToEntityReference();
                    applicants = GetAdditionalApplicantIdsByCase(Service, Tracing, caseER, null);
                }
                else
                {
                    if (context.MessageName == "Delete")
                    {
                        Entity recordBefore = (Entity)context.PreEntityImages["preImage"];
                        Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Deleting Applicants");
                        if (recordBefore == null)
                        {
                            Tracing.Trace($"{CustomCodeHelper.LineNumber()}: preImage is missing");
                            return;
                        }
                        if (!recordBefore.Attributes.Contains("dfa_caseid"))
                        {
                            Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Record does not contains dfa_caseid");
                            return; // Nothing can be done if there is no Case to update
                        }
                        caseER = recordBefore.GetAttributeValue<EntityReference>("dfa_caseid");
                        Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Case ER: {caseER.Id} - {caseER.Name}");
                        applicants = GetAdditionalApplicantIdsByCase(Service, Tracing, caseER, recordBefore.ToEntityReference());

                    }
                    else
                    {
                        Entity record = (Entity)context.PostEntityImages["postImage"];
                        Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Creating or Updating Applicants");
                        if (record == null)
                        {
                            Tracing.Trace($"{CustomCodeHelper.LineNumber()}: postImage is missing");
                            return;
                        }
                        if (!record.Attributes.Contains("dfa_caseid"))
                        {
                            Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Record does not contains dfa_caseid");
                            return;
                        }
                        caseER = record.GetAttributeValue<EntityReference>("dfa_caseid");
                        Tracing.Trace($"{CustomCodeHelper.LineNumber()}: caseER ==> {caseER.Id} - {caseER.Name}");
                        applicants = GetAdditionalApplicantIdsByCase(Service, Tracing, caseER, null);
                    }
                }
                // Check Case Record.
                var caseReadQuery = new QueryExpression("incident") { ColumnSet = new ColumnSet("customerid") };
                caseReadQuery.Criteria.AddCondition("incidentid", ConditionOperator.Equal, caseER.Id);
                caseReadQuery.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0); // Active
                var caseReadResults = Service.RetrieveMultiple(caseReadQuery);
                if (caseReadResults.Entities.Count == 0)
                {
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Parent Case record is either not available or no longer active");
                    return;
                }
                var primaryApplicantER = caseReadResults.Entities[0].GetAttributeValue<EntityReference>("customerid");
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Loop through obtained Applicants - count: {applicants.Entities.Count}");
                var applicantString = string.Empty;
                var contactInfo = GetPrimaryContactInfo(Service, Tracing, primaryApplicantER);


                foreach (var applicant in applicants.Entities)
                {
                    var recordName = GetAdditionalApplicantRecordName(Service, Tracing, applicant);
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Record Name: {recordName}");
                    contactInfo += GetAdditionalApplicantContactInfo(Service, Tracing, applicant);
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Contact Info: {contactInfo}");
                    applicantString += recordName + ";";
                    
                    
                }
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: {applicantString}");
                if (applicantString.Length > 4000)
                {
                    applicantString = applicantString.Substring(0, 4000);
                }
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: {contactInfo}");
                if (contactInfo.Length > 4000)
                {
                    contactInfo = contactInfo.Substring(0, 4000);
                }
                Entity caseRecord = new Entity("incident");
                caseRecord["incidentid"] = caseER.Id;
                caseRecord["dfa_additionalapplicants"] = applicantString;
                caseRecord["dfa_additionalapplicantsphonenumbersandemails"] = contactInfo;
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Updating Case Record caseER: {caseER.Id} Applicants: {applicantString} - Contact Info: {contactInfo}");
                Service.Update(caseRecord);
            }
            catch (Exception ex)
            {
                var error = "{\"error\":\"" + ex.Message + "|" + ex.Source + "|" + ex.StackTrace + "\"}";
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: {error}");
            }

        }

        public string GetPrimaryContactInfo(IOrganizationService Service, ITracingService Tracing, EntityReference customerER)
        {
            var phoneNumber = string.Empty;
            var mobilePhoneNumber = string.Empty;
            var alternatePhoneNumber = string.Empty;
            var emailaddress = string.Empty;
            var contactInfo = string.Empty;
            if (customerER != null)
            {
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Extract from Customer Lookup for {customerER.LogicalName} - {customerER.Id} {customerER.Name} ");
                if (customerER.LogicalName.Equals("contact"))
                {
                    var contact = Service.Retrieve("contact", customerER.Id, new ColumnSet("telephone1", "emailaddress1", "mobilephone", "telephone2"));
                    if (contact.GetAttributeValue<string>("telephone1") != null)
                    {
                        phoneNumber = contact.GetAttributeValue<string>("telephone1");
                    }
                    if (contact.GetAttributeValue<string>("emailaddress1") != null)
                    {
                        emailaddress = contact.GetAttributeValue<string>("emailaddress1");
                    }
                    if (contact.GetAttributeValue<string>("mobilephone") != null)
                    {
                        mobilePhoneNumber = contact.GetAttributeValue<string>("mobilephone");
                    }
                    if (contact.GetAttributeValue<string>("telephone2") != null)
                    {
                        alternatePhoneNumber = contact.GetAttributeValue<string>("telephone2");
                    }
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Add customerER applicant to list");
                }
                else if (customerER.LogicalName.Equals("account"))
                {
                    var account = Service.Retrieve("account", customerER.Id, new ColumnSet("telephone1", "emailaddress1"));
                    if (account.GetAttributeValue<string>("telephone1") != null)
                    {
                        phoneNumber = account.GetAttributeValue<string>("telephone1");
                    }
                    if (account.GetAttributeValue<string>("emailaddress1") != null)
                    {
                        emailaddress = account.GetAttributeValue<string>("emailaddress1");
                    }
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Add customerER applicant to list");
                }
            }

            var numbersOnlyPhoneNumber = ConvertFormattedPhoneNumberIntoNumbersOnly(Tracing, phoneNumber);
            var numbersOnlyMobilePhoneNumber = ConvertFormattedPhoneNumberIntoNumbersOnly(Tracing, mobilePhoneNumber);
            var numbersOnlyAlternatePhonenumber = ConvertFormattedPhoneNumberIntoNumbersOnly(Tracing, alternatePhoneNumber);
            if (!string.IsNullOrEmpty(numbersOnlyPhoneNumber))
            {
                contactInfo += numbersOnlyPhoneNumber + "|";
            }
            if (!string.IsNullOrEmpty(emailaddress))
            {
                contactInfo += emailaddress + "|";
            }
            if (!string.IsNullOrEmpty(numbersOnlyMobilePhoneNumber))
            {
                contactInfo += numbersOnlyMobilePhoneNumber + "|";
            }
            if (!string.IsNullOrEmpty(numbersOnlyAlternatePhonenumber))
            {
                contactInfo += numbersOnlyAlternatePhonenumber + "|";
            }
            return contactInfo;
        }

        public string GetAdditionalApplicantContactInfo(IOrganizationService Service, ITracingService Tracing, Entity record)
        {
            Tracing.Trace($"{CustomCodeHelper.LineNumber()}: GetAdditionalApplicantContactInfo");
            var phoneNumber = record?.GetAttributeValue<string>("dfa_phonenumber");
            var mobilePhoneNumber = string.Empty;
            var alternatePhoneNumber = string.Empty;
            var emailaddress = record?.GetAttributeValue<string>("dfa_emailaddress");
            var customerER = record?.GetAttributeValue<EntityReference>("dfa_customer");
            var applicantTypeOS = record?.GetAttributeValue<OptionSetValue>("dfa_applicanttype");
            var contactInfo = string.Empty;
            if (customerER != null)
            {
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Extract from Customer Lookup for {customerER.LogicalName} - {customerER.Id} {customerER.Name} ");
                if (customerER.LogicalName.Equals("contact"))
                {
                    var contact = Service.Retrieve("contact", customerER.Id, new ColumnSet("telephone1", "emailaddress1", "mobilephone", "telephone2"));
                    if (contact.GetAttributeValue<string>("telephone1") != null)
                    {
                        phoneNumber = contact.GetAttributeValue<string>("telephone1");
                    }
                    if (contact.GetAttributeValue<string>("emailaddress1") != null)
                    {
                        emailaddress = contact.GetAttributeValue<string>("emailaddress1");
                    }
                    if (contact.GetAttributeValue<string>("mobilephone") != null)
                    {
                        mobilePhoneNumber = contact.GetAttributeValue<string>("mobilephone");
                    }
                    if (contact.GetAttributeValue<string>("telephone2") != null)
                    {
                        alternatePhoneNumber = contact.GetAttributeValue<string>("telephone2");
                    }
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Add customerER applicant to list");
                }
                else if (customerER.LogicalName.Equals("account"))
                {
                    var account = Service.Retrieve("account", customerER.Id, new ColumnSet("telephone1", "emailaddress1"));
                    if (account.GetAttributeValue<string>("telephone1") != null)
                    {
                        phoneNumber = account.GetAttributeValue<string>("telephone1");
                    }
                    if (account.GetAttributeValue<string>("emailaddress1") != null)
                    {
                        emailaddress = account.GetAttributeValue<string>("emailaddress1");
                    }
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Add customerER applicant to list");
                }
            }

            var numbersOnlyPhoneNumber = ConvertFormattedPhoneNumberIntoNumbersOnly(Tracing, phoneNumber);
            var numbersOnlyMobilePhoneNumber = ConvertFormattedPhoneNumberIntoNumbersOnly(Tracing, mobilePhoneNumber);
            var numbersOnlyAlternatePhonenumber = ConvertFormattedPhoneNumberIntoNumbersOnly(Tracing, alternatePhoneNumber);
            if (!string.IsNullOrEmpty(numbersOnlyPhoneNumber))
            {
                contactInfo += numbersOnlyPhoneNumber +"|";
            }
            if (!string.IsNullOrEmpty(emailaddress))
            {
                contactInfo += emailaddress + "|";
            }
            if (!string.IsNullOrEmpty(numbersOnlyMobilePhoneNumber))
            {
                contactInfo += numbersOnlyMobilePhoneNumber + "|";
            }
            if (!string.IsNullOrEmpty(numbersOnlyAlternatePhonenumber))
            {
                contactInfo += numbersOnlyAlternatePhonenumber + "|";
            }
            return contactInfo;
        }

        public string ConvertFormattedPhoneNumberIntoNumbersOnly(ITracingService Tracing, string inPhoneNumber)
        {
            Tracing.Trace($"{CustomCodeHelper.LineNumber()}: ConvertFormattedPhoneNumberIntoNumbersOnly");
            if (string.IsNullOrEmpty(inPhoneNumber))
            {
                return string.Empty;
            }
            var array = inPhoneNumber.ToCharArray();
            var outPhoneNumber = string.Empty;
            var number = -1;
            foreach(var character in array)
            {
                var charString = character.ToString();
                if (!int.TryParse(charString, out number))
                {
                    continue;
                }
                outPhoneNumber += charString;
            }
            return outPhoneNumber;
        }

        public string GetAdditionalApplicantRecordName(IOrganizationService Service, ITracingService Tracing, Entity record)
        {
            Tracing.Trace($"{CustomCodeHelper.LineNumber()}: GetAdditionalApplicantRecordName");
            var lastName = record?.GetAttributeValue<string>("dfa_lastname");
            var firstName = record?.GetAttributeValue<string>("dfa_firstname");
            var fullName = (lastName ?? string.Empty) + ", " + (firstName ?? string.Empty);
            var recordName = (lastName ?? string.Empty) + ", " + (firstName ?? string.Empty);
            var organizationName = record?.GetAttributeValue<string>("dfa_organizationname");
            var customerER = record?.GetAttributeValue<EntityReference>("dfa_customer");
            var applicantTypeOS = record?.GetAttributeValue<OptionSetValue>("dfa_applicanttype");
            if (customerER != null)
            {
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Extract from Customer Lookup for {customerER.LogicalName} - {customerER.Id} {customerER.Name} ");
                if (customerER.LogicalName.Equals("contact"))
                {
                    var contact = Service.Retrieve("contact", customerER.Id, new ColumnSet("lastname", "firstname"));
                    lastName = contact.GetAttributeValue<string>("lastname");
                    firstName = contact.GetAttributeValue<string>("firstname");
                    recordName = (lastName ?? string.Empty) + ", " + (firstName ?? string.Empty);
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Add customerER applicant {fullName} to list");
                }
                else
                {
                    recordName = customerER.Name;
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Add customerER applicant {organizationName} to list");
                }
            }
            else if (applicantTypeOS != null && applicantTypeOS.Value == 222710000 && !string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName)) // Contact
            {
                recordName = fullName;
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Applicant Type OS Value: {applicantTypeOS.Value}");
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Add applicant {fullName} to list");
            }
            else if (applicantTypeOS != null && applicantTypeOS.Value == 222710001 && !string.IsNullOrWhiteSpace(organizationName)) // Organization
            {
                recordName = organizationName;
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Applicant Type OS Value: {applicantTypeOS.Value}");
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Add applicant {organizationName} to list");
            }
            return recordName;
        }

        public EntityCollection GetAdditionalApplicantIdsByCase(IOrganizationService Service, ITracingService Tracing, EntityReference caseER, EntityReference excludedRecordER)
        {
            Tracing.Trace($"{CustomCodeHelper.LineNumber()}: GetAdditionalApplicantIdsByCase");
            var query = new QueryExpression("dfa_additionalapplicant") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0); // Active
            query.Criteria.AddCondition("dfa_caseid", ConditionOperator.Equal, caseER.Id);
            if (excludedRecordER != null)
            {
                query.Criteria.AddCondition("dfa_additionalapplicantid", ConditionOperator.NotEqual, excludedRecordER.Id);
            }
            var results = Service.RetrieveMultiple(query);

            // Get Case Customer
            var caseRecord = Service.Retrieve("incident", caseER.Id, new ColumnSet("customerid"));

            var mainApplicant = new Entity("dfa_additionalapplicant");
            mainApplicant["dfa_customer"] = caseRecord?.GetAttributeValue<EntityReference>("customerid");
            mainApplicant["dfa_caseid"] = caseER;
            results.Entities.Add(mainApplicant);
            return results;
        }
    }
}
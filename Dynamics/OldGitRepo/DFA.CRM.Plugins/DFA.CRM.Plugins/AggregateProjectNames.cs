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
    /// For Other Contact
    /// </summary>
    public class AggregateProjectNames : IPlugin
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
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Obtain Project from Case update");
                    Entity caseEntity = (Entity)context.PostEntityImages["postImage"];
                    if (caseEntity == null)
                    {
                        Tracing.Trace($"{CustomCodeHelper.LineNumber()}: postImage is missing");
                        return;
                    }
                    caseER = caseEntity?.ToEntityReference();
                    
                    applicants = GetRecordsByCase(Service, Tracing, caseER, null);
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
                        applicants = GetRecordsByCase(Service, Tracing, caseER, recordBefore.ToEntityReference());

                    }
                    else
                    {
                        Entity record = (Entity)context.PostEntityImages["postImage"];
                        Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Creating or Updating Projects");
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
                        applicants = GetRecordsByCase(Service, Tracing, caseER, null);
                    }
                }
                // Check Case Record.
                var caseReadQuery = new QueryExpression("incident") { ColumnSet = new ColumnSet(false) };
                caseReadQuery.Criteria.AddCondition("incidentid", ConditionOperator.Equal, caseER.Id);
                caseReadQuery.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0); // Active
                var caseReadResults = Service.RetrieveMultiple(caseReadQuery);
                if (caseReadResults.Entities.Count == 0)
                {
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Parent Case record is either not available or no longer active");
                    return;
                }

                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Loop through obtained Applicants - count: {applicants.Entities.Count}");
                var applicantString = string.Empty;
                var contactInfo = string.Empty;
                foreach(var applicant in applicants.Entities)
                {
                    var recordName = applicant.GetAttributeValue<string>("dfa_projectname");
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Record Name: {recordName}");
                    applicantString += recordName + ";";
                    
                    
                }
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: {applicantString}");
                if (applicantString.Length > 4000)
                {
                    applicantString = applicantString.Substring(0, 4000);
                }
                Entity caseRecord = new Entity("incident");
                caseRecord["incidentid"] = caseER.Id;
                caseRecord["dfa_projectnames"] = applicantString;
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Updating Trace Record");
                Service.Update(caseRecord);
            }
            catch (Exception ex)
            {
                var error = "{\"error\":\"" + ex.Message + "|" + ex.Source + "|" + ex.StackTrace + "\"}";
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: {error}");
            }

        }

        public EntityCollection GetRecordsByCase(IOrganizationService Service, ITracingService Tracing, EntityReference caseER, EntityReference excludedRecordER)
        {
            Tracing.Trace($"{CustomCodeHelper.LineNumber()}: GetRecordsByCase");
            var query = new QueryExpression("dfa_project") { ColumnSet = new ColumnSet(true) };
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0); // Active
            query.Criteria.AddCondition("dfa_caseid", ConditionOperator.Equal, caseER.Id);
            if (excludedRecordER != null)
            {
                query.Criteria.AddCondition("dfa_projectid", ConditionOperator.NotEqual, excludedRecordER.Id);
            }
            var results = Service.RetrieveMultiple(query);
            return results;
        }
    }
}
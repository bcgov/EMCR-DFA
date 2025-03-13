using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DFA.CRM.Plugins
{
    /// <summary>
    /// Rollup Claim Amount, Approved Amounts to Project
    /// </summary>
    public class UpdateEMBCRegionalManagerInEffectedRegionCommunity : IPlugin
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
            var service = serviceFactory.CreateOrganizationService(context.UserId);
            Tracing.Trace("contextuser" + context.UserId.ToString());
            Tracing.Trace("service" + service.ToString());
            Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Message Name: {context.MessageName}");

            string eMBCRegionalManager = string.Empty;

            try
            {
                // The InputParameters collection contains all the data passed in the message request.  
                if (context.InputParameters.Contains("Target") &&
                    (context.InputParameters["Target"] is Entity) &&
                    (context.MessageName == "Update"))
                {

                    Guid regardingobjectid = Guid.NewGuid();
                    Entity parentEntity = (Entity)context.InputParameters["Target"];

                    if (parentEntity != null &&
                       parentEntity.Attributes.Contains("dfa_regionalmanager"))
                    {
                        eMBCRegionalManager = parentEntity.Attributes["dfa_regionalmanager"].ToString();
                        Tracing.Trace($"{CustomCodeHelper.LineNumber()}: New EMBC Regional Manager:  ==> {eMBCRegionalManager}");
                    }
                    else
                    {
                        Tracing.Trace($"{CustomCodeHelper.LineNumber()}: dfa_regionalmanager is not passed");
                        return;
                    }

                    // Get the current Parent Entity GUID            
                    regardingobjectid = new Guid(parentEntity.Id.ToString());

                    string kidEntity = "dfa_effectedregioncommunity";

                    // Retrieve all child entity records
                    var queryExpression = new QueryExpression(kidEntity);
                    var qeFilter = new FilterExpression(LogicalOperator.And);

                    qeFilter.AddCondition(new ConditionExpression("dfa_regionid", ConditionOperator.Equal, regardingobjectid));
                    queryExpression.Criteria = qeFilter;

                    //Get results:
                    var result = service.RetrieveMultiple(queryExpression);
                    foreach (var relatedKidEntityRecord in result.Entities)
                    {         
                        //Update EMBC Regional Manager in Effected Region/Community Entity
                        Entity effectedRegionCommunityRecord = new Entity("dfa_effectedregioncommunity");
                        effectedRegionCommunityRecord.Id = relatedKidEntityRecord.Id;
                        effectedRegionCommunityRecord["dfa_embcregionalmanager"] = eMBCRegionalManager;
                        Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Updating Trace Record");
                        service.Update(effectedRegionCommunityRecord);
                    }
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                var error = "{\"error\":\"" + ex.Message + "|" + ex.Source + "|" + ex.StackTrace + "\"}";
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: {error}");
            }

        }
    }
}
using System;
using System.Linq;
using System.Activities;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Crm.Sdk.Messages;

namespace DFA.CRM.CustomWorkflow
{
    public class RevisedRollupChildEntityNumberField : CodeActivity
    {
        [RequiredArgument]
        [Input("For Each Entity Name")]
        public InArgument<string> ForEachEntityNameInput { get; set; }

        [RequiredArgument]
        [Input("For Each Relationship Name")]
        public InArgument<string> ForEachRelationshipNameInput { get; set; }

        [Input("Child Entity Attribute Name")]
        public InArgument<string> ChildEntityAttributeNameInput { get; set; }

        [Input("Include Inactive Record in Search")]
        public InArgument<bool> IncludeInactiveInput { get; set; }

        [Output("Calculated Totals")]
        public OutArgument<decimal> CalculatedTotalsOutput { get; set; }

        [Output("Row Count")]
        public OutArgument<int> AffectedRecordsCountOutput { get; set; }

        CodeActivityContext Activity;
        ITracingService Tracing;
        IWorkflowContext Workflow;
        IOrganizationServiceFactory ServiceFactory;
        IOrganizationService Service;
        OrganizationServiceContext ServiceContext;

        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="activityContext">The execution Workflow.</param>
        protected override void Execute(CodeActivityContext activityContext)
        {
            Activity = activityContext;
            Tracing = Activity.GetExtension<ITracingService>();
            Workflow = Activity.GetExtension<IWorkflowContext>();
            ServiceFactory = Activity.GetExtension<IOrganizationServiceFactory>();
            Service = ServiceFactory.CreateOrganizationService(Workflow.UserId);
            ServiceContext = new OrganizationServiceContext(Service);


            Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Obtain Input Parameters");
            var forEachEntityName = ForEachEntityNameInput.Get(activityContext);
            Tracing.Trace($"{CustomCodeHelper.LineNumber()}: For Each Entity Name: {forEachEntityName}");
            var forEachRelationshipName = ForEachRelationshipNameInput.Get(activityContext);
            Tracing.Trace($"{CustomCodeHelper.LineNumber()}: For Each Relationship Name: {forEachRelationshipName}");
            var childEntityAttributeName = ChildEntityAttributeNameInput.Get(activityContext);
            Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Child Entity Name: {childEntityAttributeName}");
            var includeInactive = IncludeInactiveInput.Get(activityContext);
            Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Include Inactive: {includeInactive}");
            try
            {
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Obtain the Target Entity Record");
                Entity entity = Service.Retrieve(Workflow.PrimaryEntityName, Workflow.PrimaryEntityId, new ColumnSet(true));


                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Check What sort of relationship from Metadata of Primary Entity");
                var targetMetadataRequest = new RetrieveEntityRequest
                {
                    LogicalName = Workflow.PrimaryEntityName,
                    EntityFilters = EntityFilters.All
                };

                var targetMetadataResponse = (RetrieveEntityResponse)ServiceContext.Execute(targetMetadataRequest);

                var metadataManyToMany = targetMetadataResponse.EntityMetadata.ManyToManyRelationships.FirstOrDefault(r => r.SchemaName == forEachRelationshipName);
                var metadataOneToMany = targetMetadataResponse.EntityMetadata.OneToManyRelationships.FirstOrDefault(r => r.SchemaName == forEachRelationshipName);

                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Check if the relationship truly exists");
                if (metadataManyToMany == null && metadataOneToMany == null)
                {
                    var errMsg = $"{CustomCodeHelper.LineNumber()}: Cannot Find relationship. {forEachRelationshipName} does not correspond to a proper 1:N or N:N relationship";
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: {errMsg}");
                    throw new InvalidPluginExecutionException(errMsg);
                }

                var targetPrimaryFieldLogicalName = targetMetadataResponse.EntityMetadata.PrimaryIdAttribute;
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: targetPrimaryFieldLogicalName: {targetPrimaryFieldLogicalName}");
                var forEachMetadataRequest = new RetrieveEntityRequest
                {
                    LogicalName = forEachEntityName,
                    EntityFilters = EntityFilters.Attributes,
                };

                var forEachMetadataResponse = (RetrieveEntityResponse)ServiceContext.Execute(forEachMetadataRequest);

                var forEachPrimaryFieldLogicalName = forEachMetadataResponse.EntityMetadata.PrimaryIdAttribute;
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: forEachPrimaryFieldLogicalName: {forEachPrimaryFieldLogicalName}");

                List<Entity> relatedRecords = new List<Entity>();

                if (metadataManyToMany != null)
                {
                    var fetchXml = new StringBuilder();
                    fetchXml.Append("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>");
                    fetchXml.Append($"<entity name='{forEachEntityName}'>");
                    fetchXml.Append($"<attribute name='{forEachEntityName}id' />");
                    if (!string.IsNullOrWhiteSpace(childEntityAttributeName))
                    {
                        fetchXml.Append($"<attribute name='{childEntityAttributeName}' />");
                    }
                    // Notes does not have statecode
                    if (forEachEntityName != "annotation")
                    {
                        fetchXml.Append($"<attribute name='statecode' />");
                    }
                    fetchXml.Append($"<link-entity name='{metadataManyToMany.IntersectEntityName}' from='{forEachPrimaryFieldLogicalName}' to='{forEachPrimaryFieldLogicalName}' visible='false' intersect='true'>");
                    fetchXml.Append($"<link-entity name='{Workflow.PrimaryEntityName}' from='{targetPrimaryFieldLogicalName}' to='{targetPrimaryFieldLogicalName}' alias='aa'>");
                    fetchXml.Append("<filter type='and'>");
                    fetchXml.Append($"<condition attribute='{targetPrimaryFieldLogicalName}' operator='eq' value='{entity.Id}' />");
                    fetchXml.Append("</filter>");
                    fetchXml.Append("</link-entity>");
                    fetchXml.Append("</link-entity>");
                    fetchXml.Append("</entity>");
                    fetchXml.Append("</fetch>");
                    Tracing.Trace($"FetchXML   {fetchXml.ToString()}");
                    relatedRecords = Service.RetrieveMultiple(new FetchExpression(fetchXml.ToString())).Entities.ToList();

                    if (!includeInactive)
                    {
                        Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Filter only Active Records, if StateCode is a valid attribute");
                        if (relatedRecords.Any(r => r.Contains("statecode")))
                        {
                            relatedRecords = relatedRecords.Where(r => r.GetAttributeValue<OptionSetValue>("statecode").Value == 0).ToList();
                        }
                    }

                }
                else if (metadataOneToMany != null)
                {
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: N:1 Relationship. Referenced Attribute: {metadataOneToMany.ReferencedAttribute}.  Referencing Attribute: {metadataOneToMany.ReferencingAttribute}");
                    var oneToManyQuery = new QueryExpression(forEachEntityName) { ColumnSet = new ColumnSet(true) };
                    oneToManyQuery.Criteria.AddCondition(metadataOneToMany.ReferencingAttribute, ConditionOperator.Equal, entity.Id);
                   
                    relatedRecords = Service.RetrieveMultiple(oneToManyQuery).Entities.ToList();
                    if (!includeInactive)
                    {
                        Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Filter only Active Records, if StateCode is a valid attribute");
                        if (relatedRecords.Any(r => r.Contains("statecode")))
                        {
                            relatedRecords = relatedRecords.Where(r => r.GetAttributeValue<OptionSetValue>("statecode").Value == 0).ToList();
                        }
                    }
                }
                // Get Attribute Type
                
                if (!string.IsNullOrEmpty(childEntityAttributeName))
                {
                    var attributeRequest = new RetrieveAttributeRequest
                    {
                        EntityLogicalName = forEachEntityName,
                        LogicalName = childEntityAttributeName,
                        RetrieveAsIfPublished = false
                    };

                    var attributeResponse = (RetrieveAttributeResponse)Service.Execute(attributeRequest);
                    var attributeTypeCode = attributeResponse.AttributeMetadata.AttributeType;
                    var totals = 0m;
                    foreach (var relatedRecord in relatedRecords)
                    {
                        var number = 0m;
                        switch (attributeTypeCode)
                        {
                            case AttributeTypeCode.BigInt:
                            case AttributeTypeCode.Integer:
                                number = relatedRecord.GetAttributeValue<int>(childEntityAttributeName);
                                break;
                            case AttributeTypeCode.Double:
                                number = (decimal)relatedRecord.GetAttributeValue<double>(childEntityAttributeName);
                                break;
                            case AttributeTypeCode.Money:
                                number = relatedRecord.GetAttributeValue<Money>(childEntityAttributeName) == null ? 0m :
                                    relatedRecord.GetAttributeValue<Money>(childEntityAttributeName).Value;
                                break;
                            case AttributeTypeCode.Decimal:
                                number = relatedRecord.GetAttributeValue<decimal>(childEntityAttributeName);
                                break;
                        }
                        Tracing.Trace($"{CustomCodeHelper.LineNumber()}: number: {number} - {childEntityAttributeName}");
                        totals += number;
                    }
                    CalculatedTotalsOutput.Set(activityContext, totals);
                }
                AffectedRecordsCountOutput.Set(activityContext, relatedRecords.Count);
            }
            catch (Exception e)
            {
                Tracing.Trace("Exception: {0}", e.ToString());

                // Handle the exception.
                throw new InvalidPluginExecutionException(e.ToString());
            }

            Tracing.Trace("Exiting WorkflowActivityTest.Execute(), Correlation Id: {0}", Workflow.CorrelationId);
        }
    }
}
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
    public class ForEachHelper : CodeActivity
    {
        [RequiredArgument]
        [Input("For Each Entity Name")]
        public InArgument<string> ForEachEntityNameInput { get; set; }

        [RequiredArgument]
        [Input("For Each Relationship Name")]
        public InArgument<string> ForEachRelationshipNameInput { get; set; }

        [RequiredArgument]
        [Input("Child Workflow Name")]
        public InArgument<string> ChildWorkflowNameInput { get; set; }

        [Input("Include Inactive Record in Search")]
        public InArgument<bool> IncludeInactiveInput { get; set; }

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
            var childWorkflowName = ChildWorkflowNameInput.Get(activityContext);
            Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Child Workflow Name: {childWorkflowName}");
            var includeInactive = IncludeInactiveInput.Get(activityContext);

            try
            {
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Obtain the Target Entity Record");
                Entity entity = Service.Retrieve(Workflow.PrimaryEntityName, Workflow.PrimaryEntityId, new ColumnSet(true));

                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Obtain the Child Workflow By Name");
                var childWorkflowQuery = new QueryExpression("workflow") { ColumnSet = new ColumnSet(true) };
                childWorkflowQuery.Criteria.AddCondition("name", ConditionOperator.Equal, childWorkflowName);
                childWorkflowQuery.Criteria.AddCondition("type", ConditionOperator.Equal, 1); // Definition
                childWorkflowQuery.Criteria.AddCondition("statecode", ConditionOperator.Equal, 1); // Activated
                childWorkflowQuery.Criteria.AddCondition("category", ConditionOperator.Equal, 0); // Workflow
                childWorkflowQuery.Criteria.AddCondition("subprocess", ConditionOperator.Equal, true); // Is Child Workflow

                childWorkflowQuery.TopCount = 1;
                var childWorkflow = Service.RetrieveMultiple(childWorkflowQuery).Entities.FirstOrDefault();

                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: If Workflow cannot be found");
                if (childWorkflow == null)
                {
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Cannot find activated Child Workflow: {childWorkflowName}");
                    throw new InvalidPluginExecutionException($"Cannot find activated Child Workflow: {childWorkflowName}");
                }

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

                foreach (var relatedRecord in relatedRecords)
                {
                    ExecuteWorkflowRequest request = new ExecuteWorkflowRequest()
                    {
                        WorkflowId = childWorkflow.Id,
                        EntityId = relatedRecord.Id,
                    };

                    ExecuteWorkflowResponse response = (ExecuteWorkflowResponse)ServiceContext.Execute(request);
                }
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
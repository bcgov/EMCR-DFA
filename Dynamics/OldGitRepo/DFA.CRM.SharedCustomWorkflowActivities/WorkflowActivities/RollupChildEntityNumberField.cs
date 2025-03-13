using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;

namespace DFA.CRM.CustomWorkflow
{
    public class RollupChildEntityNumberField : CodeActivity
    {
        [RequiredArgument]
        [Input("Child Entity Logical Name")]
        public InArgument<string> ChildEntityLogicalNameInput { get; set; }

        [RequiredArgument]
        [Input("Child Entity Parent Lookup Attribute Name")]
        public InArgument<string> ParentLookupAttributeNameInput { get; set; }

        [RequiredArgument]
        [Input("Child Entity Attribute Name")]
        public InArgument<string> ChildEntityAttributeNameInput { get; set; }

        [Input("Include Inactive Record in Calculation")]
        public InArgument<bool> IncludeInactiveInput { get; set; }

        [Output("Calculated Totals")]
        public OutArgument<decimal> CalculatedTotalsOutput { get; set; }

        [Output("Record Exists")]
        public OutArgument<bool> RecordExistsOutput { get; set; }

        CodeActivityContext Activity;
        ITracingService Tracing;
        IWorkflowContext Workflow;
        IOrganizationServiceFactory ServiceFactory;
        IOrganizationService Service;

        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="activityContext"></param>
        protected override void Execute(CodeActivityContext activityContext)
        {

            Activity = activityContext;
            Tracing = Activity.GetExtension<ITracingService>();
            Workflow = Activity.GetExtension<IWorkflowContext>();
            ServiceFactory = Activity.GetExtension<IOrganizationServiceFactory>();
            Service = ServiceFactory.CreateOrganizationService(Workflow.UserId);
            CalculatedTotalsOutput.Set(activityContext, 0m);
            RecordExistsOutput.Set(activityContext, false);
            try
            {
                var childEntityLogicalName = ChildEntityLogicalNameInput.Get(activityContext);
                var childEntityAttributeName = ChildEntityAttributeNameInput.Get(activityContext);
                var includeInactive = IncludeInactiveInput.Get(activityContext);
                var parentLookupAttributeName = ParentLookupAttributeNameInput.Get(activityContext);
                var primaryEntityId = Workflow.PrimaryEntityId;


                Tracing.Trace($"Child Entity: {childEntityLogicalName}; Total of field:{childEntityAttributeName}; Include Inactive Records: {includeInactive}");
                Tracing.Trace($"Parent Lookup Attribute: {parentLookupAttributeName} - Parent Id: {primaryEntityId} of {Workflow.PrimaryEntityName}");

                var query = new QueryExpression(childEntityLogicalName) { ColumnSet = new ColumnSet(childEntityAttributeName) };
                query.Criteria.AddCondition(parentLookupAttributeName, ConditionOperator.Equal, primaryEntityId);
                if (!includeInactive)
                {
                    query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0); // Active only
                }
                var results = Service.RetrieveMultiple(query);
                var totals = 0m;

                // Get Attribute Type
                var attributeRequest = new RetrieveAttributeRequest
                {
                    EntityLogicalName = childEntityLogicalName,
                    LogicalName = childEntityAttributeName,
                    RetrieveAsIfPublished = false
                };

                var attributeResponse = (RetrieveAttributeResponse)Service.Execute(attributeRequest);
                var attributeTypeCode = attributeResponse.AttributeMetadata.AttributeType;

                foreach (var entity in results.Entities)
                {
                    switch (attributeTypeCode)
                    {
                        case AttributeTypeCode.BigInt:
                        case AttributeTypeCode.Integer:
                            totals += entity.GetAttributeValue<int>(childEntityAttributeName);
                            break;
                        case AttributeTypeCode.Double:
                            totals += (decimal)entity.GetAttributeValue<double>(childEntityAttributeName);
                            break;
                        case AttributeTypeCode.Money:
                            totals += entity.GetAttributeValue<Money>(childEntityAttributeName) == null ? 0m : 
                                entity.GetAttributeValue<Money>(childEntityAttributeName).Value;
                            break;
                        case AttributeTypeCode.Decimal:
                            totals += entity.GetAttributeValue<decimal>(childEntityAttributeName);
                            break;
                    }
                }

                CalculatedTotalsOutput.Set(activityContext, totals);
                RecordExistsOutput.Set(activityContext, results.Entities.Count > 0);
            }
            catch (InvalidPluginExecutionException) { throw; }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException($"Unexpected error. {ex.Message}", ex);
            }
        }
    }
}

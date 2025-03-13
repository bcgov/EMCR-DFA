using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Linq;

namespace DFA.CRM.CustomWorkflow
{
    /// <summary>
    /// Dynamics Workflow to delete record
    /// </summary>
    public class DeleteRecord : CodeActivity
    {

        [RequiredArgument]
        [Input("Delete Using Record URL")]
        [Default("True")]
        public InArgument<bool> DeleteUsingRecordURL { get; set; }

        [Input("Record URL")]
        [ReferenceTarget("")]
        public InArgument<String> DeleteRecordURL { get; set; }

        [Input("Entity Type Name")]
        [ReferenceTarget("")]
        public InArgument<String> EntityTypeName { get; set; }

        [Input("Entity Guid")]
        [ReferenceTarget("")]
        public InArgument<String> EntityGuid { get; set; }

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
            try
            {
                Activity = activityContext;
                Tracing = Activity.GetExtension<ITracingService>();
                Workflow = Activity.GetExtension<IWorkflowContext>();
                ServiceFactory = Activity.GetExtension<IOrganizationServiceFactory>();
                Service = ServiceFactory.CreateOrganizationService(Workflow.UserId);

                #region "Read Parameters"
                String _deleteRecordURL = this.DeleteRecordURL.Get(activityContext);
                string entityName = "";
                string objectId = "";
                if (_deleteRecordURL != null)
                {
                    string[] urlParts = _deleteRecordURL.Split("?".ToArray());
                    string[] urlParams = urlParts[1].Split("&".ToCharArray());
                    string objectTypeCode = urlParams[0].Replace("etc=", "");
                    entityName = sGetEntityNameFromCode(objectTypeCode, Service);
                    objectId = urlParams[1].Replace("id=", "");
                    Tracing.Trace("ObjectTypeCode=" + objectTypeCode + "--ParentId=" + objectId);
                }
                bool _deleteUsingRecordURL = this.DeleteUsingRecordURL.Get(activityContext);
                String _entityTypeName = this.EntityTypeName.Get(activityContext);
                String _entityGuid = this.EntityGuid.Get(activityContext);

                #endregion

                #region "Delete Record Execution"

                if (_deleteUsingRecordURL)
                {
                    Tracing.Trace("Deleting record by URL: {0}", _deleteRecordURL);

                    if (_deleteRecordURL == null || _deleteRecordURL == "")
                    {
                        throw new InvalidOperationException("ERROR: Delete Record URL to be deleted missing.");
                    }
                    Service.Delete(entityName, new Guid(objectId));
                }
                else
                {
                    Tracing.Trace("Record type to be deleted: " + _entityTypeName + " and ID:" + _entityGuid);
                    if (_entityTypeName == null || _entityTypeName == "" || _entityGuid == null || _entityGuid == "")
                    {
                        throw new InvalidOperationException("ERROR: Entity Type name or GUID to be deleted missing.");
                    }
                    Tracing.Trace("Deleting record by Guid: {0}-{1}", _entityTypeName, _entityGuid);
                    Service.Delete(_entityTypeName, new Guid(_entityGuid));
                }
                #endregion

            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException($"Unexpected error. {ex.Message}", ex);
            }
        }

        public string sGetEntityNameFromCode(string ObjectTypeCode, IOrganizationService service)
        {
            MetadataFilterExpression entityFilter = new MetadataFilterExpression(LogicalOperator.And);
            entityFilter.Conditions.Add(new MetadataConditionExpression("ObjectTypeCode", MetadataConditionOperator.Equals, Convert.ToInt32(ObjectTypeCode)));
            EntityQueryExpression entityQueryExpression = new EntityQueryExpression()
            {
                Criteria = entityFilter
            };
            RetrieveMetadataChangesRequest retrieveMetadataChangesRequest = new RetrieveMetadataChangesRequest()
            {
                Query = entityQueryExpression,
                ClientVersionStamp = null
            };
            RetrieveMetadataChangesResponse response = (RetrieveMetadataChangesResponse)service.Execute(retrieveMetadataChangesRequest);

            EntityMetadata entityMetadata = (EntityMetadata)response.EntityMetadata[0];
            return entityMetadata.SchemaName.ToLower();
        }

    }
}

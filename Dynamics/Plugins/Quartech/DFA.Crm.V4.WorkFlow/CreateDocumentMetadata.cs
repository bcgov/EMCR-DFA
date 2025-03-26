using System;
using System.Activities;
using DFA.Crm.V4.Common;
using DFA.Crm.V4.Common.Extensions;
using DFA.Crm.V4.Common.Model;
using DFA.Crm.V4.Core.bcgov_documenturl.Contract;
using DFA.Crm.V4.Core.bcgov_documenturl.Process;
using DFA.Crm.V4.Data.bcgov_documenturl.Contract;
using DFA.Crm.V4.Data.bcgov_documenturl.Process;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace DFA.Crm.V4.WorkFlow
{
    public class CreateDocumentMetadata : CodeActivity
    {

        [RequiredArgument]
        [Input("RegardingEntitySchemaName")]
        public InArgument<string> RegardingEntitySchemaName { get; set; }

        [RequiredArgument]
        [Input("RegardingEntityID")]
        public InArgument<string> RegardingEntityID { get; set; }

        [RequiredArgument]
        [Input("RegardingEntityLookUpFieldName")]
        public InArgument<string> RegardingEntityLookUpFieldName { get; set; }

        [RequiredArgument]
        [Input("OriginCode")]
        public InArgument<int> OriginCode { get; set; }

        [RequiredArgument]
        [Input("ReceivedDate")]
        public InArgument<DateTime> ReceivedDate { get; set; }

        [RequiredArgument]
        [Input("DocumentSize")]
        public InArgument<Decimal> DocumentSize { get; set; }
        

        [Input("DocumentFileName")]
        public InArgument<string> DocumentFileName { get; set; }

        [Input("Metadata_1")]
        public InArgument<string> Metadata1 { get; set; }

        [Input("Metadata_2")]
        public InArgument<string> Metadata2 { get; set; }

        [Input("Metadata_3")]
        public InArgument<string> Metadata3 { get; set; }

        [Output("DocumentId")]
        [ReferenceTarget("bcgov_documenturl")]
        public OutArgument<EntityReference> DocumentId { get; set; }

        [Output("DerivedFileName")]
        public OutArgument<string> DerivedFileName { get; set; }

        [Output("Result")]
        public OutArgument<bool> Result { get; set; }

        [Output("ErrorMessage")]
        public OutArgument<string> ErrorMessage { get; set; }

        protected override void Execute(CodeActivityContext executionContext)
        {
            var xrmService = new DynamicsWorkflowService(executionContext);

            try
            {
                Result.Set(executionContext, false);

                var documentData = new UploadToS3Request
                {
                    RegardingEntitySchemaName = RegardingEntitySchemaName.Get(executionContext),
                    RegardingEntityID = RegardingEntityID.Get(executionContext),
                    RegardingEntityLookUpFieldName = RegardingEntityLookUpFieldName.Get(executionContext),
                    OriginCode = OriginCode.Get(executionContext) >0 ? OriginCode.Get<int>(executionContext).ToOptionsetValue() : null,
                    Metadata1 = Metadata1.Get(executionContext),
                    Metadata2 = Metadata2.Get(executionContext),
                    Metadata3 = Metadata3.Get(executionContext),
                    DocumentFileName = DocumentFileName.Get(executionContext),
                    ReceivedDate = ReceivedDate.Get(executionContext),
                    DocumentSize = DocumentSize.Get(executionContext),
                };

                if (documentData != null)
                {
                    Ibcgov_documenturlRepository ibcgov_DocumenturlRepository = new bcgov_documenturlRepository(xrmService.SystemService);

                    ICreateDocumentTemplateProcess process = new CreateDocumentTemplateProcess(xrmService, ibcgov_DocumenturlRepository);

                    var respone = process.Execute(documentData);

                    if (respone != null)
                    {
                        DocumentId.Set(executionContext, respone.DocumentTemplate);
                        DerivedFileName.Set(executionContext, respone.DerivedFileName);

                        Result.Set(executionContext, true);
                        ErrorMessage.Set(executionContext, string.Empty);
                    }
                    else
                    {
                        Result.Set(executionContext, false);
                        ErrorMessage.Set(executionContext, String.Empty);
                        DocumentId.Set(executionContext, null);
                        DerivedFileName.Set(executionContext, "");
                    }
                }


            }
            catch (Exception ex)
            {
                throw ex;
                Result.Set(executionContext, false);
                ErrorMessage.Set(executionContext, ex.Message);
                DocumentId.Set(executionContext, null);
                DerivedFileName.Set(executionContext, "");
            }
        }
    }
}

using System;
using System.Activities;
using DFA.Crm.V4.Common;
using DFA.Crm.V4.Common.Model;
using DFA.Crm.V4.Core.S3.Contract;
using DFA.Crm.V4.Core.S3.Process;
using DFA.Crm.V4.Data.bcgov_config.Contract;
using DFA.Crm.V4.Data.bcgov_config.Repository;
using DFA.Crm.V4.Data.S3.Contract;
using DFA.Crm.V4.Data.S3.Repository;
using Microsoft.Xrm.Sdk.Workflow;

namespace DFA.Crm.V3.WorkFlow
{
    public class UploadFileToS3 : CodeActivity
    {


        [RequiredArgument]
        [Input("DocumentContent")]
        public InArgument<string> DocumentContent { get; set; }

        [RequiredArgument]
        [Input("DocumentFileName")]
        public InArgument<string> DocumentFileName { get; set; }

        [RequiredArgument]
        [Input("RegardingEntitySchemaName")]
        public InArgument<string> RegardingEntitySchemaName { get; set; }

        [RequiredArgument]
        [Input("RegardingEntityId")]
        public InArgument<string> RegardingEntityId { get; set; }

        [Output("ErrorMessage")]
        public OutArgument<string> ErrorMessage { get; set; }

        [Output("Result")]
        public OutArgument<bool> Result { get; set; }



        protected override void Execute(CodeActivityContext executionContext)
        {
            var xrmService = new DynamicsWorkflowService(executionContext);

            try
            {
                var documentData = new UploadToS3Request
                {
                    RegardingEntitySchemaName = RegardingEntitySchemaName.Get(executionContext),
                    DerivedFileName = DocumentFileName.Get(executionContext),
                    DocumentContent = DocumentContent.Get(executionContext),
                    RegardingEntityID = RegardingEntityId.Get(executionContext),
                };

                if (documentData != null)
                {
                    Ibcgov_configRepository bcgov_configRepository = new bcgov_configRepository(xrmService.SystemService);

                    IAuthenticationRepository authenticationRepository = new AuthenticationRepository();
                    IS3ProviderRepository s3ProviderRepository = new S3ProviderRepository();

                    IUploadDocumentToS3Process process = new UploadDocumentToS3Process(authenticationRepository, s3ProviderRepository, bcgov_configRepository);

                    var respone = process.Execute(documentData);

                    if (respone != null) {

                        ErrorMessage.Set(executionContext, respone.ErrorMessage);
                        Result.Set(executionContext, respone.Result);

                        return;
                    }

                    ErrorMessage.Set(executionContext, "API Response is null");
                    Result.Set(executionContext, false);
                }


            }
            catch (Exception ex)
            {
                ErrorMessage.Set(executionContext, ex.Message);
                Result.Set(executionContext, false);
            }
        }
    }
}

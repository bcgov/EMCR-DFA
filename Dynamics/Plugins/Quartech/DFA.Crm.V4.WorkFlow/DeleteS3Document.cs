using System;
using System.Activities;
using DFA.Crm.V4.Common;
using DFA.Crm.V4.Common.Model;
using DFA.Crm.V4.Core.Base.Contract;
using DFA.Crm.V4.Core.Base.Process;
using DFA.Crm.V4.Core.S3.Contract;
using DFA.Crm.V4.Core.S3.Process;
using DFA.Crm.V4.Data.Base.Contract;
using DFA.Crm.V4.Data.Base.Repository;
using DFA.Crm.V4.Data.bcgov_config.Contract;
using DFA.Crm.V4.Data.bcgov_config.Repository;
using DFA.Crm.V4.Data.S3.Contract;
using DFA.Crm.V4.Data.S3.Repository;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace DFA.Crm.V3.WorkFlow
{
    public class DeleteS3Document : CodeActivity
    {


        [RequiredArgument]
        [Input("Document Metadata")]
        [ReferenceTarget("bcgov_documenturl")]
        public InArgument<EntityReference> RecordId { get; set; }

        [RequiredArgument]
        [Input("Location")]
        public InArgument<string> Location { get; set; }

        [Output("ErrorMessage")]
        public OutArgument<string> ErrorMessage { get; set; }

        [Output("Result")]
        public OutArgument<bool> Result { get; set; }



        protected override void Execute(CodeActivityContext executionContext)
        {
            var xrmService = new DynamicsWorkflowService(executionContext);

            try
            {
                var metadataId = RecordId.Get<EntityReference>(executionContext);
                var location = Location.Get<string>(executionContext);

                if (metadataId != null && !string.IsNullOrEmpty(location))
                {
                    var fileName = metadataId.Id.ToString().Replace("{","").Replace("}","");

                    Ibcgov_configRepository bcgov_configRepository = new bcgov_configRepository(xrmService.SystemService);
                    IAuthenticationRepository authenticationRepository = new AuthenticationRepository();
                    IS3ProviderRepository s3ProviderRepository = new S3ProviderRepository();

                    IDeleteocumentFromS3Process deleteocumentFromS3Process = new DeleteocumentFromS3Process(authenticationRepository,s3ProviderRepository, bcgov_configRepository);

                    var response = deleteocumentFromS3Process.Execute(fileName, location);

                    if(response == null)
                    {
                        ErrorMessage.Set(executionContext, "An error occured");
                        Result.Set(executionContext, false);
                    }
                    else
                    {
                        ErrorMessage.Set(executionContext, response.ErrorMessage);
                        Result.Set(executionContext, response.Result);
                    }

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

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
using Microsoft.Xrm.Sdk.Workflow;

namespace DFA.Crm.V4.WorkFlow
{
    public class DeleteRecord : CodeActivity
    {


        [RequiredArgument]
        [Input("RecordId (Guid)")]
        public InArgument<string> RecordId { get; set; }

        [RequiredArgument]
        [Input("EntityType")]
        public InArgument<string> EntityType { get; set; }

        [Output("ErrorMessage")]
        public OutArgument<string> ErrorMessage { get; set; }

        [Output("Result")]
        public OutArgument<bool> Result { get; set; }



        protected override void Execute(CodeActivityContext executionContext)
        {
            var xrmService = new DynamicsWorkflowService(executionContext);

            try
            {
                var recordId = RecordId.Get(executionContext);
                var recordType = EntityType.Get(executionContext);

                if (!string.IsNullOrEmpty(recordId) && !string.IsNullOrEmpty(recordType))
                {
                    if (Guid.TryParse(recordId, out Guid id))
                    {

                        IBaseRepository baseRepository = new BaseRepository(xrmService.SystemService);
                        IDeleteRecordProcess deleteRecordProcess = new DeleteRecordProcess(baseRepository);

                        var response = deleteRecordProcess.Delete(recordType, id);
                        Result.Set(executionContext, response);

                        if (response)
                        {
                            ErrorMessage.Set(executionContext, "Success");
                        }
                        else
                        {
                            ErrorMessage.Set(executionContext, "Something went wrong");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Record Id is not parsable to Guid");
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

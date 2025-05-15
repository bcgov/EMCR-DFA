using System;
using System.IO;
using System.Net.Http.Headers;
using DFA.Crm.V4.Common;
using DFA.Crm.V4.Common.Extensions;
using DFA.Crm.V4.Common.Model;
using DFA.Crm.V4.Common.Model.Interface;
using DFA.Crm.V4.Core.bcgov_documenturl.Contract;
using DFA.Crm.V4.Data.bcgov_documenturl.Contract;
using Microsoft.Xrm.Sdk;

namespace DFA.Crm.V4.Core.bcgov_documenturl.Process
{
    public class CreateDocumentTemplateProcess : ICreateDocumentTemplateProcess
    {
        private readonly IDynamicsService workflowService;
        private readonly Ibcgov_documenturlRepository ibcgov_DocumenturlRepository;

        public CreateDocumentTemplateProcess(IDynamicsService workflowService, Ibcgov_documenturlRepository ibcgov_DocumenturlRepository)
        {
            this.workflowService = workflowService;
            this.ibcgov_DocumenturlRepository = ibcgov_DocumenturlRepository;
        }

        public IS3Response Execute(IUploadToS3Request request)
        {
            if(request != null)
            {
                var docTemplateId = Guid.NewGuid();

                var newFileName = docTemplateId.ToString();

                var template = this.GetDocumentUrl(request, docTemplateId);

                Guid recordId = ibcgov_DocumenturlRepository.Create(template);

                return new UploadToS3Response
                {
                    DerivedFileName = newFileName,
                    DocumentTemplate = new EntityReference("bcgov_documenturl", recordId)
                };

            }

            return null;
        }

        public Entity GetDocumentUrl(IUploadToS3Request request, Guid recordId)
        {
            Entity entity = new Entity("bcgov_documenturl");
            entity.Id = recordId;
            
            entity["bcgov_origincode"] = request.OriginCode;

            entity["bcgov_url"] = $"{request.RegardingEntitySchemaName}/{request.RegardingEntityID.ToString()}";

            if (!string.IsNullOrEmpty(request.DocumentFileName))
                entity["bcgov_filename"] = request.DocumentFileName;

            if (!string.IsNullOrEmpty(request.RegardingEntitySchemaName) && !string.IsNullOrEmpty(request.RegardingEntityID) && !string.IsNullOrEmpty(request.RegardingEntityLookUpFieldName))
                entity[request.RegardingEntityLookUpFieldName] = new EntityReference
                {
                    Id = new Guid(request.RegardingEntityID),
                    LogicalName = request.RegardingEntitySchemaName
                };

            if (!string.IsNullOrEmpty(request.Metadata1))
                entity["dfa_category"] = request.Metadata1;

            if (!string.IsNullOrEmpty(request.Metadata2))
                entity["dfa_requireddocumenttype"] = request.Metadata2;

            if (!string.IsNullOrEmpty(request.Metadata3))
                entity["dfa_description"] = request.Metadata3;

            if (request.DocumentSize != 0)
                entity["bcgov_size"] = request.DocumentSize;

            if (request.ReceivedDate > DateTime.MinValue)
                entity["dfa_dateuploaded"] = request.ReceivedDate;

            if (!string.IsNullOrEmpty(request.DocumentFileName))
            {
                entity["bcgov_fileextension"] = Path.GetExtension(request.DocumentFileName);
            }

            return entity;
        }

        public string GetNewRecordFileName(string entityId, string fileName)
        {
            string extension = Path.GetExtension(fileName);
            string newFilename = $"{entityId}{extension}"; 
            return newFilename;
        }
    }
}

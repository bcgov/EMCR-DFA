using System;
using DFA.Crm.V4.Data.bcgov_documenturl.Contract;
using Microsoft.Xrm.Sdk;

namespace DFA.Crm.V4.Data.bcgov_documenturl.Process
{
    public class bcgov_documenturlRepository : Ibcgov_documenturlRepository
    {
        private readonly IOrganizationService service;

        public bcgov_documenturlRepository(IOrganizationService service)
        {
            this.service = service;
        }

        public Guid Create(Entity documentEntity)
        {
           return this.service.Create(documentEntity);
        }
    }
}

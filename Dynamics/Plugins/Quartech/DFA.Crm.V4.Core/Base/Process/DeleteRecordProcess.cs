using System;
using System.Collections.Generic;
using System.Text;
using DFA.Crm.V4.Core.Base.Contract;
using DFA.Crm.V4.Data.Base.Contract;
using Microsoft.Xrm.Sdk;

namespace DFA.Crm.V4.Core.Base.Process
{
    public class DeleteRecordProcess : IDeleteRecordProcess
    {
        private readonly IBaseRepository baseRepository;

        public DeleteRecordProcess(IBaseRepository baseRepository)
        {
            this.baseRepository = baseRepository;
        }
        public bool Delete(string entityType, Guid recordId)
        {
            return this.baseRepository.Delete(entityType, recordId);
        }
    }
}

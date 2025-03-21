using System;
using System.Collections.Generic;
using System.Text;

namespace DFA.Crm.V4.Core.Base.Contract
{
    internal interface IDeleteRecordProcess
    {
        bool Delete(string entityType, Guid recordId);
    }
}

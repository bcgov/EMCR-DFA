using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xrm.Sdk;

namespace DFA.Crm.V4.Data.Base.Contract
{
    public interface IBaseRepository
    {
        bool Delete(string entityType, Guid id);

        Guid Create(Entity project);

        bool Update(Entity project);
    }
}

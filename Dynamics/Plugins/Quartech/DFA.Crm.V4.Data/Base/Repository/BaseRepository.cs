using System;
using System.Collections.Generic;
using System.Text;
using DFA.Crm.V4.Data.Base.Contract;
using Microsoft.Xrm.Sdk;

namespace DFA.Crm.V4.Data.Base.Repository
{
    public class BaseRepository : IBaseRepository
    {
        private readonly IOrganizationService service;

        public BaseRepository(IOrganizationService service)
        {
            this.service = service;
        }

        public Guid Create(Entity project)
        {
            try
            {
                return this.service.Create(project);
            }
            catch (Exception ex) {
                throw new Exception("Exception from base repo +" + ex.Message, ex);

    }
}

        public bool Delete(string entityType, Guid id)
        {
            this.service.Delete(entityType, id);
            return true;
        }

        public bool Update(Entity project)
        {
            this.service.Update(project);
            return true;
        }
    }
}

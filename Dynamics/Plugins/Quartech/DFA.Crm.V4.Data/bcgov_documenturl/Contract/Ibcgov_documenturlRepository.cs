using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xrm.Sdk;

namespace DFA.Crm.V4.Data.bcgov_documenturl.Contract
{
    public interface Ibcgov_documenturlRepository
    {
        Guid Create(Entity documentEntity);
    }
}

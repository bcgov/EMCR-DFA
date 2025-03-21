using System;
using System.Collections.Generic;
using System.Text;

namespace DFA.Crm.V4.Data.S3.Contract
{
    public interface IS3ProviderRepository
    {
        string Upload(string file, string location, string token);
    }
}

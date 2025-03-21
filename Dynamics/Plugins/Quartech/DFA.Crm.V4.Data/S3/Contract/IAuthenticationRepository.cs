using System;
using System.Collections.Generic;
using System.Text;
using DFA.Crm.V4.Common.Model;

namespace DFA.Crm.V4.Data.S3.Contract
{
    public interface IAuthenticationRepository
    {
        AccessToken GetToken(string url, string clientId, string secret);
    }
}

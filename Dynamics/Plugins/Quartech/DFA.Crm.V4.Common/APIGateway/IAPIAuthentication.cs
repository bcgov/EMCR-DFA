using System;
using System.Collections.Generic;
using System.Text;

namespace DFA.Crm.V3.Common.APIGateway
{
    interface IAPIAuthentication
    {
        string GetAuthToken(string url, string clientId, string secret);
    }
}

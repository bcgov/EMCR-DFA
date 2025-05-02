using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xrm.Sdk;

namespace DFA.Crm.V4.Data.bcgov_config.Contract
{
    public interface Ibcgov_configRepository
    {
        Dictionary<string, string> GetAllGroupConfigs(string groupName);
        string GetValue(string keyName);
    }
}

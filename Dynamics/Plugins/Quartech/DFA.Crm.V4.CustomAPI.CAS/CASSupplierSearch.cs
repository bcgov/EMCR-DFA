using DFA.Crm.V4.Core.CAS.Process;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Activities;
using System.Activities.Statements;

namespace DFA.Crm.V4.CustomAPI.CAS
{
    public class CASSupplierSearch : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var casService = new CASIntegration(serviceProvider);
            casService.SearchSupplier();
        }
    }
}

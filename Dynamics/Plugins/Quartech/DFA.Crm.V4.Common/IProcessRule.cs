using System;
using System.Collections.Generic;
using System.Text;

namespace DFA.Crm.V4.Common
{
    public interface IProcessRule
    {
        string[] Messages { get; set; }

        bool IsValid(IDynamicsService dynamicsService);
    }
}

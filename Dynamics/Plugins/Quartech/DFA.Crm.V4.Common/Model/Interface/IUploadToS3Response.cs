using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xrm.Sdk;

namespace DFA.Crm.V4.Common.Model.Interface
{
    public interface IS3Response
    {
        string DerivedFileName { get; set; }

        EntityReference DocumentTemplate { get; set; }

        bool Result { get; set; }

        string ErrorMessage { get; set; }
    }
}

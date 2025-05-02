using System;
using System.Collections.Generic;
using System.Text;

namespace DFA.Crm.V3.Common.Model.Interface
{
    interface ICASInvoiceSearchResponse : ICASBaseSearchResponse
    {
        string Invoice { get; set; }

    }
}

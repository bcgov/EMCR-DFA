using System;
using System.Collections.Generic;
using System.Text;

namespace DFA.Crm.V3.Common.Model.Interface
{
    interface ICASInvoiceSearchRequest
    {
        string InvoiceNumber { get; set; }
        string SupplierNumber { get; set; }
        string SupplierSiteCode { get; set; }

        bool IsValid();
        
    }
}

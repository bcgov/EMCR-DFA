using DFA.Crm.V3.Common.Model.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFA.Crm.V3.Common.Model
{
    class CASInvoiceSearchRequest : ICASInvoiceSearchRequest
    {
        public string InvoiceNumber { get; set; }
        public string SupplierNumber { get; set; }
        public string SupplierSiteCode { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(InvoiceNumber) &&
                   !string.IsNullOrWhiteSpace(SupplierNumber) &&
                   !string.IsNullOrWhiteSpace(SupplierSiteCode);
        }
    }
}

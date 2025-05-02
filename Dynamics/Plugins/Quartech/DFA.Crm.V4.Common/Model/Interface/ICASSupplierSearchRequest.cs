using System;
using System.Collections.Generic;
using System.Text;

namespace DFA.Crm.V3.Common.Model.Interface
{
    interface ICASSupplierSearchRequest
    {
        string BusinessNumber { get; set; }
        string SupplierNumber { get; set; }
        string SiteCode { get; set; }
        string PostalCode { get; set; }
        string SIN { get; set; }
        string SupplierLastName { get; set; }
        string SupplierName { get; set; }
        string PartialSupplierNameWithWildcard { get; set; }
        int SupplierSearchType { get; set; }

        bool IsValid();

    }
}

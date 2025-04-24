using DFA.Crm.V3.Common.Model.Interface;
using DFA.Crm.V4.Common.Model.CAS;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFA.Crm.V3.Common.Model
{
    class CASSupplierSearchRequest : ICASSupplierSearchRequest
    {
        public string BusinessNumber { get; set; }
        public string SupplierNumber { get; set; }
        public string PostalCode { get; set; }
        public string SIN { get; set; }
        public string SiteCode { get; set; }
        public string SupplierLastName { get; set; }
        public string SupplierName { get; set; }
        public string PartialSupplierNameWithWildcard { get; set; }
        public int SupplierSearchType { get; set; }

        public bool IsValid()
        {
            if (SupplierSearchType == (int)CASSupplierSearchType.BusinessNumber)
            {
                return !string.IsNullOrWhiteSpace(BusinessNumber);
            }
            else if (SupplierSearchType == (int)CASSupplierSearchType.SupplierNumber)
            {
                return !string.IsNullOrWhiteSpace(SupplierNumber);
            }
            else if (SupplierSearchType == (int)CASSupplierSearchType.PostalCode)
            {
                return !string.IsNullOrWhiteSpace(PostalCode) && !string.IsNullOrWhiteSpace(SupplierName);
            }
            else if (SupplierSearchType == (int)CASSupplierSearchType.SIN)
            {
                return !string.IsNullOrWhiteSpace(SIN) && !string.IsNullOrWhiteSpace(SupplierLastName);
            }
            else if (SupplierSearchType == (int)CASSupplierSearchType.SiteCode)
            {
                return !string.IsNullOrWhiteSpace(SiteCode) && !string.IsNullOrWhiteSpace(SupplierNumber);
            }
            else if (SupplierSearchType == (int)CASSupplierSearchType.PartialSupplierNameWithWildcard)
            {
                return !string.IsNullOrWhiteSpace(PartialSupplierNameWithWildcard) && PartialSupplierNameWithWildcard.Length >= 4;
            }

            return false;
        }
    }    
}

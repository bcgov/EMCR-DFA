using DFA.Crm.V3.Common.Model.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFA.Crm.V3.Common.Model.CAS
{
    class CASGenerateInvoiceResponse : ICASGenerateInvoiceResponse
    {
        public CASGenerateInvoiceResponse()
        {
            Result = true;
        }
        public bool Result { get; set; }
        public string ErrorMessage { get; set; }
        public string Invoice { get; set; }
        public string APIResult { get; set; }
    }
}

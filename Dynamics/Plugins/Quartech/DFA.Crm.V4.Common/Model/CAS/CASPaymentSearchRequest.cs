using DFA.Crm.V3.Common.Model.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFA.Crm.V3.Common.Model
{
    class CASPaymentSearchRequest : ICASPaymentSearchRequest
    {
        public string PaymentNumber { get; set; }
        public string PayGroup { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(PaymentNumber) &&
                   !string.IsNullOrWhiteSpace(PayGroup);
        }
    }
}

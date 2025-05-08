using System;
using System.Collections.Generic;
using System.Text;

namespace DFA.Crm.V3.Common.Model.Interface
{
    interface ICASPaymentSearchRequest
    {
        string PaymentNumber { get; set; }
        string PayGroup { get; set; }

        bool IsValid();
        
    }
}

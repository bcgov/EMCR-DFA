using DFA.Crm.V3.Common.Model.CAS;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFA.Crm.V3.Common.Model.Interface
{

    interface ICASPaymentSearchResponse : ICASBaseSearchResponse
    {
        CASPayment Payment { get; set; }

    }
}

using DFA.Crm.V3.Common.Model.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace DFA.Crm.V4.Core.CAS.Contract
{
    interface ICASIntegration
    {
        void SearchInvoice();

        void SearchPayment();

        void SearchSupplier();
    }
}

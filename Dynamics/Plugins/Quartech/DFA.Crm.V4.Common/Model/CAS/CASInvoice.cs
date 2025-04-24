using System;
using System.Collections.Generic;
using System.Text;

namespace DFA.Crm.V3.Common.Model
{
    class CASInvoice
    {
        public string invoice_number { get; set; }
        public string invoice_status { get; set; }
        public string payment_status { get; set; }
        public string payment_number { get; set; }
        public string payment_date { get; set; }
    }
}

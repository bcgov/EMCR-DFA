using System;
using System.Collections.Generic;
using System.Text;

namespace DFA.Crm.V3.Common.Model.CAS
{
    class CASPayment
    {
        public string payGroup { get; set; }
        public string paymentDate { get; set; }
        public string paymentAmount { get; set; }
        public string paymentStatus { get; set; }
        public string paymentStatusDate { get; set; }
    }
}

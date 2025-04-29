using System;
using System.Collections.Generic;
using System.Text;

namespace DFA.Crm.V3.Common.Model
{
    public class CASInvoice
    {
        public bool IsBlockSupplier { get; set; }
        public string InvoiceType { get; set; }
        public string SupplierNumber { get; set; }
        public string SupplierSiteNumber { get; set; }
        public string InvoiceDate { get; set; }
        public string InvoiceNumber { get; set; }
        public decimal InvoiceAmount { get; set; }
        public string PayGroup { get; set; }
        public string DateInvoiceReceived { get; set; }
        public string DateGoodsReceived { get; set; }
        public string RemittanceCode { get; set; }
        public bool SpecialHandling { get; set; }
        public string NameLine1 { get; set; }
        public object NameLine2 { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public string QualifiedReceiver { get; set; }
        public string Terms { get; set; }
        public string PayAloneFlag { get; set; }
        public string PaymentAdviceComments { get; set; }
        public string RemittanceMessage1 { get; set; }
        public string RemittanceMessage2 { get; set; }
        public string RemittanceMessage3 { get; set; }
        public string GLDate { get; set; }
        public string InvoiceBatchName { get; set; }
        public string CurrencyCode { get; set; }
        public object AccountNumber { get; set; }
        public object TransitNumber { get; set; }
        public object InstitutionNumber { get; set; }
        public object EFTAdvice { get; set; }
        public object EmailAddress { get; set; }
        public Invoicelinedetail[] InvoiceLineDetails { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(InvoiceType) && !string.IsNullOrEmpty(SupplierNumber) && InvoiceLineDetails != null && InvoiceLineDetails.Length > 0;
        }
    }

    public class Invoicelinedetail
    {
        public int InvoiceLineNumber { get; set; }
        public string InvoiceLineType { get; set; }
        public string LineCode { get; set; }
        public float InvoiceLineAmount { get; set; }
        public string DefaultDistributionAccount { get; set; }
    }
}

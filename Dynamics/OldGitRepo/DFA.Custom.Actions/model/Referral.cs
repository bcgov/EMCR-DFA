namespace EMBC.ERA.Suppliers.Actions.UnInvoiceSubmission.Plugin.model
{
    class Referral 
    {
        public static string ENTITY_NAME = "era_referral";
        internal class Schema
        {
            public static string REFERRAL_NUMBER = "era_referralnumber";//primary field
            public static string TOTAL_GST = "era_totalgst"; //Currency
            public static string TOTAL_AMOUNT = "era_totalamount"; //currency
            public static string RELATED_INVOICE = "era_relatedinvoice";
            public static string INVOICE_REFERENCE = "era_invoicereference";
            public static string SUBMISSION_REFERENCE = "era_submissionreference";
            public static string APPROVED_TOTAL_AMOUNT = "era_approvedtotalamount";
            public static string REFERRAL_INVOICE_RELATIONSHIP = "era_referral_RelatedSupplierInvoice";

        }

    }
}

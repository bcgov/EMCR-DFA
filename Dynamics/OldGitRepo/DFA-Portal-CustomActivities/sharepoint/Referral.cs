namespace DFA.Custom.Actions
{
    class Referral 
    {
        public static string ENTITY_NAME = "dfa_referral";
        internal class Schema
        {
            public static string REFERRAL_NUMBER = "dfa_referralnumber";//primary field
            public static string TOTAL_GST = "dfa_totalgst"; //Currency
            public static string TOTAL_AMOUNT = "dfa_totalamount"; //currency
            public static string RELATED_INVOICE = "dfa_relatedinvoice";
            public static string INVOICE_REFERENCE = "dfa_invoicereference";
            public static string SUBMISSION_REFERENCE = "dfa_submissionreference";
            public static string APPROVED_TOTAL_AMOUNT = "dfa_approvedtotalamount";
            public static string REFERRAL_INVOICE_RELATIONSHIP = "dfa_referral_RelatedSupplierInvoice";

        }

    }
}

namespace DFA.Custom.Actions
{
    class Invoice 
    {
        public static string ENTITY_NAME = "dfa_supplierinvoice";

        internal class Schema
        {
            //Invoice 
            public static string ID = "dfa_supplierinvoiceid"; //primary key
            public static string INVOICE_NUMBER = "dfa_invoicenumber"; //Auto generated field in Dynamics

            public static string SUPPLIER_INVOICE_NUMBER = "dfa_supplierinvoicenumber";
            public static string SUBMISSION_REFERENCE_NUMBER = "dfa_referencenumber";
            public static string INVOICE_REF = "dfa_invoiceref";
            public static string SUPPLIER_INVOICE_DATE = "dfa_invoicedate";
            public static string RELATED_SUPPLIER = "dfa_relatedsupplier";
            public static string REMIT_SUPPLIER = "dfa_remitsupplier";
            public static string LEGAL_BUSINESS_NAME = "dfa_legalbusinessname";
            public static string INVOICE_TYPE = "dfa_invoicetype";
            public static string REMIT_PAYMENT_TO_OTHER_BUSINESS = "dfa_remitpaymenttootherbusiness";
            public static string SUBMITTED_TOTAL_GST = "dfa_totalgst";
            public static string SUBMITTED_TOTAL_INVOICE_AMOUNT = "dfa_totalinvoiceamount";
            public static string STATUS_REASON = "statuscode";
            public static string STATUS = "state";
            public static string APPROVED_TOTAL_AMOUNT = "dfa_approvedtotalamount";

            //Supplier information
            public static string SUPPLIER_LEGAL_NAME = "dfa_supplierlegalname";
            public static string SUPPLIER_NAME = "dfa_suppliername";
            public static string STORE_NUMBER = "dfa_storenumber";
            public static string GST_NUMBER = "dfa_gstnumber";

            //Supplier Address Information
            public static string ADDRESS_LINE1 = "dfa_addressline1";
            public static string ADDRESS_LINE2 = "dfa_addressline2";
            public static string City = "dfa_city";//TODO: to be deleted
        
            public static string RELATED_JURISDICTION = "dfa_relatedjurisdiction"; ////Container for the selected Jurisdiction record selected
            public static string POSTAL_CODE = "dfa_postalcode";
            public static string PROVINCE = "dfa_province";
            public static string COUNTRY = "dfa_country";

            //Supplier Contact person information
            public static string CONTACT_FIRST_NAME = "dfa_contactfirstname";
            public static string CONTACT_LAST_NAME = "dfa_contactlastname";
            public static string CONTACT_EMAIL = "dfa_contactemail";
            public static string CONTACT_NUMBER = "dfa_contactnumber";
            public static string CONTACT_FAX = "dfa_contactfax";

            //Remit to Supplier Address Information
            public static string REMIT_COUNTRY = "dfa_remitcountry";
            public static string REMIT_ADDRESS1 = "dfa_remitaddress1";
            public static string REMIT_ADDRESS2 = "dfa_remitaddress2";
            public static string REMIT_CITY = "dfa_remitcity";
            public static string REMIT_PROVINCE_STATE = "dfa_remitprovincestate";
            public static string REMIT_POSTAL_CODE = "dfa_remitpostalcode";




        }


    }
    class AppApplication {
        public static string ENTITY_NAME = "dfa_appapplication";
        public static string ID = "dfa_appapplicationid"; //primary key
    }
}

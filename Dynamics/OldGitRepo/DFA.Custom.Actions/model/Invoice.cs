namespace EMBC.ERA.Suppliers.Actions.UnInvoiceSubmission.Plugin.model
{
    class Invoice 
    {
        public static string ENTITY_NAME = "era_supplierinvoice";

        internal class Schema
        {
            //Invoice 
            public static string ID = "era_supplierinvoiceid"; //primary key
            public static string INVOICE_NUMBER = "era_invoicenumber"; //Auto generated field in Dynamics

            public static string SUPPLIER_INVOICE_NUMBER = "era_supplierinvoicenumber";
            public static string SUBMISSION_REFERENCE_NUMBER = "era_referencenumber";
            public static string INVOICE_REF = "era_invoiceref";
            public static string SUPPLIER_INVOICE_DATE = "era_invoicedate";
            public static string RELATED_SUPPLIER = "era_relatedsupplier";
            public static string REMIT_SUPPLIER = "era_remitsupplier";
            public static string LEGAL_BUSINESS_NAME = "era_legalbusinessname";
            public static string INVOICE_TYPE = "era_invoicetype";
            public static string REMIT_PAYMENT_TO_OTHER_BUSINESS = "era_remitpaymenttootherbusiness";
            public static string SUBMITTED_TOTAL_GST = "era_totalgst";
            public static string SUBMITTED_TOTAL_INVOICE_AMOUNT = "era_totalinvoiceamount";
            public static string STATUS_REASON = "statuscode";
            public static string STATUS = "state";
            public static string APPROVED_TOTAL_AMOUNT = "era_approvedtotalamount";

            //Supplier information
            public static string SUPPLIER_LEGAL_NAME = "era_supplierlegalname";
            public static string SUPPLIER_NAME = "era_suppliername";
            public static string STORE_NUMBER = "era_storenumber";
            public static string GST_NUMBER = "era_gstnumber";

            //Supplier Address Information
            public static string ADDRESS_LINE1 = "era_addressline1";
            public static string ADDRESS_LINE2 = "era_addressline2";
            public static string City = "era_city";//TODO: to be deleted
        
            public static string RELATED_JURISDICTION = "era_relatedjurisdiction"; ////Container for the selected Jurisdiction record selected
            public static string POSTAL_CODE = "era_postalcode";
            public static string PROVINCE = "era_province";
            public static string COUNTRY = "era_country";

            //Supplier Contact person information
            public static string CONTACT_FIRST_NAME = "era_contactfirstname";
            public static string CONTACT_LAST_NAME = "era_contactlastname";
            public static string CONTACT_EMAIL = "era_contactemail";
            public static string CONTACT_NUMBER = "era_contactnumber";
            public static string CONTACT_FAX = "era_contactfax";

            //Remit to Supplier Address Information
            public static string REMIT_COUNTRY = "era_remitcountry";
            public static string REMIT_ADDRESS1 = "era_remitaddress1";
            public static string REMIT_ADDRESS2 = "era_remitaddress2";
            public static string REMIT_CITY = "era_remitcity";
            public static string REMIT_PROVINCE_STATE = "era_remitprovincestate";
            public static string REMIT_POSTAL_CODE = "era_remitpostalcode";




        }


    }
}

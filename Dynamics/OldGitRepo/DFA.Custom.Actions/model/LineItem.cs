using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMBC.ERA.Suppliers.Actions.UnInvoiceSubmission.Plugin.model
{
    internal class LineItem
    {
        public static string ENTITY_NAME = "era_supportlineitem";
        internal class Schema
        {
    
            public static string RELATED_REFERRAL = "era_relatedreferral";
            public static string SUPPORT_PROVIDED = "era_supportprovided";
            public static string SUPPORT_ITEM_NAME = "era_name";
            public static string APPROVED_AMOUNT = "era_approvedamount";
            public static string DESCRIPTION = "era_description";
            public static string SUBMITTED_GST = "era_gst";
            public static string SUBMITTED_AMOUNT = "era_amount";
            public static string RECEIPT = "era_receipt";
            public static string REFERRAL_REFERENCE = "era_referralreference";
            public static string SUBMISSION_REFERENCE = "era_submissionreference";
            public static string ITEM_STATUS = "era_itemstatus";
            public static string LINEITEM_REFERRAL_RELATIONSHIP = "era_supportlineitem_RelatedReferral";
        }
    }
}

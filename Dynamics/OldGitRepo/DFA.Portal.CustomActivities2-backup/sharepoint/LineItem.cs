using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFA.Portal.Custom.Actions
{
    internal class LineItem
    {
        public static string ENTITY_NAME = "dfa_supportlineitem";
        internal class Schema
        {
    
            public static string RELATED_REFERRAL = "dfa_relatedreferral";
            public static string SUPPORT_PROVIDED = "dfa_supportprovided";
            public static string SUPPORT_ITEM_NAME = "dfa_name";
            public static string APPROVED_AMOUNT = "dfa_approvedamount";
            public static string DESCRIPTION = "dfa_description";
            public static string SUBMITTED_GST = "dfa_gst";
            public static string SUBMITTED_AMOUNT = "dfa_amount";
            public static string RECEIPT = "dfa_receipt";
            public static string REFERRAL_REFERENCE = "dfa_referralreference";
            public static string SUBMISSION_REFERENCE = "dfa_submissionreference";
            public static string ITEM_STATUS = "dfa_itemstatus";
            public static string LINEITEM_REFERRAL_RELATIONSHIP = "dfa_supportlineitem_RelatedReferral";
        }
    }
}

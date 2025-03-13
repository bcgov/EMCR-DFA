using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFA.Portal.Custom.Actions
{
    public static class Constants
    {
        internal class SharePoint
        {
            public static string GROUP_NAME = "SharePoint";

            public static string USERNAME = "UserName";
            public static string PASSWORD = "Password";
            public static string RELYING_PARTY_ID = "RelyingPartyId";
            public static string STS_URL = "STS_URL";
        }

       public enum Entities
        {
            Invoice, 
            Referral
        }
     
    }
}

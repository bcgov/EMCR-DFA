using System;
using System.Collections.Generic;
using System.Text;

namespace DFA.Crm.V4.Common.Model
{
    public class AccessToken
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public int refresh_expires_in { get; set; }
        public string token_type { get; set; }
        public string scope { get; set; }

    }
}

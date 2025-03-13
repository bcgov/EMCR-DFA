using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFA.Custom.Actions
{
    class Attachment
    {
        public static string ENTITY_NAME = "activitymimeattachment";


        internal class Schema
        {
            //Invoice 
            public static string FILE_NAME = "filename";
            public static string SUBJECT = "subject";
            public static string ACTIVITY_SUBJECT = "activitysubject";
            public static string BODY = "body";
        }
    }
}

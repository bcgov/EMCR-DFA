using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DFA.Custom.Actions
{
    class SupplierAction
    {
        internal class InputParams
        {
            public static string APPLICATIONID = "dfa_appapplicationid";
            public static string DOCUMENT_COLLECTION = "documentCollection";

        }

        internal class OutParams
        {
            public static string SUBMISSION_FLAG = "submissionFlag";
            public static string MESSAGE = "message";
   

        }
    }
}

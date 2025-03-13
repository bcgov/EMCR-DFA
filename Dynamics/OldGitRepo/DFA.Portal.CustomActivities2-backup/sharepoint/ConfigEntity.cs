using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFA.Portal.Custom.Actions
{
    class ConfigEntity
    {
        public static string ENTITY_NAME = "dfa_systemconfig";


        internal class Schema
        {
         
            public static string KEY = "dfa_key";
            public static string GROUP = "dfa_group";
            public static string VALUE = "dfa_value";
            public static string SECURE_VALUE = "dfa_securevalue";
            public static string STATE_CODE = "statecode";

        }
    }
}

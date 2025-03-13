using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMBC.ERA.Suppliers.Actions.Copy.model
{
    class ConfigEntity
    {
        public static string ENTITY_NAME = "era_systemconfig";


        internal class Schema
        {
         
            public static string KEY = "era_key";
            public static string GROUP = "era_group";
            public static string VALUE = "era_value";
            public static string SECURE_VALUE = "era_securevalue";
            public static string STATE_CODE = "statecode";

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFA.Custom.Actions
{
    static class Strings
    {
        public static string SUCCESS_MESSAGE = "success";
        public static string GREATER_THAN_1_INVOICE_ERROR = "The \"invoiceCollection\" contains more than 1 invoice records for a Receipt type subission";
        public static string INVALID_PLUGIN_MESSAGE = "Invalid Plugin Execution Exception occured: Message = {0}";
        public static string FAULT_EXCEPTION_MESSAGE = "FaultException occured. Check the passed data field lengths, value types, etc.: {0}";
        public static string CONFIGURATION_NOT_FOUND = "System Configuration for Group '{0}', Key '{1}' doesn't exist..";

    }
}

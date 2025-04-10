using System;
using System.Collections.Generic;
using System.Text;

namespace DFA.Crm.V4.Common.Extensions
{
    public static class Helpers
    {
        public static string GetMIMEType(string fileName)
        {
            //get file extension
            string extension = System.IO.Path.GetExtension(fileName).ToLowerInvariant();

            if (extension.Length > 0 &&
                Constants.Constants.MIMETypesDictionary.ContainsKey(extension.Remove(0, 1)))
            {
                return Constants.Constants.MIMETypesDictionary[extension.Remove(0, 1)];
            }
            return "unknown/unknown";
        }
    }


}

using System;
using System.Collections.Generic;
using System.Text;
using DFA.Crm.V4.Common.Model.Interface;
using Microsoft.Xrm.Sdk;

namespace DFA.Crm.V4.Common.Model
{
    internal class UploadToS3Response : IUploadToS3Response
    {
        public string DerivedFileName { get; set; }
        public EntityReference DocumentTemplate { get; set; }

        public bool Result { get; set; }

        public string ErrorMessage { get; set; }
    }
}

using System;
using System.Activities;
using System.Collections.Generic;
using System.Text;
using DFA.Crm.V4.Common.Model.Interface;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace DFA.Crm.V4.Common.Model
{
    public class UploadToS3Request : IUploadToS3Request
    {
        public string Tag1 { get; set; }
        public string Tag2 { get; set; }
        public string Tag3 { get; set; }
        public string DocumentContent { get; set; }
        public string RegardingEntitySchemaName { get; set; }
        public string RegardingEntityID { get; set; }
        public string RegardingEntityLookUpFieldName { get; set; }
        public OptionSetValue OriginCode { get; set; }
        public string Metadata1 { get; set; }
        public string Metadata2 { get; set; }
        public string Metadata3 { get; set; }
        public string DocumentFileName { get; set; }
        public string DerivedFileName { get; set; }
        public DateTime ReceivedDate { get; set; }
        public Decimal DocumentSize { get; set; }
    }
}
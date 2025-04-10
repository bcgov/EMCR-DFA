using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xrm.Sdk;

namespace DFA.Crm.V4.Common.Model.Interface
{
    public interface IUploadToS3Request
    {
        string Tag1 { get; set; }
        string Tag2 { get; set; }
        string Tag3 { get; set; }
        string DocumentContent { get; set; }
        string RegardingEntitySchemaName { get; set; }
        string RegardingEntityID { get; set; }
        string RegardingEntityLookUpFieldName { get; set; }
        OptionSetValue OriginCode { get; set; }
        string Metadata1 { get; set; }
        string Metadata2 { get; set; }
        string Metadata3 { get; set; }
        string DocumentFileName { get; set; }
        DateTime ReceivedDate { get; set; }
        Decimal DocumentSize { get; set; }
        string DerivedFileName { get; set; }
    }
}

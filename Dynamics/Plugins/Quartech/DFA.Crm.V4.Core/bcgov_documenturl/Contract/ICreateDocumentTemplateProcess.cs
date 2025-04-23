using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using DFA.Crm.V4.Common.Model;
using DFA.Crm.V4.Common.Model.Interface;
using Microsoft.Xrm.Sdk;

namespace DFA.Crm.V4.Core.bcgov_documenturl.Contract
{
    internal interface ICreateDocumentTemplateProcess
    {
        IS3Response Execute(IUploadToS3Request request);

        Entity GetDocumentUrl(IUploadToS3Request request, Guid recordId);

        string GetNewRecordFileName(string entityId, string fileName);

    }
}

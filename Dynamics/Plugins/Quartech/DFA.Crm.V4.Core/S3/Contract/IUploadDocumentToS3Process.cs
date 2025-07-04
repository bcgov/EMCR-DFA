﻿using System;
using System.Collections.Generic;
using System.Text;
using DFA.Crm.V4.Common.Model.Interface;

namespace DFA.Crm.V4.Core.S3.Contract
{
    public interface IUploadDocumentToS3Process
    {
        IS3Response Execute(IUploadToS3Request request);

        string GetAuthToken(string url, string clientId, string secret);

        IS3Response UploadFileToS3(IUploadToS3Request request, string url, string token);


    }
}

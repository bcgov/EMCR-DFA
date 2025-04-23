using System;
using System.Collections.Generic;
using System.Text;
using DFA.Crm.V4.Common.Model.Interface;

namespace DFA.Crm.V4.Core.S3.Contract
{
    public interface IDeleteocumentFromS3Process
    {
        IS3Response Execute(string fileName, string location);

        string GetAuthToken(string url, string clientId, string secret);

        IS3Response DeleteOrTagInS3(string apiUrl, string fileName, string location, string token);


    }
}

using System;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Policy;
using DFA.Crm.V4.Common.Model.Interface;
using DFA.Crm.V4.Core.S3.Contract;
using DFA.Crm.V4.Data.bcgov_config.Contract;
using DFA.Crm.V4.Data.S3.Contract;
using DFA.Crm.V4.Common.Extensions;
using DFA.Crm.V4.Common.Model;

namespace DFA.Crm.V4.Core.S3.Process
{
    public class UploadDocumentToS3Process : IUploadDocumentToS3Process
    {
        private readonly IAuthenticationRepository authenticationRepository;
        private readonly IS3ProviderRepository s3ProviderRepository;
        private readonly Ibcgov_configRepository lbcgov_ConfigRepository;

        public UploadDocumentToS3Process(IAuthenticationRepository authenticationRepository, IS3ProviderRepository s3ProviderRepository, Ibcgov_configRepository lbcgov_ConfigRepository)
        {
            this.authenticationRepository = authenticationRepository;
            this.s3ProviderRepository = s3ProviderRepository;
            this.lbcgov_ConfigRepository = lbcgov_ConfigRepository;
        }

        public IUploadToS3Response Execute(IUploadToS3Request request)
        {
            if (request == null || request?.DerivedFileName == null || request?.DocumentContent == null || request.RegardingEntitySchemaName == null)
                throw new Exception($"Input parameters are null or empty DocumentFileName: {request?.DocumentFileName}, DocumentContent: {request?.DocumentContent}, RegardingEntitySchemaName: {request.RegardingEntitySchemaName}");

            var configuration = lbcgov_ConfigRepository.GetAllGroupConfigs("Storage");

            if(!configuration.TryGetValue("AuthUrl", out string authUrl) || string.IsNullOrEmpty(authUrl))
                throw new Exception("The 'AuthUrl' system configuration is null or missing value for the group 'Storage'");

            if (!configuration.TryGetValue("AuthClientId", out string clientId) || string.IsNullOrEmpty(clientId))
                throw new Exception("The 'AuthClientId' system configuration is null or missing value for the group 'Storage'");

            if (!configuration.TryGetValue("AuthSecret", out string clientSecret) || string.IsNullOrEmpty(clientSecret))
                throw new Exception("The 'AuthSecret' system configuration is null or missing value for the group 'Storage'");

            if (!configuration.TryGetValue("InterfaceUrl", out string uploadAPi) || string.IsNullOrEmpty(uploadAPi))
                throw new Exception("The 'InterfaceUrl' system configuration is null or missing value for the group 'Storage'");


            var authToken = this.GetAuthToken(authUrl, clientId, clientSecret);

            if(authToken == null)
                throw new Exception("Unable to retrive the authentication token for file upload, please make sure the configurations are correct!");

            
            return this.UploadFileToS3(request, uploadAPi, authToken); 
        }

        public string GetAuthToken(string url, string clientId, string secret)
        {
            var accessToken = authenticationRepository.GetToken(url, clientId, secret);

            if(accessToken == null)
                return string.Empty;

            return accessToken.access_token;
        }

        public IUploadToS3Response UploadFileToS3(IUploadToS3Request request, string url, string token)
        {
            using (var fileStream = new System.IO.MemoryStream(Convert.FromBase64String(request.DocumentContent)))
            {
                if (fileStream.Length <= 16384) //16KB
                {
                    if (request.DerivedFileName.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase))
                    {
                        return new UploadToS3Response
                        {
                            Result = false,
                            DerivedFileName = string.Empty,
                            ErrorMessage = "Validation Error, fileStream.Length <= 16384, and name ends with .png"
                        };
                    }
                }

                MultipartFormDataContent multipartContent = new MultipartFormDataContent();

                fileStream.Position = 0;
                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(Helpers.GetMIMEType(request.DerivedFileName));
                multipartContent.Add(streamContent, "File", request.DerivedFileName);
                var postIUrl = $"{url}/api/files/{request.DerivedFileName}";

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                    //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
                    client.DefaultRequestHeaders.Add("file-classification", "Unclassified");
                    client.DefaultRequestHeaders.Add("file-folder", string.Format("{0}/{1}", request.RegardingEntitySchemaName, request.RegardingEntityID));

                    var response = client.PostAsync(postIUrl, multipartContent).Result;

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return new UploadToS3Response
                        {
                            Result = true,
                            DerivedFileName = request.DerivedFileName,
                            ErrorMessage = response.StatusCode.ToString()
                        };
                    }

                    return new UploadToS3Response
                    {
                        Result = false,
                        DerivedFileName = string.Empty,
                        ErrorMessage = $" { response.StatusCode.ToString() } , {response.Content.ReadAsStringAsync().Result}"
                    };
                }
            }
        }       
    }
}

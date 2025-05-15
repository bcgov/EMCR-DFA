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
using System.Threading;
using System.IO.Pipes;

namespace DFA.Crm.V4.Core.S3.Process
{
    public class DeleteocumentFromS3Process : IDeleteocumentFromS3Process
    {
        private readonly IAuthenticationRepository authenticationRepository;
        private readonly IS3ProviderRepository s3ProviderRepository;
        private readonly Ibcgov_configRepository lbcgov_ConfigRepository;

        public DeleteocumentFromS3Process(IAuthenticationRepository authenticationRepository, IS3ProviderRepository s3ProviderRepository, Ibcgov_configRepository lbcgov_ConfigRepository)
        {
            this.authenticationRepository = authenticationRepository;
            this.s3ProviderRepository = s3ProviderRepository;
            this.lbcgov_ConfigRepository = lbcgov_ConfigRepository;
        }

        public IS3Response Execute(string fileName, string location)
        {
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(location))
                throw new Exception($"ExecuteMethod, file name or location parameter is null");


            var configuration = lbcgov_ConfigRepository.GetAllGroupConfigs("Storage");

            if (!configuration.TryGetValue("AuthUrl", out string authUrl) || string.IsNullOrEmpty(authUrl))
                throw new Exception("The 'AuthUrl' system configuration is null or missing value for the group 'Storage'");

            if (!configuration.TryGetValue("AuthClientId", out string clientId) || string.IsNullOrEmpty(clientId))
                throw new Exception("The 'AuthClientId' system configuration is null or missing value for the group 'Storage'");

            if (!configuration.TryGetValue("AuthSecret", out string clientSecret) || string.IsNullOrEmpty(clientSecret))
                throw new Exception("The 'AuthSecret' system configuration is null or missing value for the group 'Storage'");

            if (!configuration.TryGetValue("InterfaceUrl", out string apiEndpoint) || string.IsNullOrEmpty(apiEndpoint))
                throw new Exception("The 'InterfaceUrl' system configuration is null or missing value for the group 'Storage'");


            var authToken = this.GetAuthToken(authUrl, clientId, clientSecret);

            if (authToken == null)
                throw new Exception("Unable to retrive the authentication token for file upload, please make sure the configurations are correct!");


            return this.DeleteOrTagInS3(apiEndpoint,fileName, location, authToken);

        }

        public IS3Response DeleteOrTagInS3(string apiUrl, string fileName, string location, string token)
        {


            var postIUrl = $"{apiUrl}/api/files/{fileName}";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                client.DefaultRequestHeaders.Add("file-classification", "Unclassified");
                client.DefaultRequestHeaders.Add("file-folder", string.Format("{0}", location));

                var response = client.DeleteAsync(postIUrl).Result;

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return new UploadToS3Response
                    {
                        Result = true,
                        DerivedFileName = fileName,
                        ErrorMessage = response.StatusCode.ToString()
                    };
                }

                return new UploadToS3Response
                {
                    Result = false,
                    DerivedFileName = string.Empty,
                    ErrorMessage = $"Response from S3, {response.StatusCode.ToString()}"
                };
            }
        }



        public string GetAuthToken(string url, string clientId, string secret)
        {
            var accessToken = authenticationRepository.GetToken(url, clientId, secret);

            if(accessToken == null)
                return string.Empty;

            return accessToken.access_token;
        }       
    }
}

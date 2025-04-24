using DFA.Crm.V4.Data.bcgov_config.Contract;
using DFA.Crm.V4.Data.S3.Contract;
using DFA.Crm.V4.Data.S3.Repository;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Policy;
using System.Text;

namespace DFA.Crm.V3.Common.APIGateway
{
    class APIAuthentication : IAPIAuthentication
    {
        private readonly IAuthenticationRepository authenticationRepository;
        private readonly Ibcgov_configRepository lbcgov_ConfigRepository;

        public APIAuthentication(IAuthenticationRepository authenticationRepository, Ibcgov_configRepository lbcgov_ConfigRepository)
        {
            this.authenticationRepository = authenticationRepository;
            this.lbcgov_ConfigRepository = lbcgov_ConfigRepository;
        }
        public string GetAuthToken(string url, string clientId, string secret)
        {
            var accessToken = authenticationRepository.GetToken(url, clientId, secret);
            if (accessToken == null)
                return string.Empty;

            return accessToken.access_token;
        }

    }
}

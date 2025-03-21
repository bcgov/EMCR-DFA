using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using DFA.Crm.V4.Data.S3.Contract;
using Microsoft.Xrm.Sdk;
using DFA.Crm.V4.Common.Model;
using System.Runtime.Serialization;
using System.Net.Sockets;
using System.Security.Policy;
using Microsoft.Extensions.Caching.Memory;
using System.Runtime.Caching;

namespace DFA.Crm.V4.Data.S3.Repository
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private static readonly ObjectCache _cache = MemoryCache.Default;
        string cacheKey = "S3Token";
        public AuthenticationRepository()
        {

        }

        public AccessToken GetToken(string url, string clientId, string secret)
        {
            
            var token = _cache.Get(cacheKey) as AccessToken;

            if(token == null)
            {
                var response = GetTokenFromAuthServer(url, clientId, secret);

                CacheItemPolicy policy = new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(response.expires_in - 20) };
                _cache.Set(cacheKey, response, policy);

                return response;
            }
            return token;
        }

        private AccessToken GetTokenFromAuthServer(string url, string clientId, string secret)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);

                var collection = new List<KeyValuePair<string, string>>();
                collection.Add(new KeyValuePair<string, string>("client_id", clientId));
                collection.Add(new KeyValuePair<string, string>("client_secret", secret));
                collection.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
                var content = new FormUrlEncodedContent(collection);
                request.Content = content;

                HttpResponseMessage response = httpClient.SendAsync(request).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var httpResponse = response.Content.ReadAsStringAsync().Result;

                    var jsonReader = System.Runtime.Serialization.Json.JsonReaderWriterFactory.CreateJsonReader(System.Text.Encoding.UTF8.GetBytes(httpResponse), new System.Xml.XmlDictionaryReaderQuotas());

                    var root = System.Xml.Linq.XElement.Load(jsonReader);

                    if (root.Element("access_token") != null)
                    {

                        return new AccessToken
                        {
                            access_token = root.Element("access_token") != null ? root.Element("access_token")?.Value : string.Empty,
                            expires_in = root.Element("access_token") != null ? int.Parse(root.Element("expires_in").Value) : 0,
                            refresh_expires_in = root.Element("access_token") != null ? int.Parse(root.Element("refresh_expires_in").Value) : 0,
                            scope = root.Element("access_token") != null ? root.Element("scope").Value : string.Empty,
                        };

                    }
                    else
                    {
                        throw new InvalidPluginExecutionException("Unable to secure bearer token..");
                    }
                }
            }


            return null;
        }
    }
}
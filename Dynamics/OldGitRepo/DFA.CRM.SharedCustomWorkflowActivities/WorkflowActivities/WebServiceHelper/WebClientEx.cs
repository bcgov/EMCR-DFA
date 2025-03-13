using Microsoft.Xrm.Sdk;
using System;
using System.IO;
using System.Net;

namespace DFA.CRM.CustomWorkflow.WebServiceHelper
{
    public class WebClientEx:WebClient
    {
        public int Timeout { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            request.Timeout = Timeout;
            return request;
        }

        /// <summary>
        /// Send REST Call
        /// URL is where the call is located
        /// parameterData is in the form of param1=data1&param2=data2, etc  
        /// method can be GET, POST, etc
        /// 
        /*using(WebClient client = new WebClient())
        {
            var reqparm = new System.Collections.Specialized.NameValueCollection();
            reqparm.Add("param1", "<any> kinds & of = ? strings");
            reqparm.Add("param2", "escaping is already handled");
            var responseValue = client.UploadValues("http://localhost", "POST", reqparm);
        } */
    /// </summary>
    /// <param name="Tracing"></param>
    /// <param name="url"></param>
    /// <param name="data"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    public bool SendRESTCall(IOrganizationService Service, ITracingService Tracing, string url, string parameterData, string authorizationHeader, bool isTest, string method = "POST")
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            Tracing.Trace("SendRESTCall method at Web Client EX");
            if (string.IsNullOrEmpty(parameterData))
            {
                Tracing.Trace("There is no Parameter, Use Send Web Request instead of Upload String");
                return SendWebRequest(Service, Tracing, url, authorizationHeader, isTest, method);
            }

            bool success;
            // Get REST Call - POST
            try
            {
                Tracing.Trace($"URL: {url}");
                Tracing.Trace($"Method: {method}");
                Tracing.Trace($"Data Value: {parameterData}");
                Tracing.Trace("Call Web Service");
                this.Timeout = 60000;
                this.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                if (!string.IsNullOrEmpty(authorizationHeader))
                {
                    this.Headers.Add(HttpRequestHeader.Authorization, authorizationHeader);
                }
                string responseJSON = this.UploadString(url, method, parameterData);
                Tracing.Trace($"Response JSON: {responseJSON}");
                success = true;
            }
            catch (WebException exception)
            {

                var str = string.Empty;
                if (exception.Response != null)
                {
                    using (var reader = new StreamReader(exception.Response.GetResponseStream()))
                    {
                        str = reader.ReadToEnd();
                    }

                    exception.Response.Close();
                }
                Tracing.Trace($"Inner Exception: {exception.Message}");
                if (exception.Status == WebExceptionStatus.Timeout)
                {
                    Tracing.Trace("The timeout elapsed while attempting to issue the request.");
                }
                else
                {
                    Tracing.Trace($"A Web exception occurred while attempting to issue the request. {exception.Message}: {str}");
                }
                throw;
            }
            return success;
        }

        public string SendWebRequestAndGetResponseString(IOrganizationService Service, ITracingService Tracing, string url, bool isTest, string method = "POST")
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            var uriBuilder = new UriBuilder(url);
            Tracing.Trace($"Create Web Request for URL: {uriBuilder.Uri}");
            
            var request = WebRequest.Create(uriBuilder.Uri);
            if (method == "GET" || isTest)
            {
                Tracing.Trace($"Setting Request METHOD as GET");
                request.Method = WebRequestMethods.Http.Get;
            } else
            {
                Tracing.Trace($"Setting Request METHOD as POST");
                request.Method = WebRequestMethods.Http.Post;
                request.ContentLength = 0;
            }
            request.ContentType = "application/json";
            string responseString;
            try
            {
                Tracing.Trace($"Get Response");
                responseString = GetSyncResponse(Service, Tracing, request, isTest, url, string.Empty);
                Tracing.Trace($"Response String: {responseString}");
            }
            catch (System.Exception ex)
            {
                Tracing.Trace($"Exception has occurred: {ex.Message}");
                throw;
            }
            return responseString;
        }

        public bool SendWebRequest(IOrganizationService Service, ITracingService Tracing, string url, string authorizationHeader, bool isTest, string method = "POST")
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            Tracing.Trace("Use Send Web Request Method instead");
            var uri = new Uri(url);
            Tracing.Trace($"Create Web Request for URL: {uri.AbsoluteUri}");
            var request = WebRequest.Create(uri);
            if (method == "GET" || isTest)
            {
                Tracing.Trace($"Setting Request METHOD as GET");
                request.Method = WebRequestMethods.Http.Get;
            }
            else
            {
                Tracing.Trace($"Setting Request METHOD as POST");
                request.Method = WebRequestMethods.Http.Post;
                request.ContentLength = 0; // We are not posting anything in this request.
            }
            request.ContentType = "application/json";
            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                Tracing.Trace($"Adding Authorization Header: {authorizationHeader}");
                request.Headers.Add(HttpRequestHeader.Authorization, authorizationHeader);
                //request.Headers["Authorization"] = authorizationHeader;
            }

            Tracing.Trace($"Request Authorization Header: {request.Headers["Authorization"]}");
            bool success;
            try
            {
                Tracing.Trace("Get Response");
                /* Get Response Synchorous */
                var jsonResponse = GetSyncResponse(Service, Tracing, request, isTest, url, authorizationHeader);
                Tracing.Trace($"JSON Response: {jsonResponse}");
                success = true;
            }
            catch (System.Exception ex)
            {
                Tracing.Trace($"Exception has occurred: {ex.Message}");
                throw;
            }

            return success;

        }

        protected string GetSyncResponse(IOrganizationService Service, ITracingService Tracing, WebRequest request, bool isTest, string url, string authorizationHeader)
        {
            using (var response = request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var jsonResponse = reader.ReadToEnd();
                    return jsonResponse;
                }
            }
        }
    }
}

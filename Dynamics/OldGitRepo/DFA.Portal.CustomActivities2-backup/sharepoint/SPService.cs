using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using System.Web;
using System.Net.Http;
using System.Xml.XPath;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk;

namespace DFA.Portal.Custom.Actions
{
    public class SPService
    {
        #region Private Members
        private string OdataUri { get; set; }

        private string ApiEndpoint { get
            {
                return OdataUri + "/_api/";
            }
        }

        private HttpClient _Client { get; set; }
        #endregion

        #region Constructor
        public SPService(string stsUrl, string sharePointOdataUrl, string sharePointRelyingPartyIdentifier, string userName, string password)
        {
            var _CookieContainer = new CookieContainer();
            var _HttpClientHandler = new HttpClientHandler() { UseCookies = true, AllowAutoRedirect = false, CookieContainer = _CookieContainer };
            
            _Client = new HttpClient(_HttpClientHandler);
            
            _Client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
            
            if (sharePointOdataUrl.EndsWith("/"))
                OdataUri = sharePointOdataUrl.Substring(0, sharePointOdataUrl.Length - 1);
            else
                OdataUri = sharePointOdataUrl;
            
            var samlST = Authentication.GetStsSamlToken(sharePointRelyingPartyIdentifier, userName, password, stsUrl).GetAwaiter().GetResult();
            
            Authentication.GetFedAuth(OdataUri, samlST, sharePointRelyingPartyIdentifier, _Client).GetAwaiter().GetResult();
            

            var digest = GetDigest(_Client).GetAwaiter().GetResult();
            
            if (digest != null)
                _Client.DefaultRequestHeaders.Add("X-RequestDigest", digest);
            
            // Standard headers for API access
            _Client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
            _Client.DefaultRequestHeaders.Add("OData-Version", "4.0");
        }
        #endregion

        #region Private Methods
        private bool IsValid()
        {
            bool result = false;
            if (!string.IsNullOrEmpty(OdataUri))
            {
                result = true;
            }
            return result;
        }
                
        private string EscapeApostrophe(string filename)
        {
            string result = null;
            if (!string.IsNullOrEmpty(filename))
            {
                result = filename.Replace("'", "''");
            }
            return result;
        }

        private async Task<string> GetDigest(HttpClient client)
        {
            // return early if SharePoint is disabled.
            if (!IsValid())
            {
                return null;
            }

            string result = null;

            HttpRequestMessage endpointRequest = new HttpRequestMessage(HttpMethod.Post, ApiEndpoint + "contextinfo");

            // make the request.
            var response = await client.SendAsync(endpointRequest);
            string jsonString = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.OK && jsonString.Length > 1)
            {
                if (jsonString[0] == '{')
                {
                    using (MemoryStream deserializeStream = new MemoryStream())
                    {
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ContextInfo));

                        StreamWriter writer = new StreamWriter(deserializeStream);
                        writer.Write(jsonString);
                        writer.Flush();
                        deserializeStream.Position = 0;

                        var context = (ContextInfo)serializer.ReadObject(deserializeStream);
                        result = context.FormDigestValue;
                    }
                }
                else
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(jsonString);
                    var digests = doc.GetElementsByTagName("d:FormDigestValue");
                    if (digests.Count > 0)
                    {
                        result = digests[0].InnerText;
                    }
                }

            }

            return result;
        }

        #endregion

        #region Public Methods
        public async Task<bool> CreateFolder(string folderRelativeUrl)
        {
            string odataQuery = String.Format("{0}web/folders/add('{1}')", ApiEndpoint, folderRelativeUrl);

            HttpRequestMessage endpointRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(odataQuery),
                Headers = {
                    { "Accept", "application/json" }
                }
            };
            
            StringContent strContent = new StringContent("", Encoding.UTF8);
            strContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json;odata=verbose");
            endpointRequest.Content = strContent;

            var response = await _Client.SendAsync(endpointRequest);

            if(response.StatusCode != HttpStatusCode.OK)
                throw new Exception("Unable to Add Folder. Error: " + response.Content.ReadAsStringAsync().Result);

            return true;
        }

        public async Task<bool> SetProperty(string folderRelativeUrl,string pop1,string pop2,string pop3, ITracingService tracing)
        {
            folderRelativeUrl = folderRelativeUrl.Replace(OdataUri, string.Empty);

            string newProperty = "{'__metadata': { 'type': 'SP.Data.Dfa_x005f_appapplicationItem'}, 'Required_x0020_Document_x0020_Type':'" + pop1 + "','File_x0020_description':'" + pop2 + "', 'Category':'" + pop3 + "'}";
            string odataQuery = string.Format("{0}web/GetFileByServerRelativeUrl('{1}')/ListItemAllFields", ApiEndpoint, folderRelativeUrl);
            tracing.Trace("odataQuery now is : " + odataQuery);
            tracing.Trace("newProperty now is : " + newProperty);

            _Client.DefaultRequestHeaders.Add("X-HTTP-Method", "MERGE");
            _Client.DefaultRequestHeaders.Add("If-Match", "*");
            _Client.DefaultRequestHeaders.Add("odata-version", "3.0");

            HttpRequestMessage endpointRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(odataQuery),
                Headers = {
                    { "Accept", "application/json" }
                }
            };

            StringContent strContent = new StringContent(newProperty, Encoding.UTF8);
            strContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json;odata=verbose");
            endpointRequest.Content = strContent;

            var response = await _Client.SendAsync(endpointRequest);

           // if (response.StatusCode != HttpStatusCode.OK)
           //     throw new Exception("Unable to Set Property  Error: " + response.Content.ReadAsStringAsync().Result);
            return true;

        }


        public string UploadFile(string folderRelativeUrl, string fileName, MemoryStream contentStream, ITracingService tracing)
        {
            folderRelativeUrl = folderRelativeUrl.Replace(OdataUri, string.Empty);
            tracing.Trace("folderRelative now is : " + folderRelativeUrl);
            string odataQuery = String.Format("{0}Web/GetFolderByServerRelativeUrl('{1}')/files/add(overwrite=true, url='{2}')?@target='{3}'", ApiEndpoint, folderRelativeUrl, fileName, OdataUri);
            tracing.Trace("odataQuery now is : " + odataQuery);
            HttpRequestMessage endpointRequest = new HttpRequestMessage(HttpMethod.Post, odataQuery);
            _Client.DefaultRequestHeaders.Accept.Clear();
            _Client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            ByteArrayContent byteArrayContent = new ByteArrayContent(contentStream.ToArray());
            byteArrayContent.Headers.Add(@"content-length", contentStream.Length.ToString());
            endpointRequest.Content = byteArrayContent;
            tracing.Trace("content of the file is: " + byteArrayContent);
            var response = _Client.SendAsync(endpointRequest).Result;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Uri absoluteUrl = null;
                if (Uri.TryCreate(new Uri(OdataUri), folderRelativeUrl + "/" + fileName, out absoluteUrl))
                    return absoluteUrl.AbsoluteUri;
                else
                    throw new Exception("Unable to combine the file url..");
            }
            else
                throw new Exception("Unable to upload file. Error: " + response.Content.ReadAsStringAsync().Result);
        }


        public void Dispose()
        {
            if (_Client != null)
                _Client.Dispose();
        }

        #endregion
    }

    class DocumentLibraryResponse
    {
        public DocumentLibraryResponseContent d { get; set; }
    }

    class DocumentLibraryResponseContent
    {
        public string Id { get; set; }
    }

    static class Authentication
    {
        private static string GetXMLInnerText(XmlDocument doc, string tagName)
        {
            string result = "";
            var items = doc.GetElementsByTagName(tagName);
            if (items.Count > 0)
            {
                result = items[0].InnerText;
            }
            return result;
        }

        private static string WrapInSoapMessage(string stsResponse, string relyingPartyIdentifier)
        {
            XmlDocument samlAssertion = new XmlDocument();
            samlAssertion.PreserveWhitespace = true;
            samlAssertion.LoadXml(stsResponse);

            //Select the book node with the matching attribute value.

            string notBefore = GetXMLInnerText(samlAssertion, "wsu:Created");
            string notOnOrAfter = GetXMLInnerText(samlAssertion, "wsu:Expires");
            var requestedTokenRaw = samlAssertion.GetElementsByTagName("t:RequestedSecurityToken")[0];

            var requestedTokenData = samlAssertion.ImportNode(requestedTokenRaw, true);

            XmlDocument requestedTokenXml = new XmlDocument();
            requestedTokenXml.PreserveWhitespace = true;
            requestedTokenXml.LoadXml(requestedTokenData.InnerXml);

            XmlDocument soapMessage = new XmlDocument();
            XmlElement soapEnvelope = soapMessage.CreateElement("t", "RequestSecurityTokenResponse", "http://schemas.xmlsoap.org/ws/2005/02/trust");
            soapMessage.AppendChild(soapEnvelope);
            XmlElement lifeTime = soapMessage.CreateElement("t", "Lifetime", soapMessage.DocumentElement.NamespaceURI);
            soapEnvelope.AppendChild(lifeTime);
            XmlElement created = soapMessage.CreateElement("wsu", "Created", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
            XmlText createdValue = soapMessage.CreateTextNode(notBefore);
            created.AppendChild(createdValue);
            lifeTime.AppendChild(created);
            XmlElement expires = soapMessage.CreateElement("wsu", "Expires", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
            XmlText expiresValue = soapMessage.CreateTextNode(notOnOrAfter);
            expires.AppendChild(expiresValue);
            lifeTime.AppendChild(expires);
            XmlElement appliesTo = soapMessage.CreateElement("wsp", "AppliesTo", "http://schemas.xmlsoap.org/ws/2004/09/policy");
            soapEnvelope.AppendChild(appliesTo);
            XmlElement endPointReference = soapMessage.CreateElement("wsa", "EndpointReference", "http://www.w3.org/2005/08/addressing");
            appliesTo.AppendChild(endPointReference);
            XmlElement address = soapMessage.CreateElement("wsa", "Address", endPointReference.NamespaceURI);
            XmlText addressValue = soapMessage.CreateTextNode(relyingPartyIdentifier);
            address.AppendChild(addressValue);
            endPointReference.AppendChild(address);
            XmlElement requestedSecurityToken = soapMessage.CreateElement("t", "RequestedSecurityToken", soapMessage.DocumentElement.NamespaceURI);

            XmlText createdRstValue = soapMessage.CreateTextNode("[RST]");

            requestedSecurityToken.AppendChild(createdRstValue);

            soapEnvelope.AppendChild(requestedSecurityToken);
            XmlElement tokenType = soapMessage.CreateElement("t", "TokenType", soapMessage.DocumentElement.NamespaceURI);
            XmlText tokenTypeValue = soapMessage.CreateTextNode("urn:oasis:names:tc:SAML:1.0:assertion");
            tokenType.AppendChild(tokenTypeValue);
            soapEnvelope.AppendChild(tokenType);
            XmlElement requestType = soapMessage.CreateElement("t", "RequestType", soapMessage.DocumentElement.NamespaceURI);
            XmlText requestTypeValue = soapMessage.CreateTextNode("http://schemas.xmlsoap.org/ws/2005/02/trust/Issue");
            requestType.AppendChild(requestTypeValue);
            soapEnvelope.AppendChild(requestType);
            XmlElement keyType = soapMessage.CreateElement("t", "KeyType", soapMessage.DocumentElement.NamespaceURI);
            XmlText keyTypeValue = soapMessage.CreateTextNode("http://schemas.xmlsoap.org/ws/2005/05/identity/NoProofKey");
            keyType.AppendChild(keyTypeValue);
            soapEnvelope.AppendChild(keyType);

            string result = soapMessage.OuterXml;
            result = result.Replace("[RST]", requestedTokenRaw.InnerXml);
            return result;
        }
        
        public static async Task GetFedAuth(string samlSite, string token, string relyingPartyIdentifier, HttpClient client)
        {
            string samlToken = WrapInSoapMessage(token, relyingPartyIdentifier);

            string samlServer = samlSite.EndsWith("/") ? samlSite : samlSite + "/";
            Uri samlServerRoot = new Uri(samlServer);

            var sharepointSite = new
            {
                Wctx = samlServer + "_layouts/Authenticate.aspx?Source=%2F",
                Wtrealm = samlServer,
                Wreply = samlServerRoot.Scheme + "://" + samlServerRoot.Host + "/_trust/"
            };

            // create the body of the POST
            string stringData = "wa=wsignin1.0&wctx=" + Uri.EscapeDataString(sharepointSite.Wctx).Replace("%20", "+").Replace("'", "%27").Replace("~", "%7E") + "&wresult=" + Uri.EscapeDataString(samlToken).Replace("%20", "+").Replace("'", "%27").Replace("~", "%7E");
            //string stringData = "wa=wsignin1.0&wctx=" + HttpUtility.UrlEncode(sharepointSite.Wctx) + "&wresult=" + HttpUtility.UrlEncode(samlToken);

            var content = new StringContent(stringData, Encoding.UTF8, "application/x-www-form-urlencoded");

            var _httpPostResponse = await client.PostAsync(sharepointSite.Wreply, content);
        }
        
        private static string ParameterizeSoapRequestTokenMsgWithUsernamePassword(string url, string username, string password, string toUrl)
        {
            string samlRTString = "<s:Envelope xmlns:s=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:a=\"http://www.w3.org/2005/08/addressing\" xmlns:u=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\">"
                + "<s:Header>"
                + "<a:Action s:mustUnderstand=\"1\">http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Issue</a:Action>"
                + "<a:ReplyTo><a:Address>http://www.w3.org/2005/08/addressing/anonymous</a:Address></a:ReplyTo>"
                + "<a:To s:mustUnderstand=\"1\">[toUrl]</a:To>"
                + "<o:Security s:mustUnderstand=\"1\" xmlns:o=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\">"
                + "<o:UsernameToken u:Id=\"uuid-6a13a244-dac6-42c1-84c5-cbb345b0c4c4-1\"><o:Username>[username]</o:Username><o:Password>[password]</o:Password></o:UsernameToken></o:Security></s:Header><s:Body>"
                + "<t:RequestSecurityToken xmlns:t=\"http://schemas.xmlsoap.org/ws/2005/02/trust\"><wsp:AppliesTo xmlns:wsp=\"http://schemas.xmlsoap.org/ws/2004/09/policy\">"
                + "<a:EndpointReference><a:Address>[url]</a:Address></a:EndpointReference></wsp:AppliesTo><t:KeyType>http://schemas.xmlsoap.org/ws/2005/05/identity/NoProofKey</t:KeyType>"
                + "<t:RequestType>http://schemas.xmlsoap.org/ws/2005/02/trust/Issue</t:RequestType><t:TokenType>urn:oasis:names:tc:SAML:1.0:assertion</t:TokenType>"
                + "</t:RequestSecurityToken></s:Body></s:Envelope>";


            samlRTString = samlRTString.Replace("[username]", username);
            samlRTString = samlRTString.Replace("[password]", password);
            samlRTString = samlRTString.Replace("[url]", url);
            samlRTString = samlRTString.Replace("[toUrl]", toUrl);

            return samlRTString;
        }

        public async static Task<string> GetStsSamlToken(string spSiteUrl, string username, string password, string stsUrl)
        {
            // Makes a request that conforms with the WS-Trust standard to 
            // the Security Token Service to get a SAML security token back 


            // generate the WS-Trust security token request SOAP message 
            string saml11RT = ParameterizeSoapRequestTokenMsgWithUsernamePassword(
                    spSiteUrl,
                    username,
                    password,
                    stsUrl);

            string response = null;

            if (saml11RT != null)
            {
                var client = new HttpClient();
                var content = new StringContent(saml11RT, System.Text.Encoding.UTF8, "application/soap+xml");
                var result = await client.PostAsync(stsUrl, content);
                response = await result.Content.ReadAsStringAsync();
            }

            return response;
        }
    }
}
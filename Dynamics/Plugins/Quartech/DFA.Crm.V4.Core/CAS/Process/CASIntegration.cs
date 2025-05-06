using DFA.Crm.V3.Common.Model;
using DFA.Crm.V3.Common.Model.CAS;
using DFA.Crm.V3.Common.Model.Interface;
using DFA.Crm.V4.Common.Constants;
using DFA.Crm.V4.Common.Model;
using DFA.Crm.V4.Common.Model.CAS;
using DFA.Crm.V4.Core.CAS.Contract;
using DFA.Crm.V4.Data.bcgov_config.Contract;
using DFA.Crm.V4.Data.bcgov_config.Repository;
using DFA.Crm.V4.Data.S3.Contract;
using DFA.Crm.V4.Data.S3.Repository;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Workflow.Runtime.Tracking;

namespace DFA.Crm.V4.Core.CAS.Process
{
    class CASIntegration : ICASIntegration
    {
        private readonly IExecutionContext _executionContext;
        private readonly IOrganizationService _organizationService;
        private readonly IOrganizationService _systemService;
        private readonly ITracingService _tracingService;
        private readonly IPluginExecutionContext _pluginExecutionContext;

        public CASIntegration(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            // Retrieve the IExecutionContext from the service provider
            this._executionContext = (IExecutionContext)serviceProvider.GetService(typeof(IExecutionContext));

            if (this._executionContext == null)
                throw new InvalidOperationException("Failed to retrieve IExecutionContext from IServiceProvider.");

            // Retrieve the IOrganizationService from the service provider
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            this._organizationService = factory.CreateOrganizationService(_executionContext.UserId);
            this._systemService = factory.CreateOrganizationService(null);
            this._tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            this._pluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
        }
        public void SearchInvoice()
        {
            ICASInvoiceSearchRequest request = new CASInvoiceSearchRequest();
            ICASInvoiceSearchResponse response = new CASInvoiceSearchResponse();

            if (_pluginExecutionContext.MessageName == null || !_pluginExecutionContext.MessageName.Equals("dfa_cas_invoice_search_capi"))
            {
                _tracingService.Trace("MessageName is null or wrong");
                response.Result = false;
                response.ErrorMessage = "MessageName is null or wrong";
            }
            else
            {
                if (_pluginExecutionContext.InputParameters.Contains("dfa_cas_invoice_search_capi_request"))
                {
                    try
                    {
                        var json = _pluginExecutionContext.InputParameters["dfa_cas_invoice_search_capi_request"]?.ToString();

                        request = JsonConvert.DeserializeObject<CASInvoiceSearchRequest>(json);

                        if (!request.IsValid())
                        {
                            _tracingService.Trace("Input parameter is invalid");
                            response.Result = false;
                            response.ErrorMessage = "Input parameter is invalid";
                        }
                        else
                        {
                            Ibcgov_configRepository bcgovConfigRepository = new bcgov_configRepository(_systemService);
                            IAuthenticationRepository authenticationRepository = new AuthenticationRepository();
                            CASInvoiceSearch(bcgovConfigRepository, authenticationRepository, request, ref response);
                        }
                    }
                    catch (JsonException ex)
                    {
                        response.Result = false;
                        response.ErrorMessage = $"Invalid JSON format: {ex.Message}";
                        _tracingService.Trace(response.ErrorMessage);
                    }
                    catch (Exception ex)
                    {
                        response.Result = false;
                        response.ErrorMessage = $"Unexpected error: {ex.Message}";
                        _tracingService.Trace(response.ErrorMessage);
                    }
                }
                else
                {
                    _tracingService.Trace("Input parameter is null or empty");
                    response.Result = false;
                    response.ErrorMessage = "Input parameter is null or empty";
                }
            }

            _pluginExecutionContext.OutputParameters["dfa_cas_invoice_search_capi_response"] = JsonConvert.SerializeObject(response);
        }

        private void CASInvoiceSearch(Ibcgov_configRepository bcgovConfigRepository, IAuthenticationRepository authenticationRepository,
            ICASInvoiceSearchRequest request, ref ICASInvoiceSearchResponse response)
        {
            var configuration = bcgovConfigRepository.GetAllGroupConfigs("OpenShiftAPIGateway");

            if (!TryGetApiConfig(configuration, "OpenShiftAPIGateway", out var apiConfig, out var error))
            {
                response.Result = false;
                response.ErrorMessage = error;
                return;
            }

            if (!TryGetAuthToken(apiConfig, authenticationRepository, out var authToken, out error))
            {
                response.Result = false;
                response.ErrorMessage = error;
                return;
            }
            _tracingService.Trace("Auth token retrieved successfully: " + authToken.access_token);
            CallOpenShiftInvoiceSearchAPI(request, ref response, authToken, apiConfig.InterfaceUrl);
        }
        private void CallOpenShiftInvoiceSearchAPI(ICASInvoiceSearchRequest request, ref ICASInvoiceSearchResponse response,
            AccessToken authToken, string endPoint)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authToken.access_token);

                Constants.CASAPIUrlDictionary.TryGetValue("InvoiceURL", out string searchURL);
                endPoint = $"{endPoint}" + searchURL + request.InvoiceNumber + "/" + request.SupplierNumber + "/" + request.SupplierSiteCode;

                ICASBaseSearchResponse baseResponse = response;
                CheckResponse(_tracingService, "", ref baseResponse, endPoint, client);

                response = (ICASInvoiceSearchResponse)baseResponse;
                response.Invoice = response.APIResult;
            }
        }

        private void CheckResponse(ITracingService _TracingService, string requestBody, ref ICASBaseSearchResponse response, string endPoint, HttpClient client)
        {
            _tracingService.Trace("Sending HTTP request to: " + endPoint);

            var httpResponse = client.GetAsync(endPoint).Result;

            string content = httpResponse.Content.ReadAsStringAsync().Result;
            _tracingService.Trace("HTTP response received.");
            StringBuilder errorMessage = new StringBuilder();

            if (httpResponse == null)
            {
                errorMessage.AppendLine("HTTP response is null.");
            }
            else if (httpResponse.StatusCode != HttpStatusCode.OK)
            {
                _tracingService.Trace("HTTP response is not OK.");
                if (httpResponse.IsSuccessStatusCode)
                {

                }
                errorMessage.AppendLine($"HTTP Status: {(int)httpResponse.StatusCode} {httpResponse.StatusCode}");
                errorMessage.AppendLine($"Reason: {httpResponse.ReasonPhrase}");
                errorMessage.AppendLine($"RequestUri: {httpResponse.RequestMessage?.RequestUri}");
                errorMessage.AppendLine($"Response: {content.Substring(0, content.Length > 3000 ? 3000 : content.Length)}");
                errorMessage.AppendLine($"RequestJson: {requestBody}");

                _TracingService.Trace(errorMessage.ToString());

                response.Result = false;
                response.ErrorMessage = errorMessage.ToString();
            }
            else
            {
                var responseData = httpResponse.Content.ReadAsStringAsync().Result;
                response.Result = true;
                response.APIResult = responseData;
            }
        }

        public void SearchPayment()
        {
            ICASInvoiceSearchRequest request = new CASInvoiceSearchRequest();
            ICASPaymentSearchResponse response = new CASPaymentSearchResponse();

            if (_pluginExecutionContext.MessageName == null || !_pluginExecutionContext.MessageName.Equals("dfa_cas_payment_search_capi"))
            {
                _tracingService.Trace("MessageName is null or wrong");
                response.Result = false;
                response.ErrorMessage = "MessageName is null or wrong";
            }
            else
            {
                if (_pluginExecutionContext.InputParameters.Contains("dfa_cas_payment_search_capi_request"))
                {
                    try
                    {
                        var json = _pluginExecutionContext.InputParameters["dfa_cas_payment_search_capi_request"]?.ToString();

                        request = JsonConvert.DeserializeObject<CASInvoiceSearchRequest>(json);

                        if (!request.IsValid())
                        {
                            _tracingService.Trace("Input parameter is invalid");
                            response.Result = false;
                            response.ErrorMessage = "Input parameter is invalid";
                        }
                        else
                        {
                            Ibcgov_configRepository bcgovConfigRepository = new bcgov_configRepository(_systemService);
                            IAuthenticationRepository authenticationRepository = new AuthenticationRepository();
                            CASPaymentSearch(bcgovConfigRepository, authenticationRepository, request, ref response);
                        }
                    }
                    catch (JsonException ex)
                    {
                        response.Result = false;
                        response.ErrorMessage = $"Invalid JSON format: {ex.Message}";
                        _tracingService.Trace(response.ErrorMessage);
                    }
                    catch (Exception ex)
                    {
                        response.Result = false;
                        response.ErrorMessage = $"Unexpected error: {ex.Message}";
                        _tracingService.Trace(response.ErrorMessage);
                    }
                }
                else
                {
                    _tracingService.Trace("Input parameter is null or empty");
                    response.Result = false;
                    response.ErrorMessage = "Input parameter is null or empty";
                }
            }

            _pluginExecutionContext.OutputParameters["dfa_cas_payment_search_capi_response"] = JsonConvert.SerializeObject(response);

        }
        private void CASPaymentSearch(Ibcgov_configRepository bcgovConfigRepository, IAuthenticationRepository authenticationRepository,
            ICASInvoiceSearchRequest request, ref ICASPaymentSearchResponse response)
        {
            var configuration = bcgovConfigRepository.GetAllGroupConfigs("OpenShiftAPIGateway");

            if (!TryGetApiConfig(configuration, "OpenShiftAPIGateway", out var apiConfig, out var error))
            {
                response.Result = false;
                response.ErrorMessage = error;
                return;
            }

            if (!TryGetAuthToken(apiConfig, authenticationRepository, out var authToken, out error))
            {
                response.Result = false;
                response.ErrorMessage = error;
                return;
            }

            CallOpenShiftPaymentSearchAPI(request, ref response, authToken, apiConfig.InterfaceUrl);
        }
        private void CallOpenShiftPaymentSearchAPI(ICASInvoiceSearchRequest request, ref ICASPaymentSearchResponse response,
            AccessToken authToken, string endPoint)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authToken.access_token);

                Constants.CASAPIUrlDictionary.TryGetValue("PaymentURL", out string searchURL);
                endPoint = $"{endPoint}" + searchURL + request.InvoiceNumber + "/" + request.SupplierNumber + "/" + request.SupplierSiteCode;

                ICASBaseSearchResponse baseResponse = response;
                CheckResponse(_tracingService, "", ref baseResponse, endPoint, client);

                response = (ICASPaymentSearchResponse)baseResponse;
                response.Payment = response.APIResult;
            }
        }
        public void SearchSupplier()
        {
            ICASSupplierSearchRequest request = new CASSupplierSearchRequest();
            ICASSupplierSearchResponse response = new CASSupplierSearchResponse();

            if (_pluginExecutionContext.MessageName == null || !_pluginExecutionContext.MessageName.Equals("dfa_cas_supplier_search_capi"))
            {
                _tracingService.Trace("MessageName is null or wrong");
                response.Result = false;
                response.ErrorMessage = "MessageName is null or wrong";
            }
            else
            {
                if (_pluginExecutionContext.InputParameters.Contains("dfa_cas_supplier_search_capi_request"))
                {
                    try
                    {
                        var json = _pluginExecutionContext.InputParameters["dfa_cas_supplier_search_capi_request"]?.ToString();

                        request = JsonConvert.DeserializeObject<CASSupplierSearchRequest>(json);

                        if (!request.IsValid())
                        {
                            _tracingService.Trace("Input parameter is invalid");
                            response.Result = false;
                            response.ErrorMessage = "Input parameter is invalid";
                        }
                        else
                        {
                            Ibcgov_configRepository bcgovConfigRepository = new bcgov_configRepository(_systemService);
                            IAuthenticationRepository authenticationRepository = new AuthenticationRepository();
                            CASSupplierSearch(bcgovConfigRepository, authenticationRepository, request, ref response);
                        }
                    }
                    catch (JsonException ex)
                    {
                        response.Result = false;
                        response.ErrorMessage = $"Invalid JSON format: {ex.Message}";
                        _tracingService.Trace(response.ErrorMessage);
                    }
                    catch (Exception ex)
                    {
                        response.Result = false;
                        response.ErrorMessage = $"Unexpected error: {ex.Message}";
                        _tracingService.Trace(response.ErrorMessage);
                    }
                }
                else
                {
                    _tracingService.Trace("Input parameter is null or empty");
                    response.Result = false;
                    response.ErrorMessage = "Input parameter is null or empty";
                }
            }

            _pluginExecutionContext.OutputParameters["dfa_cas_supplier_search_capi_response"] = JsonConvert.SerializeObject(response);
        }

        private void CASSupplierSearch(Ibcgov_configRepository bcgovConfigRepository, IAuthenticationRepository authenticationRepository,
            ICASSupplierSearchRequest request, ref ICASSupplierSearchResponse response)
        {
            var configuration = bcgovConfigRepository.GetAllGroupConfigs("OpenShiftAPIGateway");

            if (!TryGetApiConfig(configuration, "OpenShiftAPIGateway", out var apiConfig, out var error))
            {
                response.Result = false;
                response.ErrorMessage = error;
                return;
            }

            if (!TryGetAuthToken(apiConfig, authenticationRepository, out var authToken, out error))
            {
                response.Result = false;
                response.ErrorMessage = error;
                return;
            }

            CallOpenShiftSupplierSearchAPI(request, ref response, authToken, apiConfig.InterfaceUrl);
        }

        private void CallOpenShiftSupplierSearchAPI(ICASSupplierSearchRequest request, ref ICASSupplierSearchResponse response,
            AccessToken authToken, string endPoint)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authToken.access_token);

                string searchURL = string.Empty;

                switch (request.SupplierSearchType)
                {
                    case 0:
                        Constants.CASAPIUrlDictionary.TryGetValue("SupplierBusinessNumber", out searchURL);
                        endPoint = $"{endPoint}{searchURL}{request.BusinessNumber}";
                        break;
                    case 1:
                        Constants.CASAPIUrlDictionary.TryGetValue("SupplierNumber", out searchURL);
                        endPoint = $"{endPoint}{searchURL}{request.SupplierNumber}";
                        break;
                    case 2:
                        Constants.CASAPIUrlDictionary.TryGetValue("SupplierPostalCode", out searchURL);
                        endPoint = $"{endPoint}{searchURL} {request.SupplierName}";
                        break;
                    case 3:
                        Constants.CASAPIUrlDictionary.TryGetValue("SupplierSIN", out searchURL);
                        endPoint = $"{endPoint}{searchURL} {request.SupplierLastName}";
                        break;
                    case 4:
                        Constants.CASAPIUrlDictionary.TryGetValue("SupplierSiteCode", out searchURL);
                        endPoint = $"{endPoint}{searchURL.Replace("suppliernumber", request.SupplierNumber)}{request.SiteCode}";
                        break;
                    case 5:
                        Constants.CASAPIUrlDictionary.TryGetValue("SupplierPartialName", out searchURL);
                        endPoint = $"{endPoint}{searchURL}{request.PartialSupplierNameWithWildcard}";
                        break;
                }
                ICASBaseSearchResponse baseResponse = response;
                CheckResponse(_tracingService, "", ref baseResponse, endPoint, client);

                response = (ICASSupplierSearchResponse)baseResponse;
                response.Suppliers = response.APIResult;
            }
        }

        public void GenerateInvoice(Guid projectclaimid)
        {
            CASInvoice invoice = new CASInvoice()
            {
                InvoiceType = "Standard",
                City = "Vancouver",
                Country = "CA",
                Province = "BC",
                PostalCode = "V2T4T4",
                InvoiceBatchName = "EMCR Test",
                PayGroup = "GEN CHQ",
                RemittanceCode = "01",
                Terms = "Immediate",
                GLDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                DateInvoiceReceived = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                RemittanceMessage1 = "21-03304-VIC-Albert, Ida",
                RemittanceMessage2 = "Income Support-Lost Earning Capacity-Minimum Wage",

                CurrencyCode = "CAD",
                InvoiceLineDetails = new Invoicelinedetail[1]
                {
                    new Invoicelinedetail()
                    {
                        InvoiceLineNumber = 1,
                        InvoiceLineType = "Item",
                        DefaultDistributionAccount = "010.15004.10250.5298.1500000.000000.0000",
                        LineCode = "DR",
                        InvoiceLineAmount = 0
                    }
                }
            };
            ICASGenerateInvoiceResponse response = new CASGenerateInvoiceResponse();
            try
            {
                var fetch = $@"<fetch xmlns:generator='MarkMpn.SQL4CDS'>
                  <entity name='dfa_projectclaim'>
                    <attribute name='dfa_invoicedate' />
                    <attribute name='dfa_invoicenumber' />
                    <attribute name='dfa_invoicetotal' />
                    <attribute name='dfa_suppliernumber' />
                    <attribute name='dfa_site' />
                    <attribute name='dfa_claimtotal' />
                    <attribute name='dfa_claimtype' />
                    <attribute name='dfa_totalnetinvoicedbeingclaimed' />
                    <attribute name='dfa_projectnumber' />
                    <attribute name='dfa_totalactualinvoice' />
                    <attribute name='dfa_suppliername' />
                    <attribute name='dfa_codingblocksubmissionstatus' />
                    <attribute name='dfa_decision' />
                    <filter>
                      <condition attribute='dfa_projectclaimid' operator='eq' value='{projectclaimid}' />
                    </filter>
                  </entity>
                </fetch>";

                var projectClaim = _organizationService.RetrieveMultiple(new FetchExpression(fetch)).Entities?.FirstOrDefault();
                if (!(projectClaim != null && projectClaim.Contains("dfa_decision")
                    && (projectClaim.GetAttributeValue<OptionSetValue>("dfa_decision").Value == 222710000
                    || projectClaim.GetAttributeValue<OptionSetValue>("dfa_decision").Value == 222710003)
                    && projectClaim.Contains("dfa_codingblocksubmissionstatus")
                    && projectClaim.GetAttributeValue<OptionSetValue>("dfa_codingblocksubmissionstatus").Value == 222710002))
                {
                    _tracingService.Trace("Project claim is not in a valid state for invoice generation.");
                    return;
                }
                if (projectClaim != null)
                {
                    if (projectClaim.Contains("dfa_invoicedate"))
                    {
                        invoice.InvoiceDate = projectClaim.GetAttributeValue<DateTime>("dfa_invoicedate").ToString("yyyy-MM-dd");
                    }
                    if (projectClaim.Contains("dfa_invoicenumber"))
                    {
                        invoice.InvoiceNumber = projectClaim.GetAttributeValue<string>("dfa_invoicenumber");
                    }
                    if (projectClaim.Contains("dfa_invoicetotal"))
                    {
                        invoice.InvoiceAmount = projectClaim.GetAttributeValue<Money>("dfa_invoicetotal").Value;
                    }
                    if (projectClaim.Contains("dfa_suppliernumber"))
                    {
                        invoice.SupplierNumber = projectClaim.GetAttributeValue<string>("dfa_suppliernumber");
                    }
                    if (projectClaim.Contains("dfa_site"))
                    {
                        invoice.SupplierSiteNumber = projectClaim.GetAttributeValue<string>("dfa_site");
                    }
                }
                Ibcgov_configRepository bcgovConfigRepository = new bcgov_configRepository(_systemService);
                IAuthenticationRepository authenticationRepository = new AuthenticationRepository();
                CASGenerateInvoice(bcgovConfigRepository, authenticationRepository, invoice, ref response);

                if (response.Result)
                {
                    _tracingService.Trace("Invoice generated successfully. Invoice Number: " + response.Invoice);

                    // Update the project claim with the generated invoice number
                    var updateClaim = new Entity("dfa_projectclaim", projectclaimid)
                    {
                        ["dfa_invoicenumber"] = response.Invoice,
                        ["dfa_invoicedate"] = DateTime.UtcNow,
                        ["dfa_codingblocksubmissionstatus"] = new OptionSetValue(222710003)
                    };
                    _organizationService.Update(updateClaim);
                }
                else
                {
                    var updateClaim = new Entity("dfa_projectclaim", projectclaimid)
                    {
                        ["dfa_codingblocksubmissionstatus"] = new OptionSetValue(222710004),
                        ["dfa_lastcodingblocksubmissionerror"] = response.ErrorMessage
                    };
                    _organizationService.Update(updateClaim);
                    _tracingService.Trace("Failed to generate invoice. Error: " + response.ErrorMessage);
                }

            }
            catch (Exception ex)
            {
                var updateClaim = new Entity("dfa_projectclaim", projectclaimid)
                {
                    ["dfa_codingblocksubmissionstatus"] = new OptionSetValue(222710004),
                    ["dfa_lastcodingblocksubmissionerror"] = response.ErrorMessage
                };
                _organizationService.Update(updateClaim);
                _tracingService.Trace(ex.Message);
            }

        }

        private void CASGenerateInvoice(Ibcgov_configRepository bcgovConfigRepository, IAuthenticationRepository authenticationRepository,
            CASInvoice request, ref ICASGenerateInvoiceResponse response)
        {
            var configuration = bcgovConfigRepository.GetAllGroupConfigs("OpenShiftAPIGateway");

            if (!TryGetApiConfig(configuration, "OpenShiftAPIGateway", out var apiConfig, out var error))
            {
                response.Result = false;
                response.ErrorMessage = error;
                return;
            }

            if (!TryGetAuthToken(apiConfig, authenticationRepository, out var authToken, out error))
            {
                response.Result = false;
                response.ErrorMessage = error;
                return;
            }

            CallOpenShiftGenerateInvoiceAPI(request, ref response, authToken, apiConfig.InterfaceUrl);
        }

        private void CallOpenShiftGenerateInvoiceAPI(CASInvoice request, ref ICASGenerateInvoiceResponse response,
            AccessToken authToken, string endPoint)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authToken.access_token);

                Constants.CASAPIUrlDictionary.TryGetValue("GenerateInvoiceURL", out string searchURL);

                endPoint = $"{endPoint}" + searchURL;
                // Convert the request object into JSON content
                var jsonContent = JsonConvert.SerializeObject(request);
                var httpContent = new System.Net.Http.StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                ICASBaseSearchResponse baseResponse = response;
                CheckResponse(_tracingService, "", ref baseResponse, endPoint, client);

                response = (ICASGenerateInvoiceResponse)baseResponse;
                response.Invoice = response.APIResult;

                var responseData = response.APIResult;
                string invoiceNumber = null;

                if (!string.IsNullOrWhiteSpace(responseData))
                {
                    var jObject = JObject.Parse(responseData);

                    invoiceNumber = jObject["invoice_number"]?.ToString();
                    response.ErrorMessage = jObject["CAS-Returned-Messages"]?.ToString();
                }
                if (response.ErrorMessage == "SUCCEEDED")
                    response.Result = true;
                else
                {
                    if (response.ErrorMessage == null)
                    {
                        string type = null;
                        string title = null;
                        int status = 0;
                        string traceId = null;
                        string errorKey = null;
                        string errorMessage = null;

                        var obj = JObject.Parse(responseData);

                        type = obj["type"]?.ToString();
                        title = obj["title"]?.ToString();
                        status = obj["status"]?.ToObject<int>() ?? 0;
                        traceId = obj["traceId"]?.ToString();

                        var errors = obj["errors"] as JObject;
                        if (errors != null && errors.Properties().Any())
                        {
                            var firstError = errors.Properties().First();
                            errorKey = firstError.Name;
                            var messages = firstError.Value as JArray;
                            if (messages != null && messages.Count > 0)
                            {
                                errorMessage = messages[0]?.ToString();
                            }
                        }

                        response.ErrorMessage = errorMessage;
                    }
                    response.Result = false;
                }
                response.Invoice = invoiceNumber;

            }
        }

        private string ExtractSupplierArray(string json)
        {
            try
            {
                var token = JToken.Parse(json);

                if (token.Type == JTokenType.Array)
                {
                    return json;
                }
                else if (token.Type == JTokenType.Object && token["items"] is JArray itemsArray)
                {
                    return itemsArray.ToString();
                }
            }
            catch (JsonException ex)
            {
                _tracingService.Trace($"Failed to parse supplier JSON: {ex.Message}");
            }

            return null;
        }

        private bool TryGetApiConfig(Dictionary<string, string> config, string groupName, out ApiConfig apiConfig, out string errorMessage)
        {
            apiConfig = null;
            errorMessage = null;

            bool Try(string key, out string value, ref string error)
            {
                if (!config.TryGetValue(key, out value) || string.IsNullOrEmpty(value))
                {
                    error = $"The '{key}' system configuration is null or missing value for the group '{groupName}'";
                    return false;
                }
                return true;
            }

            if (!Try("AuthUrl", out string authUrl, ref errorMessage) ||
                !Try("AuthClientId", out string clientId, ref errorMessage) ||
                !Try("AuthSecret", out string clientSecret, ref errorMessage) ||
                !Try("InterfaceUrl", out string interfaceUrl, ref errorMessage))
            {
                return false;
            }

            apiConfig = new ApiConfig
            {
                AuthUrl = authUrl,
                ClientId = clientId,
                ClientSecret = clientSecret,
                InterfaceUrl = interfaceUrl
            };

            return true;
        }

        private bool TryGetAuthToken(ApiConfig config, IAuthenticationRepository authRepo, out AccessToken token, out string error)
        {
            try
            {
                token = authRepo.GetToken(config.AuthUrl, config.ClientId, config.ClientSecret);
                if (token == null)
                {
                    error = "Unable to retrieve the authentication token, please make sure the configurations are correct!";
                    return false;
                }

                error = null;
                return true;
            }
            catch (Exception ex)
            {
                _tracingService.Trace("Exception in TryGetAuthToken: " + ex.Message);
                token = null;
                error = "Unable to retrieve the authentication token, please make sure the configurations are correct!";
                return false;
            }
        }


        private class ApiConfig
        {
            public string AuthUrl { get; set; }
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
            public string InterfaceUrl { get; set; }
            public string IsProduction { get; set; }
        }
    }


}



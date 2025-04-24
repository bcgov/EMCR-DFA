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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows.Documents;

namespace DFA.Crm.V4.Core.CAS.Process
{
    class CASIntegration : ICASIntegration
    {
        private readonly IExecutionContext _executionContext;
        private readonly IOrganizationService _organizationService;
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
                    request = JsonConvert.DeserializeObject<CASInvoiceSearchRequest>(_pluginExecutionContext.InputParameters["dfa_cas_invoice_search_capi_request"].ToString());
                    if (!request.IsValid())
                    {
                        _tracingService.Trace("Input parameter is invalid");
                        response.Result = false;
                        response.ErrorMessage = "Input parameter is invalid";
                    }
                    else
                    {
                        Ibcgov_configRepository bcgovConfigRepository = new bcgov_configRepository(_organizationService);
                        IAuthenticationRepository authenticationRepository = new AuthenticationRepository();
                        try
                        {
                            CASInvoiceSearch(bcgovConfigRepository, authenticationRepository, request, ref response);
                        }
                        catch (Exception ex)
                        {
                            _tracingService.Trace("Exception occurred: " + ex.Message);
                            response.Result = false;
                            response.ErrorMessage = "An error occurred while processing the request: " + ex.Message;
                        }
                        
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

            if (!configuration.TryGetValue("AuthUrl", out string authUrl) || string.IsNullOrEmpty(authUrl))
            {
                response.Result = false;
                response.ErrorMessage = "The 'AuthUrl' system configuration is null or missing value for the group 'Storage'";
                return;
            }

            if (!configuration.TryGetValue("AuthClientId", out string clientId) || string.IsNullOrEmpty(clientId))
            {
                response.Result = false;
                response.ErrorMessage = "The 'AuthClientId' system configuration is null or missing value for the group 'Storage'";
                return;
            }
            if (!configuration.TryGetValue("AuthSecret", out string clientSecret) || string.IsNullOrEmpty(clientSecret))
            {
                response.Result = false;
                response.ErrorMessage = "The 'AuthSecret' system configuration is null or missing value for the group 'Storage'";
                return;
            }
            if (!configuration.TryGetValue("InterfaceUrl", out string uploadAPi) || string.IsNullOrEmpty(uploadAPi))
            {
                response.Result = false;
                response.ErrorMessage = "The 'InterfaceUrl' system configuration is null or missing value for the group 'Storage'";
                return;
            }

            var authToken = authenticationRepository.GetToken(authUrl, clientId, clientSecret);

            if (authToken == null)
            {
                response.Result = false;
                response.ErrorMessage = "Unable to retrieve the authentication token for file upload, please make sure the configurations are correct!";
                return;
            }

            CallOpenShiftInvoiceSearchAPI(request,  ref response, authToken, uploadAPi);
        }
        private void CallOpenShiftInvoiceSearchAPI(ICASInvoiceSearchRequest request, ref ICASInvoiceSearchResponse response,
            AccessToken authToken, string endPoint)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authToken);

                Constants.CASAPIUrlDictionary.TryGetValue("InvoiceSearch", out string searchURL);

                endPoint = $"{endPoint}/api/" + searchURL + request.InvoiceNumber + "/" + request.SupplierNumber + "/" + request.SupplierSiteCode;

                var httpResponse = client.GetAsync(endPoint).Result;

                if (!httpResponse.IsSuccessStatusCode)
                {
                    response.Result = false;
                    response.ErrorMessage = $"Failed to retrieve data from Open Shift API. HTTP Status: {httpResponse.StatusCode}, Reason: {httpResponse.ReasonPhrase}";
                }
                else
                {
                    var responseData = httpResponse.Content.ReadAsStringAsync().Result;
                    var invoice = Newtonsoft.Json.JsonConvert.DeserializeObject<CASInvoice>(responseData);

                    response.Result = true;
                    response.Invoice = invoice;
                }
            }
        }

        private void CASPaymentSearch(Ibcgov_configRepository bcgovConfigRepository, IAuthenticationRepository authenticationRepository,
            ICASInvoiceSearchRequest request, ref ICASPaymentSearchResponse response)
        {
            var configuration = bcgovConfigRepository.GetAllGroupConfigs("OpenShiftAPIGateway");

            if (!configuration.TryGetValue("AuthUrl", out string authUrl) || string.IsNullOrEmpty(authUrl))
            {
                response.Result = false;
                response.ErrorMessage = "The 'AuthUrl' system configuration is null or missing value for the group 'Storage'";
                return;
            }

            if (!configuration.TryGetValue("AuthClientId", out string clientId) || string.IsNullOrEmpty(clientId))
            {
                response.Result = false;
                response.ErrorMessage = "The 'AuthClientId' system configuration is null or missing value for the group 'Storage'";
                return;
            }
            if (!configuration.TryGetValue("AuthSecret", out string clientSecret) || string.IsNullOrEmpty(clientSecret))
            {
                response.Result = false;
                response.ErrorMessage = "The 'AuthSecret' system configuration is null or missing value for the group 'Storage'";
                return;
            }
            if (!configuration.TryGetValue("InterfaceUrl", out string uploadAPi) || string.IsNullOrEmpty(uploadAPi))
            {
                response.Result = false;
                response.ErrorMessage = "The 'InterfaceUrl' system configuration is null or missing value for the group 'Storage'";
                return;
            }

            var authToken = authenticationRepository.GetToken(authUrl, clientId, clientSecret);

            if (authToken == null)
            {
                response.Result = false;
                response.ErrorMessage = "Unable to retrieve the authentication token for file upload, please make sure the configurations are correct!";
                return;
            }

            CallOpenShiftPaymentSearchAPI(request, ref response, authToken, uploadAPi);
        }
        private void CallOpenShiftPaymentSearchAPI(ICASInvoiceSearchRequest request, ref ICASPaymentSearchResponse response,
            AccessToken authToken, string endPoint)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authToken);

                Constants.CASAPIUrlDictionary.TryGetValue("PaymentSearch", out string searchURL);

                endPoint = $"{endPoint}/api/" + searchURL + request.InvoiceNumber + "/" + request.SupplierNumber + "/" + request.SupplierSiteCode;

                //var httpResponse = client.GetAsync(endPoint).Result;

                //if (!httpResponse.IsSuccessStatusCode)
                //{
                //    response.Result = false;
                //    response.ErrorMessage = $"Failed to retrieve data from Open Shift API. HTTP Status: {httpResponse.StatusCode}, Reason: {httpResponse.ReasonPhrase}";
                //}

                //var responseData = httpResponse.Content.ReadAsStringAsync().Result;es
                var responseData = $@"{{ 
                  ""paymentNumber"": ""12345678"", 
                  ""payGroup"": ""ODP CHQ"", 
                  ""paymentDate"": ""04-OCT-2018 00:00:00"", 
                  ""paymentAmount"": ""99.67"", 
                  ""paymentStatus"": ""VOIDED"",  
                  ""paymentStatusDate"": ""06-OCT-2018 00:00:00""         
                }}";
                var payment = Newtonsoft.Json.JsonConvert.DeserializeObject<CASPayment>(responseData);

                response.Result = true;

                // No other changes are needed in the file as the issue is caused by the missing namespace reference.
                response.Payment = payment;
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
                    request = JsonConvert.DeserializeObject<CASInvoiceSearchRequest>(_pluginExecutionContext.InputParameters["dfa_cas_payment_search_capi_request"].ToString());
                    if (!request.IsValid())
                    {
                        _tracingService.Trace("Input parameter is invalid");
                        response.Result = false;
                        response.ErrorMessage = "Input parameter is invalid";
                    }
                    else
                    {
                        Ibcgov_configRepository bcgovConfigRepository = new bcgov_configRepository(_organizationService);
                        IAuthenticationRepository authenticationRepository = new AuthenticationRepository();
                        CASPaymentSearch(bcgovConfigRepository, authenticationRepository, request, ref response);
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
                    request = JsonConvert.DeserializeObject<CASSupplierSearchRequest>(_pluginExecutionContext.InputParameters["dfa_cas_supplier_search_capi_request"].ToString());
                    if (!request.IsValid())
                    {
                        _tracingService.Trace("Input parameter is invalid");
                        response.Result = false;
                        response.ErrorMessage = "Input parameter is invalid";
                    }
                    else
                    {
                        Ibcgov_configRepository bcgovConfigRepository = new bcgov_configRepository(_organizationService);
                        IAuthenticationRepository authenticationRepository = new AuthenticationRepository();
                        CASSupplierSearch(bcgovConfigRepository, authenticationRepository, request, ref response);
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

            if (!configuration.TryGetValue("AuthUrl", out string authUrl) || string.IsNullOrEmpty(authUrl))
            {
                response.Result = false;
                response.ErrorMessage = "The 'AuthUrl' system configuration is null or missing value for the group 'Storage'";
                return;
            }

            if (!configuration.TryGetValue("AuthClientId", out string clientId) || string.IsNullOrEmpty(clientId))
            {
                response.Result = false;
                response.ErrorMessage = "The 'AuthClientId' system configuration is null or missing value for the group 'Storage'";
                return;
            }
            if (!configuration.TryGetValue("AuthSecret", out string clientSecret) || string.IsNullOrEmpty(clientSecret))
            {
                response.Result = false;
                response.ErrorMessage = "The 'AuthSecret' system configuration is null or missing value for the group 'Storage'";
                return;
            }
            if (!configuration.TryGetValue("InterfaceUrl", out string uploadAPi) || string.IsNullOrEmpty(uploadAPi))
            {
                response.Result = false;
                response.ErrorMessage = "The 'InterfaceUrl' system configuration is null or missing value for the group 'Storage'";
                return;
            }

            var authToken = authenticationRepository.GetToken(authUrl, clientId, clientSecret);

            if (authToken == null)
            {
                response.Result = false;
                response.ErrorMessage = "Unable to retrieve the authentication token for file upload, please make sure the configurations are correct!";
                return;
            }

            CallOpenShiftSupplierSearchAPI(request, ref response, authToken, uploadAPi);
        }

        private void CallOpenShiftSupplierSearchAPI(ICASSupplierSearchRequest request, ref ICASSupplierSearchResponse response,
            AccessToken authToken, string endPoint)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authToken);

                string searchURL = string.Empty;

                switch (request.SupplierSearchType)
                {
                    case 0:
                        Constants.CASAPIUrlDictionary.TryGetValue("SupplierBusinessNumber", out searchURL);
                        endPoint = $"{endPoint}/api/{searchURL}{request.BusinessNumber}";
                        break;
                    case 1:
                        Constants.CASAPIUrlDictionary.TryGetValue("SupplierNumber", out searchURL);
                        endPoint = $"{endPoint}/api/{searchURL}{request.SupplierNumber}";
                        break;
                    case 2:
                        Constants.CASAPIUrlDictionary.TryGetValue("SupplierPostalCode", out searchURL);
                        endPoint = $"{endPoint}/api/{searchURL}{request.SupplierName}/{request.PostalCode}";
                        break;
                    case 3:
                        Constants.CASAPIUrlDictionary.TryGetValue("SupplierSIN", out searchURL);
                        endPoint = $"{endPoint}/api/{searchURL}{request.SupplierLastName}/{request.SIN}";
                        break;
                    case 4:
                        Constants.CASAPIUrlDictionary.TryGetValue("SupplierSiteCode", out searchURL);
                        endPoint = $"{endPoint}/api/{searchURL}{request.SupplierNumber}/{request.SiteCode}";
                        break;
                    case 5:
                        Constants.CASAPIUrlDictionary.TryGetValue("SupplierPartialName", out searchURL);
                        endPoint = $"{endPoint}/api/{searchURL}{request.PartialSupplierNameWithWildcard}";
                        break;
                }

                _tracingService.Trace(endPoint);

                //var httpResponse = client.GetAsync(endPoint).Result;

                //if (!httpResponse.IsSuccessStatusCode)
                //{
                //    response.Result = false;
                //    response.ErrorMessage = $"Failed to retrieve data from Open Shift API. HTTP Status: {httpResponse.StatusCode}, Reason: {httpResponse.ReasonPhrase}";
                //}

                //var responseData = httpResponse.Content.ReadAsStringAsync().Result;es
                var responseData = $@"[
	{{
		""suppliernumber"": ""2929295"",
		""suppliername"": ""FRIDAY, MONDAY"",
		""subcategory"": ""Business"",
		""sin"": null,
		""providerid"": null,
		""businessnumber"": ""936922087"",
		""status"": ""ACTIVE"",
		""supplierprotected"": null,
		""standardindustryclassification"": null,
		""lastupdated"": ""2021-06-08 08:44:41"",
		""SupplierAddress"": [
			{{
				""SupplierSiteCode"": ""001"",
				""AddressLine1"": ""3350 CLEARBROOK RD"",
				""AddressLine2"": """",
				""AddressLine3"": """",
				""City"": ""ABBOTSFORD"",
				""Province"": ""BC"",
				""Country"": ""CA"",
				""PostalCode"": ""V2T4T4"",
				""EmailAddress"": ""abc@xyz.com"",
				""EftAdvicePref"": ""E"",
				""Status"": ""ACTIVE"",
				""SiteProtected"": ""N"",
				""LastUpdated"": ""2019-11-13 11:09:52""
			}},
			{{
				""SupplierSiteCode"": ""002"",
				""AddressLine1"": ""3351 CLEARBROOK RD"",
				""AddressLine2"": """",
				""AddressLine3"": """",
				""City"": ""ABBOTSFORD"",
				""Province"": ""BC"",
				""Country"": ""CA"",
				""PostalCode"": ""V2T4T4"",
				""EmailAddress"": ""abc@xyz.com"",
				""EftAdvicePref"": ""E"",
				""ProviderId"": """",
				""Status"": ""ACTIVE"",
				""SiteProtected"": ""N"",
				""LastUpdated"": ""2019-11-13 11:09:52""
			}}
		]
	}},
	{{
		""suppliernumber"": ""2925295"",
		""suppliername"": ""ABC MONDAY"",
		""subcategory"": ""Business"",
		""sin"": null,
		""providerid"": null,
		""businessnumber"": ""936922087"",
		""status"": ""ACTIVE"",
		""supplierprotected"": null,
		""standardindustryclassification"": null,
		""lastupdated"": ""2021-06-08 08:44:41"",
		""SupplierAddress"": [
			{{
				""SupplierSiteCode"": ""001"",
				""AddressLine1"": ""3350 Whilster RD"",
				""AddressLine2"": """",
				""AddressLine3"": """",
				""City"": ""ABBOTSFORD"",
				""Province"": ""BC"",
				""Country"": ""CA"",
				""PostalCode"": ""V2T4T4"",
				""EmailAddress"": ""abc@xyz.com"",
				""EftAdvicePref"": ""E"",
				""Status"": ""ACTIVE"",
				""SiteProtected"": ""N"",
				""LastUpdated"": ""2019-11-13 11:09:52""
			}}
		]
	}}
]";
                try
                {
                    var supplier = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CASSupplier>>(responseData);

                    response.Result = true;

                    // No other changes are needed in the file as the issue is caused by the missing namespace reference.
                    response.Suppliers = supplier;
                }
                catch (Exception)
                {
                    var temp = $@"[
	{{
		""suppliernumber"": ""2929295"",
		""suppliername"": ""FRIDAY, MONDAY"",
		""subcategory"": ""Business"",
		""sin"": null,
		""providerid"": null,
		""businessnumber"": ""936922087"",
		""status"": ""ACTIVE"",
		""supplierprotected"": null,
		""standardindustryclassification"": null,
		""lastupdated"": ""2021-06-08 08:44:41""
	}},
	{{
		""suppliernumber"": ""2925295"",
		""suppliername"": ""ABC MONDAY"",
		""subcategory"": ""Business"",
		""sin"": null,
		""providerid"": null,
		""businessnumber"": ""936922087"",
		""status"": ""ACTIVE"",
		""supplierprotected"": null,
		""standardindustryclassification"": null,
		""lastupdated"": ""2021-06-08 08:44:41""
	}}
]";
                    var supplier = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CASSupplier>>(temp);

                    response.Result = true;

                    // No other changes are needed in the file as the issue is caused by the missing namespace reference.
                    response.Suppliers = supplier;
                }
                
            }
        }
    }
}


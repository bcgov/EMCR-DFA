using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.ServiceModel;
namespace DFA.Portal.Custom.Actions
{
    public class DFAUploadDocuments : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
 
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            bool submissionFlag = false;
            string message = string.Empty;
            try
            {
                EntityCollection input_DocumentCollection = (EntityCollection)context.InputParameters[SupplierAction.InputParams.DOCUMENT_COLLECTION];
                var appApplicationId = (string)context.InputParameters[SupplierAction.InputParams.APPLICATIONID];
                var description = (string)context.InputParameters["dfa_description"];
                var modifiedby = (string)context.InputParameters["dfa_modifiedby"];
                var fileType = (string)context.InputParameters["fileType"];
                var requireddocumenttype = (string)context.InputParameters["dfa_requireddocumenttype"];

                tracingService.Trace("application id recvd: " + appApplicationId);
                tracingService.Trace("description: " + description);
                tracingService.Trace("modified by: " + modifiedby);
                tracingService.Trace("fileType: " + fileType);
                tracingService.Trace("requireddocumenttype: " + requireddocumenttype);

                EntityCollection applicationAttachment = new EntityCollection();
                if (context.InputParameters[SupplierAction.InputParams.DOCUMENT_COLLECTION] != null)
                {
                    foreach (Entity item in input_DocumentCollection.Entities)
                    {
                        ValidateAttachment(item);
                        applicationAttachment.Entities.Add(item);
                    }
                }

                if (context.InputParameters[SupplierAction.InputParams.DOCUMENT_COLLECTION] != null)
                {
                 
                    List<Entity> sharePointConfigs = Helper.GetSystemConfigurations(service, Constants.SharePoint.GROUP_NAME, null);
                    string userName = Helper.GetConfigKeyValue(sharePointConfigs, Constants.SharePoint.USERNAME, Constants.SharePoint.GROUP_NAME);
                    string password = Helper.GetSecureConfigKeyValue(sharePointConfigs, Constants.SharePoint.PASSWORD, Constants.SharePoint.GROUP_NAME);
                    string stsUrl = Helper.GetConfigKeyValue(sharePointConfigs, Constants.SharePoint.STS_URL, Constants.SharePoint.GROUP_NAME);
                    string relyingPartyId = Helper.GetConfigKeyValue(sharePointConfigs, Constants.SharePoint.RELYING_PARTY_ID, Constants.SharePoint.GROUP_NAME);
                    tracingService.Trace("got the username and password");
                 
                    var applicationDocumentLocation = Helper.RetrieveEntitySharePointLocation(service, AppApplication.ENTITY_NAME.ToLowerInvariant());
                    RetrieveAbsoluteAndSiteCollectionUrlRequest req1 = new RetrieveAbsoluteAndSiteCollectionUrlRequest
                    {
                        Target = applicationDocumentLocation
                    };
                    
                    var applicationSiteCollection = (RetrieveAbsoluteAndSiteCollectionUrlResponse)service.Execute(req1);
                    string relativeUrl = string.Empty;
                    var sp = new SPService(stsUrl, applicationSiteCollection.SiteCollectionUrl, relyingPartyId, userName, password);
                    tracingService.Trace("sp got created");

                        Entity appApplication = Helper.RetrieveApplicationRecord(service, new Guid(appApplicationId));
                        tracingService.Trace("application retrvd");
                        var name = appApplication["dfa_name"].ToString();
                        //2024/01/22 check special character in name
                        name = name.Replace('/', '-');                     
                         relativeUrl = name + "_" + string.Format(appApplication.Id.ToString()).Replace("-", "");
                        tracingService.Trace("uploading");
                        var output = UploadDocumentToSharePoint(service, applicationAttachment, applicationDocumentLocation, relativeUrl, AppApplication.ENTITY_NAME, appApplicationId, appApplication, sp,tracingService,description, modifiedby, fileType, requireddocumenttype);
                        context.OutputParameters["output"] = output;
                }
            }

            catch (InvalidPluginExecutionException ex)
            {
                message = string.Format(Strings.INVALID_PLUGIN_MESSAGE, ex.Message + "\n" + ex.StackTrace);
            }
            catch (FaultException ex)
            {
                message = string.Format(Strings.FAULT_EXCEPTION_MESSAGE, ex.Message);
            }
            catch (Exception ex)
            {
                message = ex.Message + "\n" + ex.StackTrace;
            }
            finally
            {
                context.OutputParameters[SupplierAction.OutParams.SUBMISSION_FLAG] = submissionFlag;
                if (!string.IsNullOrEmpty(message))
                    context.OutputParameters[SupplierAction.OutParams.MESSAGE] = message;
            }

        }

        private static string UploadDocumentToSharePoint(IOrganizationService service, EntityCollection attachmentCollection, EntityReference entityDocumentLocation,
            string relativeUrl, string entityName, string reference, Entity parent, SPService sp, ITracingService tracing,string desc,string modifiedby,string fileType,string dfa_requireddocumenttype)
        {

            string appDocument = string.Empty;
            tracing.Trace("inside upload function");
            if (sp.CreateFolder(string.Format("/{0}/{1}", entityName.ToLowerInvariant(), relativeUrl)).GetAwaiter().GetResult())
            {
                tracing.Trace("folder is getting created. ");
                //create a document location path in Dynamics
                Entity location = new Entity("sharepointdocumentlocation");
                location["name"] = "Documents on " + relativeUrl;
                location["regardingobjectid"] = new EntityReference(entityName, parent.Id);
                location["relativeurl"] = relativeUrl;
                location["locationtype"] = new OptionSetValue(0); //General
                location["servicetype"] = new OptionSetValue(0); //SharePoint
                location["parentsiteorlocation"] = entityDocumentLocation;

                var documentLocationId = service.Create(location);
                tracing.Trace("documentlocationid :. " + documentLocationId);
                RetrieveAbsoluteAndSiteCollectionUrlRequest req2 = new RetrieveAbsoluteAndSiteCollectionUrlRequest();
                req2.Target = new EntityReference("sharepointdocumentlocation", documentLocationId);
                var resp2 = (RetrieveAbsoluteAndSiteCollectionUrlResponse)service.Execute(req2);
                foreach (Entity attachment in attachmentCollection.Entities)
                {
                    tracing.Trace("attachment :. " + attachment.GetAttributeValue<string>("body"));
                    tracing.Trace("attachment filename is : " + attachment.GetAttributeValue<string>("filename"));
                    {
                        using (var fileStream = new System.IO.MemoryStream(Convert.FromBase64String(attachment.Attributes[Attachment.Schema.BODY].ToString())))
                        {
                            tracing.Trace("Going to upload now with abs URL: " + resp2.AbsoluteUrl);
                            var fileUrl = sp.UploadFile(resp2.AbsoluteUrl, attachment[Attachment.Schema.FILE_NAME].ToString(), fileStream,tracing);
                            appDocument = createAppDocumentLocation(service,attachment,parent.Id.ToString(),tracing,desc,modifiedby,fileType, fileUrl, dfa_requireddocumenttype);
                            tracing.Trace("fileURL is: " + fileUrl);
                        }
                    }

                }

            }
            return appDocument;

        }
        private static string createAppDocumentLocation(IOrganizationService service, Entity entity,string appApplicationId,ITracingService tracing,string desc,string modifiedBy,string fileType,string fileurl, string dfa_requireddocumenttype)
        {
        

            var appLocationDocument = new Entity()
            {
                LogicalName = "dfa_appdocumentlocations"
            };
            appLocationDocument["dfa_name"] = entity["filename"];
            appLocationDocument["dfa_modifiedby"] = modifiedBy;
            appLocationDocument["dfa_documenttype"] = fileType;
            appLocationDocument["dfa_description"] = desc;
            appLocationDocument["dfa_documentlinkonsharepoint"] = fileurl;
            appLocationDocument["dfa_requireddocumenttype "] = dfa_requireddocumenttype;
            appLocationDocument["dfa_applicationid"] = new EntityReference("dfa_appapplication",new Guid(appApplicationId));
            tracing.Trace("Creating appLocationDocumentation");
            var id = service.Create(appLocationDocument);
            tracing.Trace("app Location Documentation has been created with : " + id);
            return id.ToString();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        private static void ValidateAttachment(Entity item)
        {
            if (!item.Attributes.Contains(Attachment.Schema.ACTIVITY_SUBJECT))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Canceled, string.Format("The {0} key doesn't exist in the activitymimeattachment JSON Object.",
                    Attachment.Schema.ACTIVITY_SUBJECT));
            }

            if (item.Attributes[Attachment.Schema.ACTIVITY_SUBJECT] == null || string.IsNullOrEmpty(item.Attributes[Attachment.Schema.ACTIVITY_SUBJECT].ToString()))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Canceled, string.Format("The {0} key of the activitymimeattachment object is null or empty.",
                    Attachment.Schema.ACTIVITY_SUBJECT));
            }

            if (!item.Attributes.Contains(Attachment.Schema.SUBJECT))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Canceled, string.Format("The {0} key doesn't exist in the activitymimeattachment JSON Object.",
                    Attachment.Schema.SUBJECT));
            }

            if (item.Attributes[Attachment.Schema.SUBJECT] == null || string.IsNullOrEmpty(item.Attributes[Attachment.Schema.SUBJECT].ToString()))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Canceled, string.Format("The {0} key of the activitymimeattachment object is null or empty.",
                    Attachment.Schema.SUBJECT));
            }


            if (!item.Attributes.Contains(Attachment.Schema.BODY))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Canceled, string.Format("The {0} key doesn't exist in the activitymimeattachment JSON Object.",
                    Attachment.Schema.BODY));
            }

            if (item.Attributes[Attachment.Schema.BODY] == null || string.IsNullOrEmpty(item.Attributes[Attachment.Schema.BODY].ToString()))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Canceled, string.Format("The {0} key of the activitymimeattachment object is null or empty.",
                    Attachment.Schema.BODY));
            }

            if (!item.Attributes.Contains(Attachment.Schema.FILE_NAME))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Canceled, string.Format("The {0} key doesn't exist in the activitymimeattachment JSON Object.",
                    Attachment.Schema.FILE_NAME));
            }

            if (item.Attributes[Attachment.Schema.FILE_NAME] == null || string.IsNullOrEmpty(item.Attributes[Attachment.Schema.FILE_NAME].ToString()))
            {
                throw new InvalidPluginExecutionException(OperationStatus.Canceled, string.Format("The {0} key of the activitymimeattachment object is null or empty.",
                    Attachment.Schema.FILE_NAME));
            }
        }



        /// <summary>
        /// Clones the Invoice Entity from the Singleton Invoice Record passed in the invoiceCollection input parameter
        /// </summary>
        /// <param name="invoiceCopier">Cloned invoice object</param>
        /// <returns></returns>
        private Entity CloneInvoice(Entity invoiceCopier)
        {
            Entity clone = new Entity(Invoice.ENTITY_NAME);

            clone[Invoice.Schema.INVOICE_REF] = invoiceCopier[Invoice.Schema.INVOICE_REF];
            clone[Invoice.Schema.SUBMISSION_REFERENCE_NUMBER] = invoiceCopier[Invoice.Schema.SUBMISSION_REFERENCE_NUMBER];
            clone[Invoice.Schema.REMIT_PAYMENT_TO_OTHER_BUSINESS] = invoiceCopier[Invoice.Schema.REMIT_PAYMENT_TO_OTHER_BUSINESS];
            clone[Invoice.Schema.SUBMITTED_TOTAL_GST] = invoiceCopier[Invoice.Schema.SUBMITTED_TOTAL_GST];
            clone[Invoice.Schema.SUBMITTED_TOTAL_INVOICE_AMOUNT] = invoiceCopier[Invoice.Schema.SUBMITTED_TOTAL_INVOICE_AMOUNT];
            clone[Invoice.Schema.INVOICE_TYPE] = invoiceCopier[Invoice.Schema.INVOICE_TYPE];
            clone[Invoice.Schema.SUPPLIER_NAME] = invoiceCopier[Invoice.Schema.SUPPLIER_NAME];
            clone[Invoice.Schema.SUPPLIER_LEGAL_NAME] = invoiceCopier[Invoice.Schema.SUPPLIER_LEGAL_NAME];
            clone[Invoice.Schema.STORE_NUMBER] = invoiceCopier[Invoice.Schema.STORE_NUMBER];
            clone[Invoice.Schema.GST_NUMBER] = invoiceCopier[Invoice.Schema.GST_NUMBER];
            clone[Invoice.Schema.ADDRESS_LINE1] = invoiceCopier[Invoice.Schema.ADDRESS_LINE1];
            clone[Invoice.Schema.ADDRESS_LINE2] = invoiceCopier[Invoice.Schema.ADDRESS_LINE2];
            clone[Invoice.Schema.City] = invoiceCopier[Invoice.Schema.City];
            clone[Invoice.Schema.POSTAL_CODE] = invoiceCopier[Invoice.Schema.POSTAL_CODE];
            clone[Invoice.Schema.PROVINCE] = invoiceCopier[Invoice.Schema.PROVINCE];
            clone[Invoice.Schema.COUNTRY] = invoiceCopier[Invoice.Schema.COUNTRY];
            clone[Invoice.Schema.LEGAL_BUSINESS_NAME] = invoiceCopier[Invoice.Schema.LEGAL_BUSINESS_NAME];
            clone[Invoice.Schema.REMIT_COUNTRY] = invoiceCopier[Invoice.Schema.REMIT_COUNTRY];
            clone[Invoice.Schema.REMIT_CITY] = invoiceCopier[Invoice.Schema.REMIT_CITY];
            clone[Invoice.Schema.RELATED_JURISDICTION] = invoiceCopier[Invoice.Schema.RELATED_JURISDICTION];
            clone[Invoice.Schema.REMIT_ADDRESS1] = invoiceCopier[Invoice.Schema.REMIT_ADDRESS1];
            clone[Invoice.Schema.REMIT_ADDRESS2] = invoiceCopier[Invoice.Schema.REMIT_ADDRESS2];
            clone[Invoice.Schema.REMIT_PROVINCE_STATE] = invoiceCopier[Invoice.Schema.REMIT_PROVINCE_STATE];
            clone[Invoice.Schema.REMIT_POSTAL_CODE] = invoiceCopier[Invoice.Schema.REMIT_POSTAL_CODE];
            clone[Invoice.Schema.CONTACT_FIRST_NAME] = invoiceCopier[Invoice.Schema.CONTACT_FIRST_NAME];
            clone[Invoice.Schema.CONTACT_LAST_NAME] = invoiceCopier[Invoice.Schema.CONTACT_LAST_NAME];
            clone[Invoice.Schema.CONTACT_EMAIL] = invoiceCopier[Invoice.Schema.CONTACT_EMAIL];
            clone[Invoice.Schema.CONTACT_NUMBER] = invoiceCopier[Invoice.Schema.CONTACT_NUMBER];
            clone[Invoice.Schema.CONTACT_FAX] = invoiceCopier[Invoice.Schema.CONTACT_FAX];


            return clone; //return the entity object

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tracingService"></param>
        /// <param name="context"></param>
        private static void ValidateInputParameters(ITracingService tracingService, IPluginExecutionContext context)
        {

            //TODO: turning it off just for portal development
            //if (!context.InputParameters.Contains(SupplierAction.InputParams.DOCUMENT_COLLECTION))
            //{
            //    tracingService.Trace("Input Parameter \"documentCollection\" is not found");
            //    throw new InvalidPluginExecutionException(OperationStatus.Failed, "Input Parameter \"documentCollection\" is not found");
            //}

            //if (context.InputParameters[SupplierAction.InputParams.DOCUMENT_COLLECTION] == null)
            //{
            //    tracingService.Trace("Input Parameter \"documentCollection\" is null or empty");
            //    throw new InvalidPluginExecutionException(OperationStatus.Failed, "Input Parameter \"documentCollection\" is null or empty");
        

           

        }

    }//end of class
}

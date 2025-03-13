using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace DFA.Portal.Custom.Actions
{
    public class DFAPortalAnnotationCreation : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService orgService = serviceFactory.CreateOrganizationService(context.UserId);
            var annotations =  context.InputParameters["signature"];
            var appAppId = context.InputParameters["dfa_appapplicationid"];
            var filename = context.InputParameters["filename"];

            tracingService.Trace("the decoding is : " + annotations);
     
            Entity annotation = new Entity("annotation");
            annotation["subject"] = filename;
            annotation["objectid"] = new EntityReference("dfa_appapplication", new Guid(appAppId.ToString()));
            annotation["documentbody"] = annotations;
            annotation["filename"] = filename +".png";

            annotation["mimetype"] = "image/png"; 
             var ann = orgService.Create(annotation);
            tracingService.Trace("Annotations has created" + ann.ToString());
        }
       
    }

    public class JsonHelper
    {
        internal static string JsonSerializer<T>(T t)
        {
            string jsonString = null;
            using (MemoryStream ms = new MemoryStream())
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
                ser.WriteObject(ms, t);
                jsonString = Encoding.UTF8.GetString(ms.ToArray());
            }
            return jsonString;
        }

        internal static T JsonDeserialize<T>(string jsonString)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            T obj = (T)ser.ReadObject(ms);
            return obj;
        }
    }
    public class signature
    {
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
        public string Regarding { get; set; }
        public string id { get; set; }
    }
}
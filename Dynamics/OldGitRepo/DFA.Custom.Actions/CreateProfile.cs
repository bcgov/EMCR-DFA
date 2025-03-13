using Microsoft.Xrm.Sdk;
using System;

namespace DFA.Custom.Actions
{
    public class CreateProfile : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService orgService = serviceFactory.CreateOrganizationService(context.UserId);

            try
            {
                // Retrieve input parameters from the plugin execution context
                string firstname = context.InputParameters["dfa_firstname"] as string;
                string lastname = context.InputParameters["dfa_lastname"] as string;

                // Perform custom logic using the input parameters
                var appcontact = new Entity("dfa_appcontact");
                appcontact.Attributes["dfa_firstname"] = firstname;
                appcontact.Attributes["dfa_lastname"] = lastname;
                
                var guid = orgService.Create(appcontact); 
                // Set output parameters in the plugin execution context
                context.OutputParameters["output"] = guid;
              
            }
            catch (Exception ex)
            {
                // Handle any exceptions and log error details
                tracingService.Trace($"Error executing CreateProfile plugin: {ex.Message}");
                throw new InvalidPluginExecutionException("An error occurred in the CreateProfile plugin.", ex);
            }
        }
    }
}
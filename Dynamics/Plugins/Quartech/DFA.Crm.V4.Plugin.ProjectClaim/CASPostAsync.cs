using DFA.Crm.V4.Core.CAS.Process;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFA.Crm.V4.Plugin.ProjectClaim
{
    public class CASPostAsync : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);


            if (context.PrimaryEntityName != "dfa_projectclaim" || context.Stage != 40)
                return;
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity projectClaim = (Entity)context.InputParameters["Target"];
                if (projectClaim.LogicalName != "dfa_projectclaim")
                    return;
                if (projectClaim.Attributes.Contains("dfa_projectclaimid"))
                {
                    Guid projectClaimId = projectClaim.GetAttributeValue<Guid>("dfa_projectclaimid");
                    tracingService.Trace("Project Claim ID: " + projectClaimId);

                    var casService = new CASIntegration(serviceProvider);
                    casService.GenerateInvoice(projectClaimId);
                }
            }
                    
        }
    }
}

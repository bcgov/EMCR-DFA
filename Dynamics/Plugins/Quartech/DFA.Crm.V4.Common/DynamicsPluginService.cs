using System;
using System.Activities;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace DFA.Crm.V4.Common
{
    public class DynamicsPluginService : IDynamicsService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IPluginExecutionContext context;

        public DynamicsPluginService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
        }

        public ITracingService TracingService
        {
            get => (ITracingService)this.serviceProvider.GetService(typeof(ITracingService));
        }
        public IOrganizationService UserService
        {
            get
            {
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                return serviceFactory.CreateOrganizationService(this.context.UserId);
            }
        }
        public IOrganizationService SystemService
        {
            get
            {
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                return serviceFactory.CreateOrganizationService(Guid.Empty);
            }
        }

        public Entity Target
        {
            get
            {
                return this.context.InputParameters.Contains("Target") && this.context.InputParameters["Target"] is Entity ? (Entity)context.InputParameters["Target"] : null;
            }
        }

        public Entity PreTargetImage
        {
            get
            {
                return context.PreEntityImages.Contains("Target") ? (Entity)context.PreEntityImages["Target"] : null;
            }
        }

        public Entity PostTargetImage
        {
            get
            {
                return context.PostEntityImages.Contains("Target") ? (Entity)context.PostEntityImages["Target"] : null;
            }
        }

        public IPluginExecutionContext PluginExecutionContext
        {
            get
            {
                return (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            }
        }

        public string MessageName
        {
            get
            {
                return this.context.MessageName;
            }
        }

        public string PrimaryEntityName
        {
            get
            {
                return this.context.PrimaryEntityName;
            }
        }

        public Guid PrimaryEntityId
        {
            get
            {
                return this.context.PrimaryEntityId;
            }
        }
    }
}
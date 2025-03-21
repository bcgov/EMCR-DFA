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
    public class DynamicsWorkflowService : IDynamicsService
    {
        private readonly CodeActivityContext executionContext;
        private readonly IWorkflowContext workflowContext;

        public DynamicsWorkflowService(CodeActivityContext executionContext)
        {
            this.executionContext = executionContext;
            this.workflowContext = executionContext.GetExtension<IWorkflowContext>();
        }

        public ITracingService TracingService
        {
            get => this.executionContext.GetExtension<ITracingService>();
        }
        public IOrganizationService UserService
        {
            get
            {
                IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
                return serviceFactory.CreateOrganizationService(workflowContext.UserId); ;
            }
        }
        public IOrganizationService SystemService {
            get
            {
                IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
                return serviceFactory.CreateOrganizationService(null); ;
            }
        }

        public Entity Target => throw new NotImplementedException();

        public Entity PreTargetImage => throw new NotImplementedException();

        public Entity PostTargetImage => throw new NotImplementedException();

        public IPluginExecutionContext PluginExecutionContext => throw new NotImplementedException();

        public string MessageName => throw new NotImplementedException();

        public string PrimaryEntityName => throw new NotImplementedException();

        public Guid PrimaryEntityId => throw new NotImplementedException();
    }
}
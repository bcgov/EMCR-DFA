using System;
using Microsoft.Xrm.Sdk;

namespace DFA.Crm.V4.Common
{
    public interface IDynamicsService
    {
        ITracingService TracingService { get; }
        IOrganizationService UserService { get;}
        IOrganizationService SystemService { get;}

        Entity Target { get;}
        Entity PreTargetImage { get; }
        Entity PostTargetImage { get; }

        IPluginExecutionContext PluginExecutionContext { get; }

        string MessageName { get; }

        string PrimaryEntityName { get; }

        Guid PrimaryEntityId { get; }
    }
}

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DFA.CRM.Plugins
{
    /// <summary>
    /// For cares_AddApprovalItems Custom Action
    /// </summary>
    public class ValidateUserHasSecurityRoleAction : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            //Extract the tracing service for use in debugging sandboxed plug-ins.
            var Tracing =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            // Obtain the execution context from the service provider.
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            Tracing.Trace("context");
            // Obtain the organization service reference.
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            Tracing.Trace("servicefactory");
            var Service = serviceFactory.CreateOrganizationService(context.UserId);
            Tracing.Trace("contextuser" + context.UserId.ToString());
            Tracing.Trace("service" + Service.ToString());
            try
            {
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Obtain Input Parameters");
                var securityRoleName = (string)context.InputParameters["RoleName"];
                var userIDString = (string)context.InputParameters["UserID"];
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Role: {securityRoleName} - User ID: {userIDString}");
                Guid userID = Guid.Empty;
                if (!Guid.TryParse(userIDString, out userID))
                {
                    context.OutputParameters["HasRole"] = false;
                    return;
                }

                var userER = new EntityReference("systemuser", userID);


                // obtain all the roleIds of the specified security role by name
                Tracing.Trace("Get all applicable RoleIds of various business units for match");
                // securityRoleEntityReference will not have a name sometimes
                var roleIds = CustomCodeHelper.GetRoleIdsByRoleName(Service, Tracing, securityRoleName);

                Tracing.Trace("Roles to Match");
                foreach (var roleId in roleIds)
                {
                    Tracing.Trace("Role: " + roleId);
                }

                Tracing.Trace("Run the is user in role method.");
                string debugString;
                var userHasRole = CustomCodeHelper.IsUserInRole(Service, Tracing, userER.Id, roleIds, out debugString);
                Tracing.Trace("Set output");
                context.OutputParameters["HasRole"] = userHasRole;
            }
            catch (Exception ex)
            {
                var error = "{\"error\":\"" + ex.Message + "|" + ex.Source + "|" + ex.StackTrace + "\"}";
                context.OutputParameters["Message"] = error;
            }

        }
    }
}
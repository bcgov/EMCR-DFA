using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;

namespace DFA.CRM.CustomWorkflow
{
    /// <summary>
    /// Validated if the provided user has certain security role by name
    /// </summary>
    public class VerifyUserSecurityRoleActivity : CodeActivity
    {
        [Input("Security Role Name")]
        [RequiredArgument]
        public InArgument<string> SecurityRoleInArgument { get; set; }

        [Input("User")]
        [RequiredArgument]
        [ReferenceTarget("systemuser")]
        public InArgument<EntityReference> UserInArgument { get; set; }

        [Output("User has specified Security Role")]
        public OutArgument<bool> HasSecurityRoleOutArgument { get; set; }

        CodeActivityContext Activity;
        ITracingService Tracing;
        IWorkflowContext Workflow;
        IOrganizationServiceFactory ServiceFactory;
        IOrganizationService Service;

        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="activityContext"></param>
        protected override void Execute(CodeActivityContext activityContext)
        {
            try
            {
                Activity = activityContext;
                Tracing = Activity.GetExtension<ITracingService>();
                Workflow = Activity.GetExtension<IWorkflowContext>();
                ServiceFactory = Activity.GetExtension<IOrganizationServiceFactory>();
                Service = ServiceFactory.CreateOrganizationService(Workflow.UserId);

                Tracing.Trace(
                "Entered WorkflowActivityTest.Execute(), Activity Instance Id: {0}, Workflow Instance Id: {1}",
                activityContext.ActivityInstanceId,
                activityContext.WorkflowInstanceId);

                Tracing.Trace("WorkflowActivityTest.Execute(), Correlation Id: {0}, Initiating User: {1}",
                Workflow.CorrelationId,
                Workflow.InitiatingUserId);

                var securityRoleName = SecurityRoleInArgument.Get(activityContext);
                var userER = UserInArgument.Get(activityContext);

                Tracing.Trace("Specified Role: " + securityRoleName);

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
                HasSecurityRoleOutArgument.Set(activityContext, userHasRole);

            }
            catch (InvalidPluginExecutionException) { throw; }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException($"Unexpected error. {ex.Message}", ex);
            }
        }
    }
}
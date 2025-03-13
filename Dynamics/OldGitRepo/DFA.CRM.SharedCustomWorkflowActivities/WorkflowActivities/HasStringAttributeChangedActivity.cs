using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Linq;

namespace DFA.CRM.CustomWorkflow
{
    public class HasStringAttributeChangedActivity : CodeActivity
    {
        [RequiredArgument]
        [Input("String Field Schema Name")]
        public InArgument<string> CompareSchemaNameInput { get; set; }

        [Input("Ignore Change to NULL")]
        public InArgument<bool> IgnoreChangeToNULLInput { get; set; }

        [Output("Has Changes")]
        public OutArgument<bool> HasChangesOutput { get; set; }

        CodeActivityContext Activity;
        ITracingService Tracing;
        IWorkflowContext Workflow;
        IOrganizationServiceFactory ServiceFactory;
        IOrganizationService Service;

        /// <summary>
        /// Current snapshot of Licence Application (adoxio_application).
        /// </summary>
        Entity Target;
        /// <summary>
        /// Previous snapshot of Licence Application (adoxio_application).
        /// </summary>
        Entity TargetPreImage;

        /// <summary>
        /// Previous value of the specified string attribute.
        /// </summary>
        string OldString;
        /// <summary>
        /// Current value of the specified string attribute.
        /// </summary>
        string NewString;

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

                Initialize();

                var schemaName = CompareSchemaNameInput.Get(activityContext);
                var ignoreChangeToNULL = IgnoreChangeToNULLInput.Get(activityContext);

                Tracing.Trace($"Compare Schema Name: {schemaName}");

                OldString = TargetPreImage?.GetAttributeValue<string>(schemaName);
                NewString = Target.GetAttributeValue<string>(schemaName);

                Tracing.Trace($"Old String = ||{OldString}||");
                Tracing.Trace($"New String = ||{NewString}||");

                var oldStringHasContents = !string.IsNullOrEmpty(OldString);
                var newStringHasContents = !string.IsNullOrEmpty(NewString);
                var changed = OldString?.Equals(NewString) == false || NewString?.Equals(OldString) == false;
                var ignoredNULLChange = changed;
                if (!newStringHasContents && ignoreChangeToNULL)
                {
                    ignoredNULLChange = false;
                }

                var reportAsChanged = changed && (ignoredNULLChange);

                Tracing.Trace($"Value changed: {changed}");
                Tracing.Trace($"Reporting Value as changed: {reportAsChanged}");

                HasChangesOutput.Set(activityContext, reportAsChanged);

            }
            catch (InvalidPluginExecutionException) { throw; }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException($"Unexpected error. {ex.Message}", ex);
            }
        }

        protected void Initialize()
        {
            Tracing.Trace($"PrimaryEntityName = {Workflow.PrimaryEntityName}");//Add Licence Application Sub Steps
            Tracing.Trace($"PrimaryEntityId = {Workflow.PrimaryEntityId}");//Add Licence Application Sub Steps
            Tracing.Trace($"MessageName = {Workflow.MessageName}");//Add Licence Application Sub Steps

            switch (Workflow.MessageName)
            {
                case "Create":
                    break;
                case "Update":
                    TargetPreImage = Workflow.PreEntityImages?.Values.FirstOrDefault(x => x.LogicalName == Workflow.PrimaryEntityName && x.Id == Workflow.PrimaryEntityId);
                    Tracing.Trace($"PreEntityImage => {TargetPreImage?.LogicalName} {TargetPreImage?.Id}");
                    break;
                case "ExecuteWorkflow":
                    // On Demand;
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: On Demand Workflow, Obtain the Target entity since the target may not have all attributes");
                    Target = Service.Retrieve(Workflow.PrimaryEntityName, Workflow.PrimaryEntityId, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                    break;
                default:
                    throw new InvalidOperationException($"This activity does not support the {Workflow.MessageName} message.");
            }

            if (Target == null && Workflow.InputParameters.Contains("Target"))
            {
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Get Target if Create/Update");
                Target = (Entity)Workflow.InputParameters["Target"];
            }
            Tracing.Trace($"Target.LogicalName = {Target.LogicalName}");
            Tracing.Trace($"Target.Id = {Target.Id}");
        }
    }
}
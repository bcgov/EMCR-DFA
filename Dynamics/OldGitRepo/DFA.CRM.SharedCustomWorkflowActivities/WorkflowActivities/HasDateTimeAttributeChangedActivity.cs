using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Linq;

namespace DFA.CRM.CustomWorkflow
{
    public class HasDateTimeAttributeChangedActivity : CodeActivity
    {
        [RequiredArgument]
        [Input("DateTime Field Schema Name")]
        public InArgument<string> CompareSchemaNameInput { get; set; }

        [Output("Has Changes")]
        public OutArgument<bool> HasChangesOutput { get; set; }

        CodeActivityContext Activity;
        ITracingService Tracing;
        IWorkflowContext Workflow;
        IOrganizationServiceFactory ServiceFactory;
        IOrganizationService Service;

        /// <summary>
        /// Current snapshot of changing Entity
        /// </summary>
        Entity Target;
        /// <summary>
        /// Previous snapshot of changing Entity
        /// </summary>
        Entity TargetPreImage;

        /// <summary>
        /// Previous value of the specified bool attribute.
        /// </summary>
        DateTime? OldValue;
        /// <summary>
        /// Current value of the specified bool attribute.
        /// </summary>
        DateTime? NewValue;

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
                bool changed = true;
                Tracing.Trace($"Compare Schema Name: {schemaName}");

                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Get Pre Image Value if any");
                OldValue = TargetPreImage?.GetAttributeValue<Nullable<DateTime>>(schemaName);
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Get Target Image Value");
                NewValue = Target.GetAttributeValue<Nullable<DateTime>>(schemaName);

                var targetHasValueChanged = Target.Attributes.Contains(schemaName);
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Target Has attribute in it: {targetHasValueChanged}");
                var bothValuesAreNULL = (OldValue == null && NewValue == null);
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Old and New attributes are NULL: {bothValuesAreNULL}");
                var valuesAreEqual = (OldValue != null && NewValue != null && OldValue.Value == NewValue.Value);
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Both Values are same: {valuesAreEqual}");
                changed = targetHasValueChanged && !(bothValuesAreNULL || valuesAreEqual);

                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Value changed: {changed}");


                HasChangesOutput.Set(activityContext, changed);

            }
            catch (InvalidPluginExecutionException) { throw; }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException($"{CustomCodeHelper.LineNumber()}: Unexpected error. {ex.Message}", ex);
            }
        }

        protected void Initialize()
        {
            Tracing.Trace($"{CustomCodeHelper.LineNumber()}: PrimaryEntityName = {Workflow.PrimaryEntityName}");//Add Licence Application Sub Steps
            Tracing.Trace($"{CustomCodeHelper.LineNumber()}: PrimaryEntityId = {Workflow.PrimaryEntityId}");//Add Licence Application Sub Steps
            Tracing.Trace($"{CustomCodeHelper.LineNumber()}: MessageName = {Workflow.MessageName}");//Add Licence Application Sub Steps

            switch (Workflow.MessageName)
            {
                case "Create":
                    break;
                case "Update":
                    TargetPreImage = Workflow.PreEntityImages?.Values.FirstOrDefault(x => x.LogicalName == Workflow.PrimaryEntityName && x.Id == Workflow.PrimaryEntityId);
                    if (TargetPreImage != null)
                    {
                        Tracing.Trace($"{CustomCodeHelper.LineNumber()}: PreEntityImage => {TargetPreImage?.LogicalName} {TargetPreImage?.Id}");
                    }
                    else
                    {
                        Tracing.Trace($"{CustomCodeHelper.LineNumber()}: PreEntityImage NOT Found");
                    }
                    break;
                case "ExecuteWorkflow":
                    // On Demand;
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: On Demand Workflow, Obtain the Target entity since the target may not have all attributes");
                    Target = Service.Retrieve(Workflow.PrimaryEntityName, Workflow.PrimaryEntityId, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                    break;
                default:
                    throw new InvalidOperationException($"{CustomCodeHelper.LineNumber()}: This activity does not support the {Workflow.MessageName} message.");
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
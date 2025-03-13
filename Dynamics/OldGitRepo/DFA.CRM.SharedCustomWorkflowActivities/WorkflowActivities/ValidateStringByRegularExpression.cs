using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;

namespace DFA.CRM.CustomWorkflow
{
    public class ValidateStringByRegularExpression : CodeActivity
    {
        [RequiredArgument]
        [Input("String to Evaluate")]
        public InArgument<string> StringInput { get; set; }

        [RequiredArgument]
        [Input("Regular Expression")]
        public InArgument<string> RegularExpressionInput { get; set; }

        [Output("Is Valid")]
        public OutArgument<bool> IsValidOutput { get; set; }

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
            Activity = activityContext;
            Tracing = Activity.GetExtension<ITracingService>();
            Workflow = Activity.GetExtension<IWorkflowContext>();
            ServiceFactory = Activity.GetExtension<IOrganizationServiceFactory>();
            Service = ServiceFactory.CreateOrganizationService(Workflow.UserId);

            var stringToEvaluate = StringInput.Get(activityContext);
            var regularExpression = RegularExpressionInput.Get(activityContext);

            // Treat for NULL value
            if (stringToEvaluate == null)
            {
                stringToEvaluate = string.Empty;
            }

            // Evaluate
            try
            {
                var rgx = new System.Text.RegularExpressions.Regex(regularExpression);
                var isValid = rgx.IsMatch(stringToEvaluate);
                IsValidOutput.Set(activityContext, isValid);
            }
            catch
            {
                IsValidOutput.Set(activityContext, false);
            }
        }
    }
}

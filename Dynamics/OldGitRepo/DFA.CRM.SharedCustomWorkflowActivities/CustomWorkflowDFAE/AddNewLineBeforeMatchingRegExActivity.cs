using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;
using System.Text.RegularExpressions;

namespace DFA.CRM.CustomWorkflow
{
    public class AddNewLineBeforeMatchingRegExActivity : CodeActivity
    {
        [RequiredArgument]
        [Input("String to Process")]
        public InArgument<string> StringInput { get; set; }

        [RequiredArgument]
        [Input("Regular Expression")]
        public InArgument<string> RegularExpressionInput { get; set; }

        [Output("ProcessedString")]
        public OutArgument<string> ResultStringOutput { get; set; }

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
            var resultString = string.Empty;
            // Treat for NULL value
            if (stringToEvaluate == null)
            {
                stringToEvaluate = string.Empty;
            }

            // Evaluate
            try
            {
                // var historicalComments = "Oct 16/20 - blah, blah, blahOct 31/20 - that, this those";
                var regExp = @"((Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)[ ]?\d{1,2}\/\d{2})";
                if (string.IsNullOrEmpty(regularExpression))
                {
                    regularExpression = regExp;
                }

                var result = Regex.Replace(stringToEvaluate, regularExpression, "\n$1");
                // Remove First Empty Line if it were started with blank line.
                if (result.Substring(0, 1) == "\n")
                {
                    result = result.Substring(1);
                }
                resultString = result;
            }
            catch
            {
                ResultStringOutput.Set(activityContext, resultString);
            }

            ResultStringOutput.Set(activityContext, resultString);
        }
    }
}

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;

namespace DFA.CRM.CustomWorkflow
{
    public class MathHelper : CodeActivity
    {
        [RequiredArgument]
        [Input("Operand A")]
        public InArgument<decimal> OperandAInput { get; set; }

        [RequiredArgument]
        [Input("Operator - Add, Subtract, Multiply, Divide By or Remainder")]
        public InArgument<string> OperatorInput { get; set; }

        [RequiredArgument]
        [Input("Operand B")]
        public InArgument<decimal> OperandBInput { get; set; }

        [Output("Calculated Results")]
        public OutArgument<decimal> ResultOuput { get; set; }

        CodeActivityContext Activity;
        ITracingService Tracing;

        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="activityContext"></param>
        protected override void Execute(CodeActivityContext activityContext)
        {
            Activity = activityContext;
            Tracing = Activity.GetExtension<ITracingService>();

            var operandA = OperandAInput.Get(activityContext);
            var operandB = OperandBInput.Get(activityContext);
            var operatorString = OperatorInput.Get(activityContext).ToUpper();
            var results = 0m;

            switch (operatorString)
            {
                case "ADD":
                    results = operandA + operandB;
                    break;
                case "SUBTRACT":
                    results = operandA - operandB;
                    break;
                case "MULTIPLY":
                case "MULTIPLE":
                    results = operandA * operandB;
                    break;
                case "DIVIDE BY":
                    if (operandB != 0)
                    {
                        results = operandA / operandB;
                    }
                    else
                    {
                        Tracing.Trace("Divide By 0");
                    }
                    break;
                case "REMAINDER":
                    if (operandB != 0)
                    {
                        results = operandA % operandB;
                    }
                    else
                    {
                        Tracing.Trace("Divide By 0");
                    }
                    break;
                default:
                    Tracing.Trace("Undefined Operand");
                    break;
            }
            Tracing.Trace($"{operandA} {operatorString} {operandB} Results: {results}");
            ResultOuput.Set(activityContext, results);

        }
    }
}

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFA.CRM.CustomWorkflow
{
    public class DeleteThisRecord : CodeActivity
    {
        CodeActivityContext Activity;
        ITracingService Tracing;
        IWorkflowContext Workflow;
        IOrganizationServiceFactory ServiceFactory;
        IOrganizationService Service;

        protected override void Execute(CodeActivityContext activityContext)
        {
            try
            {
                Activity = activityContext;
                Tracing = Activity.GetExtension<ITracingService>();
                Workflow = Activity.GetExtension<IWorkflowContext>();
                ServiceFactory = Activity.GetExtension<IOrganizationServiceFactory>();
                Service = ServiceFactory.CreateOrganizationService(Workflow.UserId);

                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Deleting {Workflow.PrimaryEntityName} record ID: {Workflow.PrimaryEntityId}");
                // ensure the record still exists
                var recordToDelete = Service.Retrieve(Workflow.PrimaryEntityName, Workflow.PrimaryEntityId, new ColumnSet(false));
                if (recordToDelete != null)
                {
                    Service.Delete(recordToDelete.LogicalName, recordToDelete.Id);
                }
            }
            catch
            {
                // Do Nothing.  this will only die when other process has already removed the record.
            }
        }
    }
}

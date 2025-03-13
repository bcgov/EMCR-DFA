using DFA.Portal.Custom.Actions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Xml.Linq;

namespace DFA.Portal.CustomActivitiesDFAE
{
    public class CreateClaim : CodeActivity
    {

        [Input("dfa_finalclaim")]
        public InArgument<bool> FinalClaim { get; set; }


        [Input("dfa_recoveryplanid")]
        public InArgument<string> RecoveryPlanId { get; set; } //Related Project ID

        [Input("dfa_projectclaimid")]
        public InArgument<string> ProjectClaimId { get; set; } //Recovery Claim Primary Key

        [Input("dfa_claimbpfstages")]
        public InArgument<int> Claimbpfstages { get; set; }

        [Input("dfa_claimbpfsubstages")]
        public InArgument<int> Claimbpfsubstages { get; set; }

        [Input("dfa_claimreceivedbyemcrdate")]
        public InArgument<DateTime> Claimreceivedbyemcrdate { get; set; }
        
        [Input("dfa_createdonportal")]
        public InArgument<bool> Createdonportal { get; set; }

        [Input("delete")]
        public InArgument<bool> Delete { get; set; }

        [Output("output")]
        public OutArgument<string> ClaimGUID { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            CodeActivityContext Activity;
            ITracingService Tracing;
            IWorkflowContext Workflow;
            IOrganizationServiceFactory ServiceFactory;
            IOrganizationService Service;

            Activity = context;
            Tracing = Activity.GetExtension<ITracingService>();
            Workflow = Activity.GetExtension<IWorkflowContext>();
            ServiceFactory = Activity.GetExtension<IOrganizationServiceFactory>();
            Service = ServiceFactory.CreateOrganizationService(Workflow.UserId);

            var dfa_recoveryplanid = RecoveryPlanId.Get(context);
            var dfa_finalclaim = FinalClaim.Get(context);
            var dfa_projectclaimid = ProjectClaimId.Get(context);

            var dfa_claimbpfstages = Claimbpfstages.Get(context);
            var dfa_claimbpfsubstages = Claimbpfsubstages.Get(context);
            var dfa_claimreceivedbyemcrdate = Claimreceivedbyemcrdate.Get(context);
            var dfa_createdonportal = Createdonportal.Get(context);

            bool delete = Delete.Get(context);
            Money defaultClaimAmount = new Money(0m);

            if (delete)
            {
                Tracing.Trace("deleting the guid id : " + dfa_projectclaimid);
                Service.Delete("dfa_projectclaim", new Guid(dfa_projectclaimid));
                ClaimGUID.Set(context, "deleted");
                Tracing.Trace("deleted the guid id : " + ClaimGUID);
                return;
            }
            if (!string.IsNullOrEmpty(dfa_projectclaimid))
            {
                Tracing.Trace("retrieving the claim with ID: " + dfa_projectclaimid);

                Entity claimUpdate = new Entity("dfa_projectclaim");
                Tracing.Trace("in the update block:");
                claimUpdate.Id = new Guid(dfa_projectclaimid);

                if (!string.IsNullOrEmpty(dfa_recoveryplanid))
                {
                    claimUpdate["dfa_recoveryplanid"] = new EntityReference("dfa_project", new Guid(dfa_recoveryplanid));
                }

                if(dfa_finalclaim != null)
                {
                    claimUpdate["dfa_finalclaim"] = dfa_finalclaim;
                }
                claimUpdate["dfa_claimamount"] = defaultClaimAmount;

                if (dfa_claimbpfstages != null)
                {
                    if (dfa_claimbpfstages == 222710000 || dfa_claimbpfstages == 222710001 ||
                        dfa_claimbpfstages == 222710002 || dfa_claimbpfstages == 222710003 ||
                        dfa_claimbpfstages == 222710004)

                        claimUpdate["dfa_claimbpfstages"] = new OptionSetValue(dfa_claimbpfstages);
                }

                if (dfa_claimbpfsubstages != null)
                {
                    if (dfa_claimbpfsubstages == 222710000 || dfa_claimbpfsubstages == 222710001 ||
                        dfa_claimbpfsubstages == 222710002 || dfa_claimbpfsubstages == 222710003 ||
                        dfa_claimbpfsubstages == 222710004 || dfa_claimbpfsubstages == 222710005 ||
                         dfa_claimbpfsubstages == 222710006 || dfa_claimbpfsubstages == 222710007 || dfa_claimbpfsubstages == 222710008)

                        claimUpdate["dfa_claimbpfsubstages"] = new OptionSetValue(dfa_claimbpfsubstages);
                }
                if (dfa_claimreceivedbyemcrdate != new DateTime())
                {
                    claimUpdate["dfa_claimreceivedbyemcrdate"] = dfa_claimreceivedbyemcrdate;
                }
                claimUpdate["dfa_createdonportal"] = true;
                Service.Update(claimUpdate);
                Tracing.Trace("Claim is updated with guid:" + dfa_projectclaimid.ToString());
                ClaimGUID.Set(context, "Updated Claim GUID: " + dfa_projectclaimid.ToString());

            }
            else
            {
                Entity claimNew = new Entity("dfa_projectclaim");

                if (!string.IsNullOrEmpty(dfa_recoveryplanid))
                {
                    claimNew["dfa_recoveryplanid"] = new EntityReference("dfa_project", new Guid(dfa_recoveryplanid));
                }

                if (dfa_finalclaim != null)
                {
                    claimNew["dfa_finalclaim"] = dfa_finalclaim;

                }
                claimNew["dfa_claimamount"] = defaultClaimAmount;

                if (dfa_claimbpfstages != null)
                {
                    if (dfa_claimbpfstages == 222710000 || dfa_claimbpfstages == 222710001 ||
                        dfa_claimbpfstages == 222710002 || dfa_claimbpfstages == 222710003 ||
                        dfa_claimbpfstages == 222710004)

                        claimNew["dfa_claimbpfstages"] = new OptionSetValue(dfa_claimbpfstages);
                }

                if (dfa_claimbpfsubstages != null)
                {
                    if (dfa_claimbpfsubstages == 222710000 || dfa_claimbpfsubstages == 222710001 ||
                        dfa_claimbpfsubstages == 222710002 || dfa_claimbpfsubstages == 222710003 ||
                        dfa_claimbpfsubstages == 222710004 || dfa_claimbpfsubstages == 222710005 ||
                         dfa_claimbpfsubstages == 222710006 || dfa_claimbpfsubstages == 222710007 || dfa_claimbpfsubstages == 222710008)

                        claimNew["dfa_claimbpfsubstages"] = new OptionSetValue(dfa_claimbpfsubstages);
                }
                if (dfa_claimreceivedbyemcrdate != new DateTime())
                {
                    claimNew["dfa_claimreceivedbyemcrdate"] = dfa_claimreceivedbyemcrdate;
                }
                claimNew["dfa_createdonportal"] = true;
                var claimGuid = Service.Create(claimNew);
                Tracing.Trace("Claim created is : " + claimGuid.ToString());
                ClaimGUID.Set(context, claimGuid.ToString());
            }
        }
    }
}

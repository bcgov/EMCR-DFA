using DFA.Portal.Custom.Actions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Xml.Linq;

namespace DFA.Portal.CustomActivitiesDFAE
{
    public class CreateInvoice : CodeActivity
    {

        [Input("dfa_receiveddatesameasinvoicedate")]
        public InArgument<bool> ReceivedDateSameAsInvoiceDate { get; set; }

        [Input("dfa_portionofinvoice")]
        public InArgument<bool> Portionofinvoice { get; set; }

        [Input("dfa_portioninvoicereason")]
        public InArgument<string> Portioninvoicereason { get; set; }


        [Input("dfa_goodsorservicesreceiveddate")]
        public InArgument<DateTime> Goodsorservicesreceiveddate { get; set; }

        [Input("dfa_invoicedate")]
        public InArgument<DateTime> Invoicedate { get; set; }

        [Input("dfa_claim")]
        public InArgument<string> ParentClaimID { get; set; } //Recovery Claim Related Key

        [Input("dfa_recoveryinvoiceid")]
        public InArgument<string> Recoveryinvoiceid { get; set; }  //Recovery Invoice Primary Key

        [Input("dfa_purpose")]
        public InArgument<string> Purpose { get; set; }

        [Input("dfa_name")]
        public InArgument<string> Name { get; set; }

        [Input("dfa_invoicenumber")]
        public InArgument<string> Invoicenumber { get; set; }

        [Input("dfa_netinvoicedbeingclaimed")]
        public InArgument<int> Netinvoicedbeingclaimed { get; set; }

        [Input("dfa_pst")]
        public InArgument<int> PST { get; set; }

        [Input("dfa_grossgst")]
        public InArgument<int> GrossGST { get; set; }


        [Input("dfa_actualinvoicetotal")]
        public InArgument<int> Actualinvoicetotal { get; set; }

        [Input("dfa_netinvoicedbeingclaimed")]
        public InArgument<int> NetInvoiceBeingClaimed { get; set; }


        [Input("dfa_eligiblegst")]
        public InArgument<int> Eligiblegst { get; set; }

        [Input("dfa_case")]
        public InArgument<string> Case { get; set; }

        [Input("delete")]
        public InArgument<bool> Delete { get; set; }

        [Output("output")]
        public OutArgument<string> InvoiceGUID { get; set; }

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


            var claimid = ParentClaimID.Get(context);
            var caseid = Case.Get(context);
            var recoveryinvoiceid = Recoveryinvoiceid.Get(context);
            var receiveddatesameasinvoicedate = ReceivedDateSameAsInvoiceDate.Get(context);
            var portionofinvoice = Portionofinvoice.Get(context);
            var portioninvoicereason = Portioninvoicereason.Get(context);
            var goodsorservicesreceiveddate = Goodsorservicesreceiveddate.Get(context);
            var invoicedate = Invoicedate.Get(context);
            var purpose = Purpose.Get(context);
            var name = Name.Get(context);
            var invoicenumber = Invoicenumber.Get(context);
            var netinvoicedbeingclaimed = Netinvoicedbeingclaimed.Get(context);
            var pst = PST.Get(context);
            var grossgst = GrossGST.Get(context);
            var actualinvoicetotal = Actualinvoicetotal.Get(context);
            var eligiblegst = Eligiblegst.Get(context);
            bool delete = Delete.Get(context);

            if (delete)
            {
                Tracing.Trace("deleting the guid id : " + recoveryinvoiceid);
                Service.Delete("dfa_recoveryinvoice", new Guid(recoveryinvoiceid));
                InvoiceGUID.Set(context, "deleted");
                Tracing.Trace("deleted the guid id : " + InvoiceGUID);
                return;
            }
            if (!string.IsNullOrEmpty(recoveryinvoiceid))
            {
                Tracing.Trace("retrieving the Invoice with ID: " + recoveryinvoiceid);

                Entity invoiceUpdate = new Entity("dfa_recoveryinvoice");
                Tracing.Trace("in the update block:");
                invoiceUpdate.Id = new Guid(recoveryinvoiceid);

                if (!string.IsNullOrEmpty(claimid))
                {
                    invoiceUpdate["dfa_claim"] = new EntityReference("dfa_projectclaim", new Guid(claimid)); ;
                }
                if (!string.IsNullOrEmpty(caseid))
                {
                    invoiceUpdate["dfa_case"] = new EntityReference("incident", new Guid(caseid)); ;
                }
                //Booleans
                if (receiveddatesameasinvoicedate != null)
                {
                    invoiceUpdate["dfa_receiveddatesameasinvoicedate"] = receiveddatesameasinvoicedate;
                }
                if (portionofinvoice != null)
                {
                    invoiceUpdate["dfa_portionofinvoice"] = portionofinvoice;
                }
                

                //Date times
                if (goodsorservicesreceiveddate != new DateTime())
                {
                    invoiceUpdate["dfa_goodsorservicesreceiveddate"] = goodsorservicesreceiveddate;
                }
                if (invoicedate != new DateTime())
                {
                    invoiceUpdate["dfa_invoicedate"] = invoicedate;
                }

                //Strings
                if (!string.IsNullOrEmpty(purpose))
                {
                    invoiceUpdate["dfa_purpose"] = purpose;
                }
                if (!string.IsNullOrEmpty(name))
                {
                    invoiceUpdate["dfa_name"] = name;
                }
                if (!string.IsNullOrEmpty(portioninvoicereason)) 
                {
                    invoiceUpdate["dfa_portioninvoicereason"] = portioninvoicereason;
                }
                if (!string.IsNullOrEmpty(invoicenumber))
                {
                    invoiceUpdate["dfa_invoicenumber"] = invoicenumber;
                }
                //Ints
                if (netinvoicedbeingclaimed != null)
                {
                    invoiceUpdate["dfa_netinvoicedbeingclaimed"] = new Money(netinvoicedbeingclaimed);
                }
                if (pst != null)
                {
                    invoiceUpdate["dfa_pst"] = new Money(pst);
                }

                if (grossgst != null)
                {
                    invoiceUpdate["dfa_grossgst"] = new Money(grossgst);
                }

                if (actualinvoicetotal != null)
                {
                    invoiceUpdate["dfa_actualinvoicetotal"] = new Money(actualinvoicetotal);
                }

                if (eligiblegst != null)
                {
                    invoiceUpdate["dfa_eligiblegst"] = new Money(eligiblegst);
                }

                Service.Update(invoiceUpdate);
                Tracing.Trace("Invoice is updated with guid:" + recoveryinvoiceid.ToString());
                InvoiceGUID.Set(context, "Updated Invoice GUID: " + recoveryinvoiceid.ToString());

            }
            else
            {
                Entity invoiceNew = new Entity("dfa_recoveryinvoice");

                if (!string.IsNullOrEmpty(claimid))
                {
                    invoiceNew["dfa_claim"] = new EntityReference("dfa_projectclaim", new Guid(claimid)); ;
                }
                if (!string.IsNullOrEmpty(caseid))
                {
                    invoiceNew["dfa_case"] = new EntityReference("incident", new Guid(caseid)); ;
                }
                //Booleans
                if (receiveddatesameasinvoicedate != null)
                {
                    invoiceNew["dfa_receiveddatesameasinvoicedate"] = receiveddatesameasinvoicedate;
                }
                if (portionofinvoice != null)
                {
                    invoiceNew["dfa_portionofinvoice"] = portionofinvoice;
                }

                //Date times
                if (goodsorservicesreceiveddate != new DateTime())
                {
                    invoiceNew["dfa_goodsorservicesreceiveddate"] = goodsorservicesreceiveddate;
                }
                if (invoicedate != new DateTime())
                {
                    invoiceNew["dfa_invoicedate"] = invoicedate;
                }

                //Strings
                if (!string.IsNullOrEmpty(purpose))
                {
                    invoiceNew["dfa_purpose"] = purpose;
                }
                if (!string.IsNullOrEmpty(name))
                {
                    invoiceNew["dfa_name"] = name;
                }
                if (!string.IsNullOrEmpty(portioninvoicereason))
                {
                    invoiceNew["dfa_portioninvoicereason"] = portioninvoicereason;
                }
                if (!string.IsNullOrEmpty(invoicenumber))
                {
                    invoiceNew["dfa_invoicenumber"] = invoicenumber;
                }
                //Ints
                if (netinvoicedbeingclaimed != null)
                {
                    invoiceNew["dfa_netinvoicedbeingclaimed"] = new Money(netinvoicedbeingclaimed);
                }
                if (pst != null)
                {
                    invoiceNew["dfa_pst"] = new Money(pst);
                }

                if (grossgst != null)
                {
                    invoiceNew["dfa_grossgst"] = new Money(grossgst);
                }

                if (actualinvoicetotal != null)
                {
                    invoiceNew["dfa_actualinvoicetotal"] = new Money(actualinvoicetotal);
                }

                if (eligiblegst != null)
                {
                    invoiceNew["dfa_eligiblegst"] = new Money(eligiblegst);
                }

                var invoiceGuid = Service.Create(invoiceNew);
                Tracing.Trace("Invoice created is : " + invoiceGuid.ToString());
                InvoiceGUID.Set(context, invoiceGuid.ToString());

            }
        }
    }
}

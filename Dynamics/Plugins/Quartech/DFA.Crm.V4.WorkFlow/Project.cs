using DFA.Crm.V4.Common;
using DFA.Crm.V4.Common.Model;
using DFA.Crm.V4.Core.Base.Contract;
using DFA.Crm.V4.Core.Base.Process;
using DFA.Crm.V4.Core.bcgov_documenturl.Contract;
using DFA.Crm.V4.Core.bcgov_documenturl.Process;
using DFA.Crm.V4.Core.Project.Contract;
using DFA.Crm.V4.Core.Project.Process;
using DFA.Crm.V4.Data.Base.Contract;
using DFA.Crm.V4.Data.Base.Repository;
using DFA.Crm.V4.Data.bcgov_documenturl.Contract;
using DFA.Crm.V4.Data.bcgov_documenturl.Process;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;

namespace DFA.Crm.V4.WorkFlow
{
    public class Project : CodeActivity
    {
        [Input("dfa_applicationid")]
        public InArgument<string> ApplicationId { get; set; }

        [Input("dfa_projectid")]
        public InArgument<string> ProjectId { get; set; }

        [Input("dfa_projectbusinessprocessstages")]
        public InArgument<string> Projectbusinessprocessstages { get; set; }

        [Input("dfa_projectbusinessprocesssubstages")]
        public InArgument<string> Projectbusinessprocesssubstages { get; set; }

        [Input("dfa_dateofdamagedifferencereason")]
        public InArgument<string> Dateofdamagedifferencereason { get; set; }

        [Input("dfa_emcrapprovalcomments")]
        public InArgument<string> Emcrapprovalcomments { get; set; }

        [Input("dfa_projectnumber")]
        public InArgument<string> Projectnumber { get; set; }

        [Input("dfa_projectname")]
        public InArgument<string> Projectname { get; set; }

        [Input("dfa_sitelocation")]
        public InArgument<string> Sitelocation { get; set; }

        [Input("dfa_descriptionofdamagedinfrastructure")]
        public InArgument<string> Descriptionofdamagedinfrastructure { get; set; }

        [Input("dfa_descriptionofdamagewithmaterial")]
        public InArgument<string> Descriptionofdamagewithmaterial { get; set; }

        [Input("dfa_descriptionofmaterialneededtorepair")]
        public InArgument<string> Descriptionofmaterialneededtorepair { get; set; }

        [Input("dfa_descriptionofrepairwork")]
        public InArgument<string> Descriptionofrepairwork { get; set; }

        [Input("dfa_descriptionofthecauseofdamage")]
        public InArgument<string> Descriptionofthecauseofdamage { get; set; }

        [Input("dfa_descriptionofdamage")]
        public InArgument<string> Descriptionofdamage { get; set; }

        //Date Time parameters
        [Input("dfa_estimatedcompletiondateofproject")]
        public InArgument<DateTime> Estimatedcompletiondateofproject { get; set; }

        [Input("dfa_projectapproveddate")]
        public InArgument<DateTime> Projectapproveddate { get; set; }

        [Input("dfa_dateofdamagefrom")]
        public InArgument<DateTime> Dateofdamagefrom { get; set; }

        [Input("dfa_dateofdamageto")]
        public InArgument<DateTime> Dateofdamageto { get; set; }

        [Input("dfa_dateofdamagesameasapplication ")]
        public InArgument<bool> Dateofdamagesameasapplication { get; set; }

        //Decimals and Integers
        [Input("dfa_estimatedcost")]
        public InArgument<decimal> Estimatedcost { get; set; }

        [Input("dfa_approvedcost")]
        public InArgument<decimal> Approvedcost { get; set; }

        [Input("dfa_actualcostwithtax")]
        public InArgument<decimal> Actualcostwithtax { get; set; }

        [Input("dfa_approvedpercentages")]
        public InArgument<decimal> Approvedpercentages { get; set; }

        [Input("dfa_applicanttype")]
        public InArgument<int> Applicanttype { get; set; }

        [Input("delete")]
        public InArgument<bool> Delete { get; set; }

        [Input("dfa_createdonportal")]
        public InArgument<bool> Createdonportal { get; set; }

        [Input("dfa_projecttype")]
        public InArgument<string> Projecttype { get; set; }

        [Input("dfa_projecttypeother")]
        public InArgument<string> Projecttypeother { get; set; }

        [Input("dfa_portalsubmitted")]
        public InArgument<bool> PortalSubmitted { get; set; }

        [Output("output")]
        public OutArgument<string> ProjectGUID { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            var xrmService = new DynamicsWorkflowService(context);
            ProjectGUID.Set(context, string.Empty);
            try
            {
                var model = this.GetProjectModel(context);
                if (model != null)
                {
                    IBaseRepository baseRepository = new BaseRepository(xrmService.UserService);
                    IProjectOperationsProcess projectOperationsProcess = new ProjectOperationsProcess(baseRepository, xrmService);

                    var response = projectOperationsProcess.Execute(model);

                    if (response != null) {
                        ProjectGUID.Set(context, response?.RecordId.ToString()); 
                    }

                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private ProjectWorkflowRequest GetProjectModel(CodeActivityContext context)
        {
            return new ProjectWorkflowRequest
            {
                ApplicationId = ApplicationId.Get(context),
                ProjectId = ProjectId.Get(context),
                Projectbusinessprocessstages = string.IsNullOrEmpty(Projectbusinessprocessstages.Get(context)) ? 0 : int.Parse(Projectbusinessprocessstages.Get(context)),
                Projectbusinessprocesssubstages = string.IsNullOrEmpty(Projectbusinessprocesssubstages.Get(context)) ? 0: int.Parse(Projectbusinessprocesssubstages.Get(context)),
                Dateofdamagedifferencereason = Dateofdamagedifferencereason.Get(context),
                Emcrapprovalcomments = Emcrapprovalcomments.Get(context),
                Projectnumber = Projectnumber.Get(context),
                Projectname = Projectname.Get(context),
                Sitelocation = Sitelocation.Get(context),
                Descriptionofdamagedinfrastructure = Descriptionofdamagedinfrastructure.Get(context),
                Descriptionofdamagewithmaterial = Descriptionofdamagewithmaterial.Get(context),
                Descriptionofmaterialneededtorepair = Descriptionofmaterialneededtorepair.Get(context),
                Descriptionofrepairwork = Descriptionofrepairwork.Get(context),
                Descriptionofthecauseofdamage = Descriptionofthecauseofdamage.Get(context),
                Descriptionofdamage = Descriptionofdamage.Get(context),
                Estimatedcompletiondateofproject = Estimatedcompletiondateofproject.Get(context),
                Projectapproveddate = Projectapproveddate.Get(context),
                Dateofdamagefrom = Dateofdamagefrom.Get(context),
                Dateofdamageto = Dateofdamageto.Get(context),
                Dateofdamagesameasapplication = Dateofdamagesameasapplication.Get(context),
                Estimatedcost = Estimatedcost.Get(context),
                Approvedcost = Approvedcost.Get(context),
                Actualcostwithtax = Actualcostwithtax.Get(context),
                Approvedpercentages = Approvedpercentages.Get(context),
                Applicanttype = Applicanttype.Get(context),
                Delete = Delete.Get(context),
                Createdonportal = Createdonportal.Get(context),
                Projecttype = string.IsNullOrEmpty(Projecttype.Get(context)) ? 0 : int.Parse(Projecttype.Get(context)),
                Projecttypeother = Projecttypeother.Get(context),
                PortalSubmitted = PortalSubmitted.Get<bool>(context),
                ProjectGUID = ProjectGUID.Get(context)
            };
        }
    }
}

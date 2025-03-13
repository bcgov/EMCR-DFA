using DFA.Portal.Custom.Actions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Xml.Linq;

namespace DFA.CRM.CustomWorkflow2
{
    public class CreateProject2 : CodeActivity
    {
        [Input("dfa_applicationid")]
        public InArgument<string> ApplicationId { get; set; }

        [Input("dfa_projectid")]
        public InArgument<string> ProjectId { get; set; }

        [Input("dfa_projectbusinessprocessstages")]
        public InArgument<int> Projectbusinessprocessstages { get; set; }

        [Input("dfa_projectbusinessprocesssubstages")]
        public InArgument<int> Projectbusinessprocesssubstages { get; set; }

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

        [Output("output")]
        public OutArgument<string> ProjectGUID { get; set; }

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

            var dfa_applicationid = ApplicationId.Get(context);
            var dfa_projectid = ProjectId.Get(context);
            var applicantType = ProjectId.Get(context);


            var dfa_projectbusinessprocessstages = Projectbusinessprocessstages.Get(context);
            var dfa_projectbusinessprocesssubstages = Projectbusinessprocesssubstages.Get(context);
            var dfa_dateofdamagedifferencereason = Dateofdamagedifferencereason.Get(context);
            var dfa_emcrapprovalcomments = Emcrapprovalcomments.Get(context);
            var dfa_projectnumber = Projectnumber.Get(context);
            var dfa_projectname = Projectname.Get(context);
            var dfa_sitelocation = Sitelocation.Get(context);
            var dfa_descriptionofdamagedinfrastructure = Descriptionofdamagedinfrastructure.Get(context);
            var dfa_descriptionofdamagewithmaterial = Descriptionofdamagewithmaterial.Get(context);
            var dfa_descriptionofmaterialneededtorepair = Descriptionofmaterialneededtorepair.Get(context);
            var dfa_descriptionofrepairwork = Descriptionofrepairwork.Get(context);
            var dfa_descriptionofthecauseofdamage = Descriptionofthecauseofdamage.Get(context);
            var dfa_descriptionofdamage = Descriptionofdamage.Get(context);
            var dfa_estimatedcompletiondateofproject = Estimatedcompletiondateofproject.Get(context);
            var dfa_projectapproveddate = Projectapproveddate.Get(context);
            var dfa_dateofdamageto = Dateofdamageto.Get(context);
            var dfa_dateofdamagefrom = Dateofdamagefrom.Get(context);
            var dfa_estimatedcost = Estimatedcost.Get(context);
            bool dfa_dateofdamagesameasapplication = Dateofdamagesameasapplication.Get(context);
            var dfa_approvedcost = Approvedcost.Get(context);
            var dfa_actualcostwithtax = Actualcostwithtax.Get(context);
            var dfa_createdonportal = Createdonportal.Get(context);
            // var dfa_applicantType = Applicanttype.Get(context);
            bool delete = Delete.Get(context);


            if (delete)
            {
                Tracing.Trace("deleting the guid id : " + dfa_projectid);
                Service.Delete("dfa_project", new Guid(dfa_projectid));
                ProjectGUID.Set(context, "deleted");
                Tracing.Trace("deleted the guid id : " + ProjectGUID);
                return;
            }
            if (!string.IsNullOrEmpty(dfa_projectid))
            {
                Tracing.Trace("retrieving the project with ID: " + dfa_projectid);
               // Entity currentProject = Service.Retrieve("dfa_project", new Guid(dfa_projectid), new ColumnSet(true));
                Entity projectUpdate = new Entity("dfa_project");
                Tracing.Trace("in the update block:");
                projectUpdate.Id = new Guid(dfa_projectid);

                //if (applicantType != null)
                //{
                //    Tracing.Trace("in the application type block:");
                //    if (dfa_applicantType == 222710000 || dfa_applicantType == 222710001 || dfa_applicantType == 222710002 || dfa_applicantType == 222710003 || dfa_applicantType == 222710004 || dfa_applicantType == 222710005 || dfa_applicantType == 222710006)
                //        projectUpdate["dfa_applicanttype"] = new OptionSetValue(dfa_applicantType);
                //}

                if (dfa_projectbusinessprocessstages != null)
                {
                    if (dfa_projectbusinessprocessstages == 222710000 || dfa_projectbusinessprocessstages == 222710001 ||
                        dfa_projectbusinessprocessstages == 222710002 || dfa_projectbusinessprocessstages == 222710003 ||
                        dfa_projectbusinessprocessstages == 222710004)

                        projectUpdate["dfa_projectbusinessprocessstages"] = new OptionSetValue(dfa_projectbusinessprocessstages);
                }

                if (dfa_projectbusinessprocesssubstages != null)
                {
                    if (dfa_projectbusinessprocesssubstages == 222710000 || dfa_projectbusinessprocesssubstages == 222710001 ||
                        dfa_projectbusinessprocesssubstages == 222710002 || dfa_projectbusinessprocesssubstages == 222710003 ||
                        dfa_projectbusinessprocesssubstages == 222710004 || dfa_projectbusinessprocesssubstages == 222710005 ||
                         dfa_projectbusinessprocesssubstages == 222710006 || dfa_projectbusinessprocesssubstages == 222710007)

                        projectUpdate["dfa_projectbusinessprocesssubstages"] = new OptionSetValue(dfa_projectbusinessprocesssubstages);
                }

                //if (dfa_dateofdamagesameasapplication != null && (dfa_dateofdamagesameasapplication == 222710000 || dfa_dateofdamagesameasapplication == 222710001))
                if (dfa_dateofdamagesameasapplication != null)
                {
                    Tracing.Trace("dfa_dateofdamagesameasapplication:");
                    projectUpdate["dfa_dateofdamagesameasapplication"] = dfa_dateofdamagesameasapplication;
                }


                if (!string.IsNullOrEmpty(dfa_dateofdamagedifferencereason))
                {
                    projectUpdate["dfa_dateofdamagedifferencereason"] = dfa_dateofdamagedifferencereason;
                }

                if (!string.IsNullOrEmpty(dfa_emcrapprovalcomments))
                {
                    projectUpdate["dfa_emcrapprovalcomments"] = dfa_emcrapprovalcomments;
                }
                if (!string.IsNullOrEmpty(dfa_projectnumber))
                {
                    projectUpdate["dfa_projectnumber"] = dfa_projectnumber;
                }
                if (!string.IsNullOrEmpty(dfa_projectname))
                {
                    projectUpdate["dfa_projectname"] = dfa_projectname;
                }
                if (!string.IsNullOrEmpty(dfa_sitelocation))
                {
                    projectUpdate["dfa_sitelocation"] = dfa_sitelocation;
                }

                if (!string.IsNullOrEmpty(dfa_descriptionofdamagedinfrastructure))
                {
                    projectUpdate["dfa_descriptionofdamagedinfrastructure"] = dfa_descriptionofdamagedinfrastructure;
                }
                if (!string.IsNullOrEmpty(dfa_descriptionofdamagewithmaterial))
                {
                    projectUpdate["dfa_descriptionofdamagewithmaterial"] = dfa_descriptionofdamagewithmaterial;
                }
                if (!string.IsNullOrEmpty(dfa_descriptionofmaterialneededtorepair))
                {
                    projectUpdate["dfa_descriptionofmaterialneededtorepair"] = dfa_descriptionofmaterialneededtorepair;
                }
                if (!string.IsNullOrEmpty(dfa_descriptionofrepairwork))
                {
                    projectUpdate["dfa_descriptionofrepairwork"] = dfa_descriptionofrepairwork;
                }
                if (!string.IsNullOrEmpty(dfa_descriptionofthecauseofdamage))
                {
                    projectUpdate["dfa_descriptionofthecauseofdamage"] = dfa_descriptionofthecauseofdamage;
                }
                if (!string.IsNullOrEmpty(dfa_descriptionofdamage))
                {
                    projectUpdate["dfa_descriptionofdamage"] = dfa_descriptionofdamage;
                }
                if (dfa_estimatedcompletiondateofproject != new DateTime())
                {
                    projectUpdate["dfa_estimatedcompletiondateofproject"] = dfa_estimatedcompletiondateofproject;
                }
                if (dfa_projectapproveddate != new DateTime())
                {
                    projectUpdate["dfa_projectapproveddate"] = dfa_projectapproveddate;
                }
                if (dfa_dateofdamageto != new DateTime())
                {
                    projectUpdate["dfa_dateofdamageto"] = dfa_dateofdamageto;
                }
                if (dfa_dateofdamagefrom != new DateTime())
                {
                    projectUpdate["dfa_dateofdamagefrom"] = dfa_dateofdamagefrom;
                }

                if (dfa_estimatedcost != null)
                {
                    projectUpdate["dfa_estimatedcost"] = new Money(dfa_estimatedcost);
                }
                if (dfa_approvedcost != null)
                {
                    projectUpdate["dfa_approvedcost"] = new Money(dfa_approvedcost);
                }
                if (dfa_actualcostwithtax != null)
                {
                    projectUpdate["dfa_actualcostwithtax"] = new Money(dfa_actualcostwithtax);
                }

                if (!string.IsNullOrEmpty(dfa_applicationid))
                {
                    projectUpdate["dfa_applicationid"] = new EntityReference("dfa_appapplication", new Guid(dfa_applicationid));
                }

                projectUpdate["dfa_createdonportal"] = true;

                Service.Update(projectUpdate);
                Tracing.Trace("project is updated with guid:" + dfa_projectid.ToString());
                ProjectGUID.Set(context, "Updated project GUID: " + dfa_projectid.ToString());

            }

            else
            {
                Entity project = new Entity("dfa_project");
                if (dfa_projectbusinessprocessstages != null)
                {
                    if (dfa_projectbusinessprocessstages == 222710000 || dfa_projectbusinessprocessstages == 222710001 ||
                        dfa_projectbusinessprocessstages == 222710002 || dfa_projectbusinessprocessstages == 222710003 ||
                        dfa_projectbusinessprocessstages == 222710004)

                        project["dfa_projectbusinessprocessstages"] = new OptionSetValue(dfa_projectbusinessprocessstages);
                }
                if (dfa_projectbusinessprocesssubstages != null)
                {
                    if (dfa_projectbusinessprocesssubstages == 222710000 || dfa_projectbusinessprocesssubstages == 222710001 ||
                        dfa_projectbusinessprocesssubstages == 222710002 || dfa_projectbusinessprocesssubstages == 222710003 ||
                        dfa_projectbusinessprocesssubstages == 222710004 || dfa_projectbusinessprocesssubstages == 222710005 ||
                         dfa_projectbusinessprocesssubstages == 222710006 || dfa_projectbusinessprocesssubstages == 222710007)

                        project["dfa_projectbusinessprocesssubstages"] = new OptionSetValue(dfa_projectbusinessprocesssubstages);
                }
                //if (dfa_dateofdamagesameasapplication != null && (dfa_dateofdamagesameasapplication == 222710000 || dfa_dateofdamagesameasapplication == 222710001))
                if (dfa_dateofdamagesameasapplication != null)
                { 
                    Tracing.Trace("dfa_dateofdamagesameasapplication:");
                    project["dfa_dateofdamagesameasapplication"] = dfa_dateofdamagesameasapplication;
                }
                
                //if (dfa_applicantType == 222710000 || dfa_applicantType == 222710001 || dfa_applicantType == 222710002 || dfa_applicantType == 222710003 || dfa_applicantType == 222710004 || dfa_applicantType == 222710005 || dfa_applicantType == 222710006)
                //    project["dfa_applicanttype"] = new OptionSetValue(dfa_applicantType);

                if (!string.IsNullOrEmpty(dfa_dateofdamagedifferencereason))
                {
                    project["dfa_dateofdamagedifferencereason"] = dfa_dateofdamagedifferencereason;
                }

                if (!string.IsNullOrEmpty(dfa_emcrapprovalcomments))
                {
                    project["dfa_emcrapprovalcomments"] = dfa_emcrapprovalcomments;
                }
                if (!string.IsNullOrEmpty(dfa_projectnumber))
                {
                    project["dfa_projectnumber"] = dfa_projectnumber;
                }
                if (!string.IsNullOrEmpty(dfa_projectname))
                {
                    project["dfa_projectname"] = dfa_projectname;
                }
                if (!string.IsNullOrEmpty(dfa_sitelocation))
                {
                    project["dfa_sitelocation"] = dfa_sitelocation;
                }

                if (!string.IsNullOrEmpty(dfa_descriptionofdamagedinfrastructure))
                {
                    project["dfa_descriptionofdamagedinfrastructure"] = dfa_descriptionofdamagedinfrastructure;
                }
                if (!string.IsNullOrEmpty(dfa_descriptionofdamagewithmaterial))
                {
                    project["dfa_descriptionofdamagewithmaterial"] = dfa_descriptionofdamagewithmaterial;
                }
                if (!string.IsNullOrEmpty(dfa_descriptionofmaterialneededtorepair))
                {
                    project["dfa_descriptionofmaterialneededtorepair"] = dfa_descriptionofmaterialneededtorepair;
                }
                if (!string.IsNullOrEmpty(dfa_descriptionofrepairwork))
                {
                    project["dfa_descriptionofrepairwork"] = dfa_descriptionofrepairwork;
                }
                if (!string.IsNullOrEmpty(dfa_descriptionofthecauseofdamage))
                {
                    project["dfa_descriptionofthecauseofdamage"] = dfa_descriptionofthecauseofdamage;
                }
                if (!string.IsNullOrEmpty(dfa_descriptionofdamage))
                {
                    project["dfa_descriptionofdamage"] = dfa_descriptionofdamage;
                }

                if (dfa_estimatedcompletiondateofproject != new DateTime())
                {
                    project["dfa_estimatedcompletiondateofproject"] = dfa_estimatedcompletiondateofproject;
                }
                if (dfa_projectapproveddate != new DateTime())
                {
                    project["dfa_projectapproveddate"] = dfa_projectapproveddate;
                }
                if (dfa_dateofdamageto != new DateTime())
                {
                    project["dfa_dateofdamageto"] = dfa_dateofdamageto;
                }
                if (dfa_dateofdamagefrom != new DateTime())
                {
                    project["dfa_dateofdamagefrom"] = dfa_dateofdamagefrom;
                }

                if (dfa_estimatedcost != null)
                {
                    project["dfa_estimatedcost"] = new Money(dfa_estimatedcost);
                }
                if (dfa_approvedcost != null)
                {
                    project["dfa_approvedcost"] = new Money(dfa_approvedcost);
                }
                if (dfa_actualcostwithtax != null)
                {
                    project["dfa_actualcostwithtax"] = new Money(dfa_actualcostwithtax);
                }

                if (!string.IsNullOrEmpty(dfa_applicationid))
                {
                    project["dfa_applicationid"] = new EntityReference("dfa_appapplication", new Guid(dfa_applicationid));
                }
                project["dfa_createdonportal"] = true;

                var projectGuid = Service.Create(project);
                Tracing.Trace("Project created is : " + projectGuid.ToString());
                ProjectGUID.Set(context, projectGuid.ToString());
            }

        }
    }
}

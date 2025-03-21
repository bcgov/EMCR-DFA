using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Windows.Navigation;
using DFA.Crm.V4.Common.Constants;
using DFA.Crm.V4.Common.Model;
using DFA.Crm.V4.Core.Base.Contract;
using DFA.Crm.V4.Core.Base.Process;
using DFA.Crm.V4.Core.Project.Contract;
using DFA.Crm.V4.Data.Base.Contract;
using DFA.Crm.V4.Data.Base.Repository;
using DFA.Crm.V4.WorkFlow;
using Microsoft.Xrm.Sdk;
using static DFA.Crm.V4.Common.Constants.Constants;
using DFA.Crm.V4.Common.Extensions;
using DFA.Crm.V4.Common;

namespace DFA.Crm.V4.Core.Project.Process
{
    public class ProjectOperationsProcess : IProjectOperationsProcess
    {
        private readonly IBaseRepository baseRepository;
        private readonly IDynamicsService dynamicsService;

        public ProjectOperationsProcess(IBaseRepository baseRepository, IDynamicsService dynamicsService)
        {
            this.baseRepository = baseRepository;
            this.dynamicsService = dynamicsService;
        }

        public ProjectWorkflowResponse Execute(ProjectWorkflowRequest projectWorkflowInputModel)
        {
            if (projectWorkflowInputModel.Delete)
            {
                //DeleteProject
                return this.DeleteProject(projectWorkflowInputModel.ProjectId, Constants.ProjectEntity.EntityName);
            }

            if (!string.IsNullOrEmpty(projectWorkflowInputModel.ProjectId))
            {
                // UpdateRecord
                return this.UpdateProject(projectWorkflowInputModel);

            }
            else
            {
                // Create Record
                return this.CreateProject(projectWorkflowInputModel);
            }

            return null;

        }

        public ProjectWorkflowResponse DeleteProject(string recordType, string recordId)
        {

            if (!string.IsNullOrEmpty(recordId) && !string.IsNullOrEmpty(recordType))
            {
                dynamicsService.TracingService.Trace("Delete Action");

                if (Guid.TryParse(recordId, out Guid id))
                {
                    var deleted = baseRepository.Delete(Constants.ProjectEntity.EntityName, id);

                    return new ProjectWorkflowResponse
                    {
                        RecordId = id,
                        Result = deleted,
                        ErrorMessage = deleted ? "Deleted Successfully" : "An error occured while deletion"
                    };
                }
                else
                {
                    throw new InvalidOperationException("Record Id is not parsable to Guid");
                }
            }

            return new ProjectWorkflowResponse
            {
                RecordId = Guid.NewGuid(),
                Result = false,
                ErrorMessage = "An error occured while deletion, Project Id or ENtity Name is null"
            };
        }

        public ProjectWorkflowResponse CreateProject(ProjectWorkflowRequest projectWorkflowInputModel)
        {
            dynamicsService.TracingService.Trace("ProjectWorkflowResponse: Create Action");
            var project = this.SetProjectFields(projectWorkflowInputModel);
            dynamicsService.TracingService.Trace("ProjectWorkflowResponse: Project Attributes - " + String.Join(",", project.Attributes.Keys.ToArray()));

            Guid recordId = this.baseRepository.Create(project);

            dynamicsService.TracingService.Trace($"Record Created, {recordId.ToString()}");

            return new ProjectWorkflowResponse
            {
                RecordId = recordId,
                Result = true,
                ErrorMessage = "Record is created!"
            };


        }

        public ProjectWorkflowResponse UpdateProject(ProjectWorkflowRequest projectWorkflowInputModel)
        {
            dynamicsService.TracingService.Trace("Update Action");
            if (Guid.TryParse(projectWorkflowInputModel.ProjectId, out Guid id))
            {

                var project = this.SetProjectFields(projectWorkflowInputModel);
                project.Id = id;

                dynamicsService.TracingService.Trace("Maping completed");

                var updated = baseRepository.Update(project);

                return new ProjectWorkflowResponse
                {
                    RecordId = id,
                    Result = true,
                    ErrorMessage = "Record is updated!"
                };
            }
            else
            {
                throw new InvalidOperationException("Record Id is not parsable to Guid");
            }
        }


        private Entity SetProjectFields(ProjectWorkflowRequest model)
        {
            Entity projectEntity = new Entity(Constants.ProjectEntity.EntityName);

            var fields = new Dictionary<string, object>
        {
            { "dfa_projectbusinessprocessstages", model.Projectbusinessprocessstages.MapToOptionsetValueOrNull() },
            { "dfa_projectbusinessprocesssubstages", model.Projectbusinessprocesssubstages.MapToOptionsetValueOrNull() },
            { "dfa_dateofdamagesameasapplication", model.Dateofdamagesameasapplication },
            { "dfa_dateofdamagedifferencereason", model.Dateofdamagedifferencereason },
            { "dfa_emcrapprovalcomments", model.Emcrapprovalcomments },
            { "dfa_projectnumber", model.Projectnumber },
            { "dfa_projectname", model.Projectname },
            { "dfa_sitelocation", model.Sitelocation },
            { "dfa_descriptionofdamagedinfrastructure", model.Descriptionofdamagedinfrastructure },
            { "dfa_descriptionofdamagewithmaterial", model.Descriptionofdamagewithmaterial },
            { "dfa_descriptionofmaterialneededtorepair", model.Descriptionofmaterialneededtorepair },
            { "dfa_descriptionofrepairwork", model.Descriptionofrepairwork },
            { "dfa_descriptionofthecauseofdamage", model.Descriptionofthecauseofdamage },
            { "dfa_descriptionofdamage", model.Descriptionofdamage },
            { "dfa_estimatedcompletiondateofproject",  model.Estimatedcompletiondateofproject.MapToDateTimeOrNull() },
            { "dfa_projectapproveddate", model.Projectapproveddate.MapToDateTimeOrNull() },
            { "dfa_dateofdamageto", model.Dateofdamageto.MapToDateTimeOrNull() },
            { "dfa_dateofdamagefrom", model.Dateofdamagefrom.MapToDateTimeOrNull() },
            { "dfa_estimatedcost", model.Estimatedcost.MapToMoneyOrNull() },
            { "dfa_approvedcost", model.Approvedcost.MapToMoneyOrNull() },
            { "dfa_actualcostwithtax", model.Actualcostwithtax.MapToMoneyOrNull() },
            { "dfa_applicationid", model.ApplicationId.MapToEntityReferenceOrNull("dfa_appapplication") },
            { "dfa_projecttype", model.Projecttype.MapToOptionsetValueOrNull() },
            { "dfa_projecttypeother", model.Projecttypeother },
            { "dfa_createdonportal", model.Createdonportal },
            { "dfa_portalsubmitted", model.PortalSubmitted },
        };

            fields.Where(field => field.Value != null).ToList().ForEach(field =>
            {
                //tracing.Trace($"Setting field: {field.Key}");
                projectEntity[field.Key] = field.Value;
            });

            return projectEntity;
        }
    }
}

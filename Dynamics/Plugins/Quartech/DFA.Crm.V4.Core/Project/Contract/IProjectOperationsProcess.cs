using System;
using System.Collections.Generic;
using System.Text;
using DFA.Crm.V4.Common.Model;

namespace DFA.Crm.V4.Core.Project.Contract
{
    public interface IProjectOperationsProcess
    {
        ProjectWorkflowResponse Execute(ProjectWorkflowRequest projectWorkflowInputModel);

        ProjectWorkflowResponse DeleteProject(string recordType, string recordId);

        ProjectWorkflowResponse CreateProject(ProjectWorkflowRequest projectWorkflowInputModel);

        ProjectWorkflowResponse UpdateProject(ProjectWorkflowRequest projectWorkflowInputModel);
    }
}

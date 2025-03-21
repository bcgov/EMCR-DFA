using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xrm.Sdk;

namespace DFA.Crm.V4.Common.Model
{
    public class ProjectWorkflowResponse
    {
        public bool Result{ get; set; }
        public string ErrorMessage{ get; set; }
        
        public Guid RecordId{ get; set; }
    }
}

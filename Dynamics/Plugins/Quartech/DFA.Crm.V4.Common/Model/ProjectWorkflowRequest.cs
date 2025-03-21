using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xrm.Sdk;

namespace DFA.Crm.V4.Common.Model
{
    public class ProjectWorkflowRequest
    {
        public string ApplicationId { get; set; }
        public string ProjectId { get; set; }
        public int? Projectbusinessprocessstages { get; set; }
        public int? Projectbusinessprocesssubstages { get; set; }
        public string Dateofdamagedifferencereason { get; set; }
        public string Emcrapprovalcomments { get; set; }
        public string Projectnumber { get; set; }
        public string Projectname { get; set; }
        public string Sitelocation { get; set; }
        public string Descriptionofdamagedinfrastructure { get; set; }
        public string Descriptionofdamagewithmaterial { get; set; }
        public string Descriptionofmaterialneededtorepair { get; set; }
        public string Descriptionofrepairwork { get; set; }
        public string Descriptionofthecauseofdamage { get; set; }
        public string Descriptionofdamage { get; set; }
        public DateTime? Estimatedcompletiondateofproject { get; set; }
        public DateTime? Projectapproveddate { get; set; }
        public DateTime? Dateofdamagefrom { get; set; }
        public DateTime? Dateofdamageto { get; set; }
        public bool Dateofdamagesameasapplication { get; set; }
        public decimal? Estimatedcost { get; set; }
        public decimal? Approvedcost { get; set; }
        public decimal? Actualcostwithtax { get; set; }
        public decimal? Approvedpercentages { get; set; }
        public int Applicanttype { get; set; }
        public bool Delete { get; set; }
        public bool Createdonportal { get; set; }

        public bool PortalSubmitted { get; set; }
        public int? Projecttype { get; set; }
        public string Projecttypeother { get; set; }
        public string ProjectGUID { get; set; }
    }
}

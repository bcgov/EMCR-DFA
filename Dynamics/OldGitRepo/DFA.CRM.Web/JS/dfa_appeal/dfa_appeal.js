// JavaScript source code

if (typeof DFA === "undefined") {
    var DFA = {};
}

if (typeof DFA.Jscripts === "undefined") {
    DFA.Jscripts = {};
}

DFA.Jscripts.Appeal = {
    appeal_ExecutionContext: null,
    OnLoad: function (executionContext) {
        this.appeal_ExecutionContext = executionContext;
        var formContext = executionContext.getFormContext();
        var formType = formContext.ui.getFormType();

        if (formType != 1) {
            DFA.Jscripts.Appeal.showHideProjectLookup(executionContext);
            dfa_Utility.enableDisable(executionContext, true, "dfa_caseid,dfa_projectid");
        }
    },

    showHideProjectLookup: function (executionContext) {
        var caseValue = dfa_Utility.getAttributeValue(executionContext, "dfa_caseid");
        if (caseValue == null) {
            return; // Nothing to be based for the search
        }
        var caseId = caseValue[0].id.replace('{', '').replace('}', '');
        Xrm.WebApi.retrieveRecord("incident", caseId, "?$select=dfa_applicanttype").then(
            function caseSuccess(caseData) {
                var applicantType = caseData.dfa_applicanttype;
                showProjectLookupField = applicantType == 222710005;
                var requiredLevel = "none";
                if (showProjectLookupField) {
                    requiredLevel = "required";
                }
                dfa_Utility.showHide(executionContext, showProjectLookupField, "dfa_projectid");
                dfa_Utility.enableDisable(executionContext, !showProjectLookupField, "dfa_projectid");
                dfa_Utility.setRequiredLevel(executionContext, requiredLevel, "dfa_projectid");
                if (showProjectLookupField) {
                    dfa_Utility.filterOutOptionSet(executionContext, "dfa_appealtype", "222710000,222710002");
                }
            },
            function caseFail(err) {
                var debug = err;
            }
        );
    },

    populateCaseLookupByProject: function (executionContext) {
        var projectValue = dfa_Utility.getAttributeValue(executionContext, "dfa_projectid");
        if (projectValue == null) {
            return;
        }
        var projectId = projectValue[0].id.replace('{', '').replace('}', '');
        var caseOptions = "?$select=_dfa_caseid_value&$expand=dfa_CaseId($select=title)";
        Xrm.WebApi.retrieveRecord("dfa_project", projectId, caseOptions).then(
            function projectSuccess(projectData) {
                var caseId = projectData._dfa_caseid_value;
                var caseTitle = projectData.dfa_CaseId.title;
                var caseLookupObj = dfa_Utility.generateLookupObject("incident", caseId, caseTitle);
                dfa_Utility.setLookupValue(executionContext, "dfa_caseid", caseLookupObj);
                // They changes mind again.  Allow all types of Appeal
                // dfa_Utility.filterOutOptionSet(executionContext, "dfa_appealtype", "222710000,222710002");
            },
            function caseFail(err) {
                var debug = err;
            }
        );
    }
}
// JavaScript source code

if (typeof DFA === "undefined") {
    var DFA = {};
}

if (typeof DFA.Jscripts === "undefined") {
    DFA.Jscripts = {};
}

DFA.Jscripts.Project = {
    project_ExecutionContext: null,
    OnLoad: function (executionContext) {
        this.project_ExecutionContext = executionContext;
        var formContext = executionContext.getFormContext();
        var formType = formContext.ui.getFormType();

        if (formType != 1) {
            DFA.Jscripts.Project.showHideAppealTab(executionContext);
            DFA.Jscripts.Project.showHideAdjustmentTab(executionContext);

        }
    },

    showHideAppealTab: function (executionContext) {
        var formContext = executionContext.getFormContext();
        var entityId = formContext.data.entity.getId().replace('{', '').replace('}', '');
        var entityName = formContext.data.entity.getEntityName();
        if (entityName != "dfa_project") {
            return;
        }
        var isBeingAppealed = dfa_Utility.getAttributeValue(executionContext, "dfa_isbeingappealed");
        if (isBeingAppealed == null) {
            isBeingAppealed = false;
        }
        var options = "?$filter=_dfa_projectid_value eq " + entityId;
        Xrm.WebApi.retrieveMultipleRecords("dfa_appeal", options).then(
            function success(results) {
                var rowCount = results.entities.length;
                var showAppealTab = isBeingAppealed || rowCount > 0;
                dfa_Utility.showHide(executionContext, showAppealTab, "tab_appeal");
                dfa_Utility.RefreshTimeLine(executionContext, "Timeline");
            },
            function failure(err) {
                var debug = err;
            }
        );


    },

    hasAppeal: function (formContext) {
        // Use from Ribbon Button to check if there is already an Appeal
        //var formContext = this.project_ExecutionContext.getFormContext();
        var entityId = formContext.data.entity.getId().replace('{', '').replace('}', '');
        var entityName = formContext.data.entity.getEntityName();
        //var isBeingAppealed = dfa_Utility.getAttributeValue(this.project_ExecutionContext, "dfa_isbeingappealed");
        //if (isBeingAppealed == true) {
        //    return true;
        //}
        var options = "?$filter=_dfa_projectid_value eq " + entityId;
        if (entityName == "incident") {
            options = "?$filter=_dfa_caseid_value eq " + entityId;
        }
        //var results = DFA.Jscripts.Project.retrieveMultipleCustom("dfa_appeal", options);
        var results = dfa_Utility.retrieveMultipleCustom("dfa_appeal", options);
        return results.length > 0;
    },


    showHideAdjustmentTab: function (executionContext) {
        var formContext = executionContext.getFormContext();
        var entityId = formContext.data.entity.getId().replace('{', '').replace('}', '');
        var entityName = formContext.data.entity.getEntityName();
        if (entityName != "dfa_project") {
            return;
        }

        var isApproved = dfa_Utility.getAttributeValue(executionContext, "statuscode");
        if (isApproved != '222710001') {
            dfa_Utility.showHide(executionContext, false, "Project_adjustment_payment_tab");
        } else {
            dfa_Utility.showHide(executionContext, true, "Project_adjustment_payment_tab");
        }
    }
};
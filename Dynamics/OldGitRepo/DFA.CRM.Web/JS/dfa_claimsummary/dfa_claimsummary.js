// JavaScript source code

if (typeof DFA === "undefined") {
    var DFA = {};
}

if (typeof DFA.Jscripts === "undefined") {
    DFA.Jscripts = {};
}

DFA.Jscripts.ClaimSummary = {
    OnLoad: function (executionContext) {
        var formContext = executionContext.getFormContext();
        var formType = formContext.ui.getFormType();

        if (formType != 1) {
            DFA.Jscripts.ClaimSummary.showHideFederalClaim(executionContext);
        }
    },

    showHideFederalClaim: function (executionContext) {
        var formContext = executionContext.getFormContext();
        var caseAttribute = formContext.getAttribute("dfa_caseid");
        if (caseAttribute != null && caseAttribute.getValue() != null) {
            var caseId = caseAttribute.getValue()[0].id.replace('{', '').replace('}', '');
            Xrm.WebApi.retrieveRecord("incident", caseId, "?$select=_dfa_eventid_value").then(
                function caseSuccess(caseData) {
                    var eventId = caseData._dfa_eventid_value;
                    if (eventId != null) {
                        eventId = eventId.replace('{', '').replace('}', '');
                        var eventOptions = "?$select=dfa_eventscope";
                        Xrm.WebApi.retrieveRecord("dfa_event", eventId, eventOptions).then(
                            function success(data) {
                                var eventScope = data.dfa_eventscope;
                                var isProvincial = eventScope != 222710001;
                                if (isProvincial) { // If Not Federal
                                    dfa_Utility.showHide(executionContext, !isProvincial, "dfa_federalclaim");
                                }
                            },
                            function error(err) {
                                var debug = err;
                            }
                        );
                    }
                },
                function caseFail(err) {
                    var debug = err;
                }
            );
        }
    }
}
// JavaScript source code

if (typeof DFA === "undefined") {
    var DFA = {};
}

if (typeof DFA.Jscripts === "undefined") {
    DFA.Jscripts = {};
}

DFA.Jscripts.PhoneCall = {
    phonecall_ExecutionContext: null,
    OnLoad: function (executionContext) {
        this.phonecall_ExecutionContext = executionContext;
        DFA.Jscripts.PhoneCall.showHideLegacyFields(executionContext);
        DFA.Jscripts.PhoneCall.populateClaimType(executionContext);
    },

    showHideLegacyFields: function (executionContext) {
        var callFromLegacyValue = dfa_Utility.getAttributeValue(executionContext, "dfa_callfromlegacy");
        var callToLegacyValue = dfa_Utility.getAttributeValue(executionContext, "dfa_calltolegacy");
        var hasLegacyValues = (callFromLegacyValue != null && callFromLegacyValue != "") ||
            (callToLegacyValue != null && callToLegacyValue != "");
        var oobFieldsRequiredLevel = "required";
        if (hasLegacyValues) {
            oobFieldsRequiredLevel = "recommended";
        }
        dfa_Utility.showHide(executionContext, hasLegacyValues, "dfa_callfromlegacy");
        dfa_Utility.showHide(executionContext, hasLegacyValues, "dfa_calltolegacy");
        dfa_Utility.showHide(executionContext, hasLegacyValues, "phonecall:section_legacyparticipants");
        dfa_Utility.setBPFRequiredLevel(executionContext, oobFieldsRequiredLevel, "from");
        dfa_Utility.setBPFRequiredLevel(executionContext, oobFieldsRequiredLevel, "to");

    },

    populateClaimType: function (executionContext) {
        // Check if Claim Type already contains data
        var claimTypeValue = dfa_Utility.getAttributeValue(executionContext, "dfa_claimtype");

        // Obtain Regarding Object
        var regardingObjectId = dfa_Utility.getAttributeValue(executionContext, "regardingobjectid");
        if (regardingObjectId == null) {
            return;
        }

        // Can only populate Claim Type if regarding object is a Case (incident)
        var entityType = regardingObjectId[0].entityType;
        if (entityType != "incident") {
            return;
        }

        var caseId = regardingObjectId[0].id.replace('{', '').replace('}', '');
        Xrm.WebApi.retrieveRecord("incident", caseId, "?$select=dfa_applicanttype").then(
            function caseSuccess(caseData) {
                var claimTypeOption = caseData.dfa_applicanttype;
                if (claimTypeValue != claimTypeOption) {
                    dfa_Utility.setFieldValue(executionContext, "dfa_claimtype", claimTypeOption);
                }
            },
            function caseFail(err) {
                var debug = err;
            }
        );
    }
}
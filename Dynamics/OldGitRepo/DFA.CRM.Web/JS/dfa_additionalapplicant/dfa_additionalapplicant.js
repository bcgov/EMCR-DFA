// JavaScript source code

if (typeof DFA === "undefined") {
    var DFA = {};
}

if (typeof DFA.Jscripts === "undefined") {
    DFA.Jscripts = {};
}

DFA.Jscripts.AdditonalApplicant = {
    additonalapplicant_ExecutionContext: null,
    OnLoad: function (executionContext) {
        this.additonalapplicant_ExecutionContext = executionContext;
        var formContext = executionContext.getFormContext();
        var formType = formContext.ui.getFormType();

        if (formType != 1) {
            DFA.Jscripts.AdditonalApplicant.showHide(executionContext);
        }
    },

    showHide: function (executionContext) {
        var formContext = executionContext.getFormContext();
        var applicantTypeOS = formContext.getAttribute("dfa_applicanttype");
        var additionalApplicantLookup = formContext.getAttribute("dfa_customer");
        
        if (applicantTypeOS == null || additionalApplicantLookup == null) {
            return;
        }
        var applicantType = applicantTypeOS.getValue();
        var customer = additionalApplicantLookup.getValue();
        var lockDetailFields = (customer != null);
        // Lock Fields
        dfa_Utility.enableDisable(executionContext, lockDetailFields, "dfa_organizationname");
        dfa_Utility.enableDisable(executionContext, lockDetailFields, "dfa_firstname");
        dfa_Utility.enableDisable(executionContext, lockDetailFields, "dfa_lastname");
        dfa_Utility.enableDisable(executionContext, lockDetailFields, "dfa_emailaddress");
        dfa_Utility.enableDisable(executionContext, lockDetailFields, "dfa_phonenumber");

        switch (applicantType) {
            case 222710000: // Contact
                dfa_Utility.showHide(executionContext, true, "dfa_customer");
                dfa_Utility.showHide(executionContext, false, "dfa_organizationname");
                dfa_Utility.showHide(executionContext, true, "dfa_firstname");
                dfa_Utility.showHide(executionContext, true, "dfa_lastname");
                dfa_Utility.showHide(executionContext, true, "dfa_emailaddress");
                dfa_Utility.showHide(executionContext, true, "dfa_phonenumber");
                break;
            case 222710001: // Organization
                dfa_Utility.showHide(executionContext, true, "dfa_customer");
                dfa_Utility.showHide(executionContext, true, "dfa_organizationname");
                dfa_Utility.showHide(executionContext, false, "dfa_firstname");
                dfa_Utility.showHide(executionContext, false, "dfa_lastname");
                dfa_Utility.showHide(executionContext, true, "dfa_emailaddress");
                dfa_Utility.showHide(executionContext, true, "dfa_phonenumber");
                break;
            default:
                // Hide all other fields
                dfa_Utility.showHide(executionContext, false, "dfa_customer");
                dfa_Utility.showHide(executionContext, false, "dfa_organizationname");
                dfa_Utility.showHide(executionContext, false, "dfa_firstname");
                dfa_Utility.showHide(executionContext, false, "dfa_lastname");
                dfa_Utility.showHide(executionContext, false, "dfa_emailaddress");
                dfa_Utility.showHide(executionContext, false, "dfa_phonenumber");
                break;
        }
    }
}
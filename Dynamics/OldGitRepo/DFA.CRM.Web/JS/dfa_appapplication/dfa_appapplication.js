// JavaScript source code
// JavaScript source code
// V2231117
// V202401222 


if (typeof DFA === "undefined") {
    var DFA = {};
}

if (typeof DFA.Jscripts === "undefined") {
    DFA.Jscripts = {};
}

DFA.Jscripts.AppApplication = {
    dfa_ExecutionContext: null,
    OnLoad: function (executionContext) {
        this.dfa_ExecutionContext = executionContext;
        // var formContext = executionContext.getFormContext();
        // var formType = formContext.ui.getFormType();
        DFA.Jscripts.AppApplication.SwitchFormByApplicantType(executionContext);
        DFA.Jscripts.AppApplication.ToggleLevelOfRequirementOnReviewStatus(executionContext);
        dfa_Utility.enableDisable(executionContext, true, "dfa_decidedbyid");
        dfa_Utility.enableDisable(executionContext, true, "dfa_decisiondate");
    },
    SwitchFormByApplicantType: function (executionContext) {
        var formContext = executionContext.getFormContext();
        var applicantTypeAttributeName = "dfa_applicanttype";
        var privateIndividualFormName = "Application (Private - Individual)";
        var privateBusinessFormName = "Application (Private - Organization)";
        var publicFormName = "Application (Local Govt)";
        var publicProjectFormName = "Application (BC Local Govt)";
        var appString;
        const PUBLIC_APP = "3d27a86f-57c6-ec11-b832-00505683fbf4";
        const PRIVATE_APP = "6d5e3ea0-bcb5-4569-b0df-7962feb463c5";
        var isPublicApp = false;
        var isPrivateApp = false;
        var message;

        // debugger;

        DFA.Jscripts.AppApplication.getAppId().then(function (appId) {

            if (appId) {
                // alert("App ID: " + appId);
                appString = appId.toString();
            }
            else {
                alert("App ID retrieve Failed");
            }
        }).catch(function (error) {
            alert("App ID retrieve Failed");
        });

        if (appString != null) {
            if (appString == PUBLIC_APP) {
                isPublicApp = true;
            }
            else if (appString == PRIVATE_APP) {
                isPrivateApp = true;
            }
        }

        var applicantTypeAttribute = formContext.getAttribute(applicantTypeAttributeName);
        if (applicantTypeAttribute == null || applicantTypeAttribute.getValue() == null) {
            return; // Do nothing
        }
        var applicantTypeValue = applicantTypeAttribute.getValue();
        var isPrivateRecord = (applicantTypeValue != 222710005);
        var hasAuthority = false;
        if (isPrivateRecord) {
            hasAuthority = dfa_Utility.checkCurrentUserRole("EMBC-DFA Application Review (Private)") ||
                dfa_Utility.checkCurrentUserRole("System Administrator");
        }
        else {
            hasAuthority = dfa_Utility.checkCurrentUserRole("EMBC-DFA Application Review (Public)") ||
                dfa_Utility.checkCurrentUserRole("System Administrator");
        }

        if (!hasAuthority) {
            var message = "You don't have authority to open this record."
            var alertStrings = { confirmButtonLabel: "OK", text: message, title: "" };
            var alertOptions = { height: 120, width: 260 };
            Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                function successCallBack() {
                    formContext.ui.close();
                });
        }

        var currentForm = formContext.ui.formSelector.getCurrentItem().getLabel();
        var isPrivateForm = false;

        //        if (currentForm != publicFormName || currentForm != publicProjectFormName) {
        //            isPrivateForm = true;
        //        }

        const publicFormNames = [publicFormName, publicProjectFormName];
        if (!publicFormNames.includes(currentForm)) {
            isPrivateForm = true;
        };
        console.log("private:", isPrivateForm);

        if (isPrivateForm && isPrivateRecord && isPublicApp) {
            message = "You are using the Public Local Government App to open a private sector application record.  Please change to Private App and try again."
            var alertStrings = { confirmButtonLabel: "OK", text: message, title: "" };
            var alertOptions = { height: 120, width: 260 };
            Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                function successCallBack() {
                    formContext.ui.close();
                });
        }
        else if (!isPrivateForm && !isPrivateRecord && isPrivateApp) {
            message = "You are using the Private Sector App to open a public sector application record.  Please change to Public App and try again."
            var alertStrings = { confirmButtonLabel: "OK", text: message, title: "" };
            var alertOptions = { height: 120, width: 260 };
            Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                function successCallBack() {
                    formContext.ui.close();
                });
        }



        //if ((isPrivateForm && !isPrivateRecord) ||
        //    (!isPrivateForm && isPrivateRecord)) {
        //    var message = "You are using the Public App currently trying to open a private sector application record.  Please change to Private App and try again."
        //    if (isPrivateForm) {
        //        message = "You are using the Private App currently trying to open a public sector application record.  Please change to Public App and try again."
        //    }
        //    var alertStrings = { confirmButtonLabel: "OK", text: message, title: "" };
        //    var alertOptions = { height: 120, width: 260 };
        //    Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
        //        function successCallBack() {
        //            //       formContext.ui.close();
        //        });
        //}

        if (DFA.Jscripts.AppApplication.SwitchFormByInsuranceCoverage(executionContext))
            return;

        switch (applicantTypeValue) {
            case 222710000: // Charitable Organization
            case 222710001: // Farm Owner
            case 222710004: // Small Business Owner
            case 222710006: // Incorporated
                dfa_Utility.navigateToFormByName(executionContext, privateBusinessFormName);
                break;
            case 222710002: // Home Owner
            case 222710003: // Residential Tenant
                dfa_Utility.navigateToFormByName(executionContext, privateIndividualFormName);
                break;
            case 222710005: // Indigenous/Local Government Body
                //      dfa_Utility.navigateToFormByName(executionContext, publicProjectFormName);
                break;
        }
    },

    getAppId: function () {
        return Xrm.Utility.getGlobalContext().getCurrentAppProperties().then(
            function (appProperties) {
                // alert("App ID: " + appProperties.appId);
                return appProperties.appId;
            }).catch(function (error) {
                alert("error retrieving App ID: ");
                return null;
            }
            );
    },
    PopulateDamageCityByMailingCityForPublicSectorForm: function (executionContext) {
        var formContext = executionContext.getFormContext();
        var formType = formContext.ui.getFormType();
        if (formType != 1 &&  // Create
            formType != 2) {  // Update
            return;
        }
        var mailingCityAttributeName = "dfa_areacommunity2id";
        var damageCityAttributeName = "dfa_areacommunityid";
        var mailingCityValue = dfa_Utility.getAttributeValue(executionContext, mailingCityAttributeName);
        //Project team update.
        //formContext.getAttribute(damageCityAttributeName).setValue(mailingCityValue);
    },
    ToggleLevelOfRequirementOnReviewStatus: function (executionContext) {
        var formContext = executionContext.getFormContext();
        var formType = formContext.ui.getFormType();
        if (formType != 1 &&  // Create
            formType != 2) {  // Update
            return;
        }


        var applicantTypeAttributeName = "dfa_applicanttype";
        var applicantTypeAttribute = formContext.getAttribute(applicantTypeAttributeName);
        if (applicantTypeAttribute == null || applicantTypeAttribute.getValue() == null) {
            return; // Do nothing
        }
        var reviewStatusAttributeName = "dfa_reviewstatus";
        var reviewStatusAttribute = formContext.getAttribute(reviewStatusAttributeName);
        if (reviewStatusAttribute == null || reviewStatusAttribute.getValue() == null) {
            return; // Do Nothing.
        }
        var reviewStatus = reviewStatusAttribute.getValue();
        var applicantTypeValue = applicantTypeAttribute.getValue();
        var reviewStatusApproved = reviewStatus == 222710002;
        var reviewStatusDenied = reviewStatus == 222710003;

        var approvedRequiredLevel = "none";
        var deniedRequiredLevel = "none";
        if (reviewStatusDenied) {
            deniedRequiredLevel = "required";
        }
        if (reviewStatusApproved) {
            approvedRequiredLevel = "required";
        }

        var decidedByAttributeName = "dfa_decidedbyid";
        var decisionDateAttributeName = "dfa_decisiondate";
        var decidedByRequiredLevel = "none";
        if (reviewStatusApproved || reviewStatusDenied) {
            decidedByRequiredLevel = "required";
            var userSettings = Xrm.Utility.getGlobalContext().userSettings;
            var currentuserid = userSettings.userId;
            var username = userSettings.userName;
            var userLookup = dfa_Utility.generateLookupObject("systemuser", currentuserid, username);
            dfa_Utility.setLookupValue(executionContext, decidedByAttributeName, userLookup);
            var currentDate = new Date();
            formContext.getAttribute(decisionDateAttributeName).setValue(currentDate);
        }
        else {
            dfa_Utility.clearFieldData(executionContext, decidedByAttributeName);
            dfa_Utility.clearFieldData(executionContext, decisionDateAttributeName);
        }

        // Common required field on approval
        dfa_Utility.setRequiredLevel(executionContext, deniedRequiredLevel, "dfa_decisioncomments");
        dfa_Utility.setRequiredLevel(executionContext, approvedRequiredLevel, "dfa_datereceived");
        dfa_Utility.setRequiredLevel(executionContext, approvedRequiredLevel, "dfa_eventid");
        dfa_Utility.setRequiredLevel(executionContext, approvedRequiredLevel, "dfa_confirmedcustomerid");
        dfa_Utility.setRequiredLevel(executionContext, decidedByRequiredLevel, "dfa_decidedbyid");
        dfa_Utility.setRequiredLevel(executionContext, decidedByRequiredLevel, "dfa_decisiondate");
        //project team update
        //dfa_Utility.setRequiredLevel(executionContext, approvedRequiredLevel, "dfa_areacommunityid");

        var showFarmOwnerSection = false;
        var showSmaillBusinessSection = false;
        var showForFarmOwnerAndSmallBusinessSection = false;
        var showCharitableSection = false;
        var showResidentialTenantSection = false;

        var showSmallBusinessTypeSection = true;
        var showFarmTypeSection = false;

        // Other required field by Applicant Type
        switch (applicantTypeValue) {
            case 222710000: // Charitable Organization
                showCharitableSection = true;
                break;
            case 222710006: // Incorporated
                break;
            case 222710001: // Farm Owner
                showFarmOwnerSection = true;
                showForFarmOwnerAndSmallBusinessSection = true;
                showFarmTypeSection = true;
                showSmallBusinessTypeSection = false;
                break;
            case 222710004: // Small Business Owner
                showSmaillBusinessSection = true;
                showForFarmOwnerAndSmallBusinessSection = true;
                break;
            case 222710002: // Home Owner
                break;
            case 222710003: // Residential Tenant
                showResidentialTenantSection = true;
                break;
            case 222710005: // Indigenous/Local Government Body
                dfa_Utility.setRequiredLevel(executionContext, approvedRequiredLevel, "dfa_casetitle");

                dfa_Utility.setRequiredLevel(executionContext, approvedRequiredLevel, "dfa_effectedregioncommunityid");
                break;
        }

        // Show Message if reviewStatusApproved is true
        if (reviewStatusApproved) {
            var dateReceivedValue = dfa_Utility.getAttributeValue(executionContext, "dfa_datereceived");
            var caseTitleValue = dfa_Utility.getAttributeValue(executionContext, "dfa_casetitle");
            var effectedCommunityValue = dfa_Utility.getAttributeValue(executionContext, "dfa_effectedregioncommunityid");
            var eventValue = dfa_Utility.getAttributeValue(executionContext, "dfa_eventid");
            var confirmedCustomerValue = dfa_Utility.getAttributeValue(executionContext, "dfa_confirmedcustomerid");
            //var areaCommunityValue = dfa_Utility.getAttributeValue(executionContext, "dfa_areacommunityid");

            //var commonRequiredFieldHaveData = dateReceivedValue != null && eventValue != null && confirmedCustomerValue != null && areaCommunityValue != null;
            var commonRequiredFieldHaveData = dateReceivedValue != null && eventValue != null && confirmedCustomerValue != null;

            var publicSectorRequiredFieldHaveData = caseTitleValue != null && effectedCommunityValue != null

            if (applicantTypeValue == 222710005) {
                commonRequiredFieldHaveData = commonRequiredFieldHaveData && publicSectorRequiredFieldHaveData;
            }

            if (!commonRequiredFieldHaveData) {
                dfa_Utility.showMessage("Please validate the Checklist in Review Tab and ensure all fields are provided and documents uploaded is set to YES");
                reviewStatusAttribute.setValue(222710001); // In-Review
                DFA.Jscripts.AppApplication.ToggleLevelOfRequirementOnReviewStatus(executionContext);
            }
        }

        dfa_Utility.showHide(executionContext, showFarmOwnerSection, "tab_general:section_farmownerapplicant");
        dfa_Utility.showHide(executionContext, showForFarmOwnerAndSmallBusinessSection, "tab_general:section_smallbiz_farmowner");
        dfa_Utility.showHide(executionContext, showSmaillBusinessSection, "tab_general:section_smallbusiness");
        dfa_Utility.showHide(executionContext, showCharitableSection, "tab_general:section_charitableOrganization");
        dfa_Utility.showHide(executionContext, showResidentialTenantSection, "tab_general:section_propertyinformation");

        dfa_Utility.showHide(executionContext, showSmallBusinessTypeSection, "tab_general:GeneralOther");
        dfa_Utility.showHide(executionContext, showFarmTypeSection, "tab_general:section_farmtype");


    },
    SwitchFormByInsuranceCoverage: function (executionContext) {
        var formContext = executionContext.getFormContext();
        var hasInsuranceAttributeName = "dfa_doyouhaveinsurancecoverage2";
        var portalFormName = "Application (Portal)";
        var portalFormNameSmallBusiness = "Application (Portal Small Business)";


        var hasInsuranceAttribute = formContext.getAttribute(hasInsuranceAttributeName);
        if (hasInsuranceAttribute == null || hasInsuranceAttribute.getValue() == null) {
            return false; // Continue with existing logic
        }

        var applicantTypeAttributeName = "dfa_applicanttype";
        var applicantTypeAttribute = formContext.getAttribute(applicantTypeAttributeName);
        var applicantTypeValue = applicantTypeAttribute.getValue();

        if (applicantTypeValue == 222710000 || applicantTypeValue == 222710001 || applicantTypeValue == 222710004) {
            dfa_Utility.navigateToFormByName(executionContext, portalFormNameSmallBusiness);
        }
        else {
            dfa_Utility.navigateToFormByName(executionContext, portalFormName);
        }

        // switch form
        //dfa_Utility.navigateToFormByName(executionContext, portalFormName);
        formContext.ui.tabs.get("tab_documentlocations").setVisible(false); // hide chef document location tab
        return true;
    }

}
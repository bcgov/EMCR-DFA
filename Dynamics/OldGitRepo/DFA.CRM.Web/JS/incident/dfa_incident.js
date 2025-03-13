// JavaScript source code

if (typeof DFA === "undefined") {
    var DFA = {};
}

if (typeof DFA.Jscripts === "undefined") {
    DFA.Jscripts = {};
}

DFA.Jscripts.Case = {
    incident_ExecutionContext: null,
    OnLoad: function (executionContext) {
        this.incident_ExecutionContext = executionContext;
        var isUCI = Xrm.Internal.isUci();
        if (!isUCI) {
            return; // Don't run anything else if not UCI.
        }

        var formContext = executionContext.getFormContext();
        var formType = formContext.ui.getFormType();
        var formName = "DFA - Case Main (Private)";
        var currentForm = formContext.ui.formSelector.getCurrentItem();
        if (currentForm != null) {
            formName = currentForm.getLabel();
        }

        var showAdminFields = dfa_Utility.checkCurrentUserRole("System Administrator");
        dfa_Utility.showHide(executionContext, showAdminFields, "dfa_togglecontactinfopopulation");

        dfa_Utility.enableDisable(executionContext, true, "dfa_balanceowing,dfa_totalpaid");
        DFA.Jscripts.Case.CloseFormIfNoProperSecurityRole(executionContext);
        switch (formName) {
            case "DFA - Case Main (Private)":
                DFA.Jscripts.Case.CaseHideShowForPrivateForm(executionContext);
                DFA.Jscripts.Case.CaseHideShowForAppeal(executionContext);
                //Filter Applicant Type
                formContext.getControl("dfa_applicanttype").removeOption(222710005); //Gov't option
                DFA.Jscripts.Case.PrivateEventClaimViewToggle(executionContext);

                //Added by Forrest on Aug23,2022
                DFA.Jscripts.Case.CaseHideShowForApplicantType(executionContext);

                //Added by Forrest on Aug24,2022 for EMBCDFA-200: Filter Cause of Damage/Loss
                formContext.getControl("dfa_causeofdamageloss").removeOption(222710004);

                //Added by Forrest on Aug30,2022 for EMBCDFA-210.
                DFA.Jscripts.Case.CaseSetupDocChecklistForApplicantType(executionContext);
                formContext.data.process.addOnStageChange(function () {
                    DFA.Jscripts.Case.CaseSetupDocChecklistForApplicantType(executionContext);
                });
                
                //Add by Forrest on Sep16, 2022 for EMBCDFA-203
                DFA.Jscripts.Case.CasePopulatePrimaryContact(executionContext);

                break;

            case "DFA - Case Main (Local Gov)":

                //Added by Forrest on Aug24,2022 for EMBCDFA-200: Filter Cause of Damage/Loss
                formContext.getControl("dfa_causeofdamageloss").removeOption(222710002);
                // EMBCDFA-238 - Ensure Case Title is visible and is required.
                dfa_Utility.showHide(executionContext, true, "title");
                dfa_Utility.setRequiredLevel(executionContext, "required", "title");

                //Add by Forrest on Sep29, 2022 for Gov Email Template population
                DFA.Jscripts.Case.CasePopulatePrimaryContact(executionContext);

                break;
        }
    },

    PrivateEventClaimViewToggle: function (executionContext) {
        var formContext = executionContext.getFormContext();
        var formType = formContext.ui.getFormType();
        var formName = "DFA - Case Main (Private)";
        var currentForm = formContext.ui.formSelector.getCurrentItem();
        if (currentForm != null) {
            formName = currentForm.getLabel();
        }
        // Private Forms have Claim Summary
        if (formType != 1 && formName == "DFA - Case Main (Private)") {
            var eventAttribute = formContext.getAttribute("dfa_eventid");
            if (eventAttribute != null && eventAttribute.getValue() != null) {
                var eventId = eventAttribute.getValue()[0].id.replace('{', '').replace('}', ''); // Event is a required field
                var eventOptions = "?$select=dfa_eventscope";
                Xrm.WebApi.retrieveRecord("dfa_event", eventId, eventOptions).then(
                    function success(data) {
                        var eventScope = data.dfa_eventscope;
                        DFA.Jscripts.Case.ChangeClaimsSubgridView(executionContext, eventScope);
                    },
                    function error(err) {
                        console.log(err.message);
                    }
                );
            }

        }
    },

    ChangeClaimsSubgridView: function (executionContext, eventScope) {
        "use strict";
        var formContext = executionContext.getFormContext();
        var viewSelector = formContext.getControl("subgrid_claimsummary").getViewSelector();
        var currentView = viewSelector.getCurrentView();
        var entityTypeNumber = currentView.entityType; // This may be different depends on environment.
        // Default Federal View
        var viewId = "94050E2F-821A-ED11-B834-00505683FBF4";
        var viewName = "Active Claim Summaries Federal Subgrid";
        var isProvincial = eventScope != 222710001;
        if (isProvincial) { // If Not Federal
            viewId = "FEBE5346-F21C-ED11-B834-00505683FBF4";
            viewName = "Active Claim Summaries Provincial Subgrid";
        }
        var newView = {
            entityType: entityTypeNumber,
            id: viewId,
            name: viewName
        };
        viewSelector.setCurrentView(newView);
        dfa_Utility.showHide(executionContext, !isProvincial, "dfa_federalclaimamounttotal");
    },

    CaseHideShowForPrivateForm: function (executionContext) {
        var formContext = executionContext.getFormContext();
        var Origin = formContext.getAttribute("caseorigincode").getValue();

        var showTab = (Origin != 222710000);
        dfa_Utility.showHide(executionContext, showTab, "occupants");
        dfa_Utility.showHide(executionContext, showTab, "CleanUpLogs");
        dfa_Utility.showHide(executionContext, showTab, "DamagedItems");

        //Added by Forrest on Aug 23, 2022 for EMBCDFA-156.
        //Purpose: show / hide signature section based on if Origin from WEB
        showTab = (Origin == 3);
        dfa_Utility.showHide(executionContext, showTab, "tab_general:Signatures_section");
    },

    CaseHideShowForAppeal: function (executionContext) {
        var statusCode = dfa_Utility.getAttributeValue(executionContext, "statuscode"); // Appeal is 2
        var formContext = executionContext.getFormContext();
        var entityId = formContext.data.entity.getId().replace('{', '').replace('}', '');
        var currentBPFStatus = formContext.data.process.getStatus();
        if (statusCode == 2 && currentBPFStatus == "aborted") {
            formContext.data.process.setStatus("active");
        }
        var options = "?$filter=_dfa_caseid_value eq " + entityId;
        var isBeingAppealed = (statusCode == 2);
        Xrm.WebApi.retrieveMultipleRecords("dfa_appeal", options).then(
            function success(results) {
                var rowCount = results.entities.length;
                var showAppealTab = isBeingAppealed || rowCount > 0;
                dfa_Utility.showHide(executionContext, showAppealTab, "tab_appealsummary");
                dfa_Utility.RefreshTimeLine(executionContext, "Timeline");
            },
            function failure(err) {
                var debug = err;
            }
        );
    },

    CalculateBalanceOwing: function (executionContext, delay) {
        var formContext = this.incident_ExecutionContext.getFormContext();
        var recordId = formContext.data.entity.getId().replace('{', '').replace('}', '');

        var advanceChequeValue = 0;
        var firstClaimChequeValue = 0;
        var additionalClaimChequeValue = 0;
        var appealChequeValue = 0;
        var oneTimeGrantChequeValue = 0;
        var totalPayableValue80 = 0;
        var applicantTypeValue = dfa_Utility.getAttributeValue(this.incident_ExecutionContext, "dfa_applicanttype");
        if (applicantTypeValue != 222710005) { // Not Government
            totalPayableValue80 = dfa_Utility.getAttributeValue(this.incident_ExecutionContext, "dfa_claimstotalpayable80");
        }

        var chequeOptions = "?$filter=_dfa_caseid_value eq " + recordId + " and statecode eq 0";
        if (delay == null || delay == NaN) {
            delay = 2000;
        }
        setTimeout(function () {
            Xrm.WebApi.retrieveMultipleRecords("dfa_cheque", chequeOptions).then(
                function success(results) {
                    for (var j = 0; j < results.entities.length; j++) {
                        var data = results.entities[j];
                        var chequeType = data.dfa_chequetype;
                        var amount = data.dfa_amount;
                        if (amount == null || amount == NaN) {
                            amount = 0;
                        }
                        switch (chequeType) {
                            case 222710000: // Advance Cheque
                                advanceChequeValue += amount;
                                break;
                            case 222710001: // First Claim Cheque
                                firstClaimChequeValue += amount;
                                break;
                            case 222710002: // Additional Claim Cheque
                                additionalClaimChequeValue += amount;
                                break;
                            case 222710003: // Appeal Cheque
                                appealChequeValue += amount;
                                break;
                            case 222710004: // One Time Only Grant Cheque
                                oneTimeGrantChequeValue += amount;
                                break;
                        }
                    }
                    if (advanceChequeValue == null) {
                        advanceChequeValue = 0;
                    }
                    if (firstClaimChequeValue == null) {
                        firstClaimChequeValue = 0;
                    }
                    if (additionalClaimChequeValue == null) {
                        additionalClaimChequeValue = 0;
                    }
                    if (appealChequeValue == null) {
                        appealChequeValue = 0;
                    }
                    if (oneTimeGrantChequeValue == null) {
                        oneTimeGrantChequeValue = 0;
                    }
                    if (totalPayableValue80 == null) {
                        totalPayableValue80 = 0;
                    }
                    var subtotal = advanceChequeValue + firstClaimChequeValue + additionalClaimChequeValue +
                        appealChequeValue + oneTimeGrantChequeValue;
                    var totalPaidAttribute = formContext.getAttribute("dfa_totalpaid");
                    if (totalPaidAttribute != null) {
                        totalPaidAttribute.setValue(subtotal);
                    }
                    var balanceOwing = totalPayableValue80 - subtotal;
                    var balanceOwingAttribute = formContext.getAttribute("dfa_balanceowing");
                    if (balanceOwingAttribute != null) {
                        balanceOwingAttribute.setValue(balanceOwing);
                    }
                },
                function failure(err) {
                    var debug = err;
                }
            );
        }, delay);
    },

    // EMBCDFA-218 Close Form If No Proper Security Roles
    CloseFormIfNoProperSecurityRole: function (executionContext) {
        var formContext = this.incident_ExecutionContext.getFormContext();
        formItem = formContext.ui.formSelector.getCurrentItem();
        var publicFormName = "DFA - Case Main (Local Gov)";
        if (formItem != null && formItem.getLabel().includes("DataFix")) {
            return; // Exit and don't run any 
        }
        var applicantType = formContext.getAttribute("dfa_applicanttype").getValue();
        var formType = formContext.ui.getFormType();
        if (formType == 1) {
            return
        }
        var isPrivateRecord = (applicantType != 222710005);
        var hasAuthority = false;
        if (isPrivateRecord) {
            hasAuthority = dfa_Utility.checkCurrentUserRole("EMBC-DFA Case Worker (Private)") ||
                dfa_Utility.checkCurrentUserRole("EMBC-DFA Field Manager") ||
                dfa_Utility.checkCurrentUserRole("EMBC-DFA Manager (Private)") ||
                dfa_Utility.checkCurrentUserRole("EMBC-DFA DataFix");
        }
        else {
            hasAuthority = dfa_Utility.checkCurrentUserRole("EMBC-DFA Manager (Local Gov)") ||
                dfa_Utility.checkCurrentUserRole("EMBC-DFA Field Manager") ||
                dfa_Utility.checkCurrentUserRole("EMBC-DFA Case Worker (Local Gov)") ||
                dfa_Utility.checkCurrentUserRole("EMBC-DFA DataFix");
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
        if (currentForm != publicFormName) {
            isPrivateForm = true;
        }

        if ((isPrivateForm && !isPrivateRecord) ||
            (!isPrivateForm && isPrivateRecord)) {
            var message = "You are using the Public App currently trying to open a private sector record.  Please change to Private App and try again."
            if (isPrivateForm) {
                message = "You are using the Private App currently trying to open a public sector record.  Please change to Public App and try again."
            }
            var alertStrings = { confirmButtonLabel: "OK", text: message, title: "" };
            var alertOptions = { height: 120, width: 260 };
            Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
                function successCallBack() {
                    formContext.ui.close();
                });
        }
    },
   
    // EMBCDFA-223 Primvate Sector Claim Summary Calculations
    UpdateClaimSubtotals: function (gridContext, delay) {
        var isUCI = Xrm.Internal.isUci();
        if (!isUCI) {
            return; // Don't run anything else if not UCI.
        }
        var formContext = this.incident_ExecutionContext.getFormContext();
        var federalClaimTotalAttribute = formContext.getAttribute("dfa_federalclaimamounttotal");
        var provincialClaimTotalAttribute = formContext.getAttribute("dfa_provincialeligibleamounttotal");
        var ineligibleTotalAttribute = formContext.getAttribute("dfa_ineligibleamounttotal");
        var totalPayableAttribute = formContext.getAttribute("dfa_claimstotalpayable");
        var less1000Attribute = formContext.getAttribute("dfa_less1000deductable");
        var totalPayable80Attribute = formContext.getAttribute("dfa_claimstotalpayable80");
        var subgridControl = formContext.getControl("subgrid_claimsummary");
        var recordId = formContext.data.entity.getId().replace('{', '').replace('}', '');

        if (delay == null || delay == NaN) {
            delay = 2000;
        }
        setTimeout(function () {
            // The issue is that the on Save is actually prior to Save.  So current change won't go in.  You need to wait for it anyway.

            var claimOptions = "?$select=dfa_federalclaim,dfa_ineligibleamount,dfa_provincialeligibleamount&$filter=_dfa_caseid_value eq " + recordId + " and statecode eq 0";
            Xrm.WebApi.retrieveMultipleRecords("dfa_claimsummary", claimOptions).then(
                function success(results) {
                    var provincialTotals = 0;
                    var ineligibleTotals = 0;
                    var federalTotals = 0;
                    for (var i = 0; i < results.entities.length; i++) {
                        var data = results.entities[i];
                        var provincialAmount = data.dfa_provincialeligibleamount;
                        if (provincialAmount == null || provincialAmount == NaN) {
                            provincialAmount = 0;
                        }
                        var ineligibleAmount = data.dfa_ineligibleamount;
                        if (ineligibleAmount == null || ineligibleAmount == NaN) {
                            ineligibleAmount = 0;
                        }
                        var federalAmount = data.dfa_federalclaim;
                        if (federalAmount == null || federalAmount == NaN) {
                            federalAmount = 0;
                        }
                        provincialTotals += provincialAmount;
                        ineligibleTotals += ineligibleAmount;
                        federalTotals += federalAmount;
                    }
                    var less1000 = federalTotals - 1000;
                    if (less1000 < 0) {
                        less1000 = 0;
                    }
                    var totalPayable = less1000;
                    var eightyPercent = totalPayable * 0.8;
                    if (federalClaimTotalAttribute != null) {
                        federalClaimTotalAttribute.setValue(federalTotals);
                    }
                    if (provincialClaimTotalAttribute != null) {
                        provincialClaimTotalAttribute.setValue(provincialTotals);
                    }
                    if (ineligibleTotalAttribute != null) {
                        ineligibleTotalAttribute.setValue(ineligibleTotals);
                    }
                    if (less1000Attribute != null) {
                        less1000Attribute.setValue(less1000);
                    }
                    if (totalPayable80Attribute != null) {
                        totalPayable80Attribute.setValue(eightyPercent);
                    }
                    if (totalPayableAttribute != null) {
                        totalPayableAttribute.setValue(totalPayable);
                    }
                    subgridControl.refresh();
                    DFA.Jscripts.Case.CalculateBalanceOwing(this.incident_ExecutionContext, 0);
                },
                function error(err) {
                    var debug = err;
                }
            );
        }, delay);

    },

    SetFieldManagerAssignmentByAssignedTo: function (executionContext) {
        var formContext = executionContext.getFormContext();
        var fieldManagerAssignmentAttribute = formContext.getAttribute("dfa_fieldmanagerassignment");
        if (fieldManagerAssignmentAttribute == null) {
            return;
            // No field to update
        }
        var assignedToLookupObject = dfa_Utility.getAttributeValue(executionContext, "dfa_assignedto");
        if (assignedToLookupObject == null) {
            return;
            // No source field to evaluate
        }
        var userID = assignedToLookupObject[0].id;
        var fieldManagerSecurityRoleName = "EMBC-DFA Field Manager";
        var hasRole = dfa_Utility.checkUserRole(fieldManagerSecurityRoleName, userID);
        fieldManagerAssignmentAttribute.setValue(hasRole);
        fieldManagerAssignmentAttribute.fireOnChange();
    },

    DisableClaimSummaryGridFields: function (executionContext) {
        var formContext = executionContext.getFormContext();
        formContext.getData().getEntity().attributes.forEach(function (attr) {

            if (attr.getName() === "dfa_displayorder" ||
                attr.getName() === "dfa_federalclaim") {
                attr.controls.forEach(function (c) {
                    c.setDisabled(true);
                });
            }
        });
    },

    //Added by Forrest on Aug 23, 2022.
    //For task: EMBCDFA-156.  
    //Purpose: Show/hide related sections based on applicant type
    CaseHideShowForApplicantType: function (executionContext) {
        var applicantType = dfa_Utility.getAttributeValue(executionContext, "dfa_applicanttype");

        //debugger;

        //If orgin is not web, hide and return
        if (dfa_Utility.getAttributeValue(executionContext, "caseorigincode") != 3) {
            dfa_Utility.showHide(executionContext, false, "tab_general:Small_Business_Application_section");
            dfa_Utility.showHide(executionContext, false, "tab_general:Farm_Owner_Applicant_section");
            dfa_Utility.showHide(executionContext, false, "tab_general:For_Small_Business_and_Farm_Owner_section");
            dfa_Utility.showHide(executionContext, false, "tab_general:tab_general_section_13");
            dfa_Utility.showHide(executionContext, false, "tab_general:For_Charitable_Organizations_section");
            return;
        }

        switch (applicantType) {
            //"Small Business Owner"
            case 222710003:
                //Set visibility of Business Application section
                dfa_Utility.showHide(executionContext, true, "tab_general:Small_Business_Application_section");
                dfa_Utility.showHide(executionContext, true, "tab_general:For_Small_Business_and_Farm_Owner_section");
                dfa_Utility.showHide(executionContext, true, "tab_general:tab_general_section_13");

                dfa_Utility.showHide(executionContext, false, "tab_general:Farm_Owner_Applicant_section");
                dfa_Utility.showHide(executionContext, false, "tab_general:For_Charitable_Organizations_section");
                break;

            //"Farm Owner"
            case 222710002:
                //Set visibility of Business Application section
                dfa_Utility.showHide(executionContext, true, "tab_general:Farm_Owner_Applicant_section");
                dfa_Utility.showHide(executionContext, true, "tab_general:For_Small_Business_and_Farm_Owner_section");
                dfa_Utility.showHide(executionContext, true, "tab_general:tab_general_section_13");

                dfa_Utility.showHide(executionContext, false, "tab_general:Small_Business_Application_section");
                dfa_Utility.showHide(executionContext, false, "tab_general:For_Charitable_Organizations_section");
                break;

            //"Charitable / Volunteer Organization"
            case 222710004:
                //Set visibility of Business Application section
                dfa_Utility.showHide(executionContext, true, "tab_general:For_Charitable_Organizations_section");
                dfa_Utility.showHide(executionContext, true, "tab_general:tab_general_section_13");

                dfa_Utility.showHide(executionContext, false, "tab_general:Small_Business_Application_section");
                dfa_Utility.showHide(executionContext, false, "tab_general:Farm_Owner_Applicant_section");
                dfa_Utility.showHide(executionContext, false, "tab_general:For_Small_Business_and_Farm_Owner_section");
                break;

            default:
                dfa_Utility.showHide(executionContext, false, "tab_general:Small_Business_Application_section");
                dfa_Utility.showHide(executionContext, false, "tab_general:Farm_Owner_Applicant_section");
                dfa_Utility.showHide(executionContext, false, "tab_general:For_Small_Business_and_Farm_Owner_section");
                dfa_Utility.showHide(executionContext, false, "tab_general:tab_general_section_13");
                dfa_Utility.showHide(executionContext, false, "tab_general:For_Charitable_Organizations_section");
        }
    },



    //Added by Forrest on Aug 30, 2022.
    //For task: EMBCDFA-210.
    //Purpose: Set up doc checklist based on APPLICANT TYPE.  
    //Repace existing BR: "DFA - Private - Check required documents based on applicant type - BR")
    CaseSetupDocChecklistForApplicantType: function (executionContext) {
        var formContext = executionContext.getFormContext();
        var curBPF = formContext.data.process.getActiveProcess();

        //debugger;

        //Return when BPF not equals "Private - Case Business Process".
        if ((curBPF != null) && (curBPF.getName() != "Private - Case Business Process")) {
            return;
        }

        var applicantType = dfa_Utility.getAttributeValue(executionContext, "dfa_applicanttype");

        //Get active stage name
        var activeStage = formContext.data.process.getActiveStage();
        var stagename = (activeStage != null) ? activeStage.getName() : "";


        switch (stagename) {
            case "Confirm Eligibility":
                DFA.Jscripts.Case.CaseSetupDocChecklistForConfirmEligibilityStage(executionContext, applicantType);
                break;

            case "Assess Damage":
                DFA.Jscripts.Case.CaseSetupDocChecklistForAssessDamageStage(executionContext, applicantType);
                break;

            case "Review Report":
                DFA.Jscripts.Case.CaseSetupChecklistForReviewReportStage(executionContext, applicantType);
                break;

            default:

                //Do nothing
                break;
        }
    },


    //Added by Forrest on Aug 31, 2022.
    //For task: EMBCDFA-210.
    //Purpose: Set up doc checklist for "Confirm Eligibility" stage.
    //Repace existing BR: "DFA - Private - Check required documents based on applicant type - BR")
    CaseSetupDocChecklistForConfirmEligibilityStage: function (executionContext, applicantType) {
        var formContext = executionContext.getFormContext();

        //All fields hide, all optional
        //field Application form (signed)
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_applicationformsigned");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_applicationformsigned");

        //field Insurance template completed by insurer / broker
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_insurancetemplatecompletedbyinsurerbroker");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_insurancetemplatecompletedbyinsurerbroker");

        //field Home Owner Grant eligibility confirmed
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_homeownergranteligibilityconfirmed");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_homeownergranteligibilityconfirmed");

        //field Land Title document or Mobile Home Registry info
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_landtitledocumentormobilehomeregistryinfo");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_landtitledocumentormobilehomeregistryinfo");

        //field BC Assessment Roll report
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_bcassessmentrollreport");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_bcassessmentrollreport");

        //field Covenant(s)
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_covenants");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_covenants");

        //field Supplementary proof of principal residency – ID;
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_supplementaryproofofprincipalresidencyid");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_supplementaryproofofprincipalresidencyid");

        //field Central securities register
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_centralsecuritiesregister");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_centralsecuritiesregister");

        //field Rental or Lease agreement
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_rentalorleaseagreement");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_rentalorleaseagreement");

        //field Financial statements
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_financialstatements");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_financialstatements");

        //field BC Societies Registration
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_bcsocietiesregistration");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_bcsocietiesregistration");

        //field List of Directors
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_listofdirectors");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_listofdirectors");



        //"Home Owner"
        if (applicantType == 222710000) {

            //Show & Require: field Application form (signed)
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_applicationformsigned");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_applicationformsigned");

            //Show & Require: field Insurance template completed by insurer / broker
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_insurancetemplatecompletedbyinsurerbroker");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_insurancetemplatecompletedbyinsurerbroker");

            //Show & Require: field Home Owner Grant eligibility confirmed
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_homeownergranteligibilityconfirmed");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_homeownergranteligibilityconfirmed");

            //Show & Require: field Land Title document or Mobile Home Registry info
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_landtitledocumentormobilehomeregistryinfo");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_landtitledocumentormobilehomeregistryinfo");

            //Show & Require: field BC Assessment Roll report
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_bcassessmentrollreport");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_bcassessmentrollreport");

            //Show & Require: field Covenant(s)
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_covenants");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_covenants");

            //Show & Optional: field Supplementary proof of principal residency – ID;
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_supplementaryproofofprincipalresidencyid");
            dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_supplementaryproofofprincipalresidencyid");

            //Show & Optional: field Central securities register
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_centralsecuritiesregister");
            dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_centralsecuritiesregister");
        }


        //"Small Business Owner" or "Farm Owner"
        else if ((applicantType == 222710003) || (applicantType == 222710002)) {
            //Show & Require: field Application form(signed)
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_applicationformsigned");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_applicationformsigned");

            //Show & Require: field Insurance template completed by insurer / broker
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_insurancetemplatecompletedbyinsurerbroker");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_insurancetemplatecompletedbyinsurerbroker");

            //Show & Require: field Rental or Lease agreement
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_rentalorleaseagreement");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_rentalorleaseagreement");

            //Show & Require: field Financial statements
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_financialstatements");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_financialstatements");

            //Show & Require: field Covenant(s)
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_covenants");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_covenants");

            //Show & Optional: field Central securities register
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_centralsecuritiesregister");
            dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_centralsecuritiesregister");

            //Show & Optional: field BC Assessment Roll report
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_bcassessmentrollreport");
            dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_bcassessmentrollreport");

        }

        //"Residential Tenant"
        else if (applicantType == 222710001) {
            //Show & Require: field Application form(signed)
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_applicationformsigned");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_applicationformsigned");

            //Show & Require: field Insurance template completed by insurer / broker
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_insurancetemplatecompletedbyinsurerbroker");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_insurancetemplatecompletedbyinsurerbroker");

            //Show & Require: field Rental or Lease agreement
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_rentalorleaseagreement");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_rentalorleaseagreement");

            //Show & Require: field Supplementary proof of principal residency – ID;
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_supplementaryproofofprincipalresidencyid");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_supplementaryproofofprincipalresidencyid");


        }

        //"Charitable / Volunteer Organization"
        else if (applicantType == 222710004) {
            //Show & Require: field Application form(signed)
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_applicationformsigned");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_applicationformsigned");

            //Show & Require: field Insurance template completed by insurer / broker
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_insurancetemplatecompletedbyinsurerbroker");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_insurancetemplatecompletedbyinsurerbroker");

            //Show & Require: field BC Societies Registration
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_bcsocietiesregistration");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_bcsocietiesregistration");

            //Show & Require: field List of Directors
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_listofdirectors");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_listofdirectors");

            //Show & Require: field Financial statements
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_financialstatements");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_financialstatements");

            //Show & Require: field Rental or Lease agreement
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_rentalorleaseagreement");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_rentalorleaseagreement");

            //Show & Require: field Covenant(s)
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_covenants");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_covenants");
        }
    },

    //Added by Forrest on Aug 31, 2022.
    //For task: EMBCDFA-212.
    //Purpose: Set up doc checklist for "Assess Damage" stage.
    CaseSetupDocChecklistForAssessDamageStage: function (executionContext, applicantType) {
        var formContext = executionContext.getFormContext();

        //All fields hide, all optional
        //field Claim summary sheet
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_claimsummarysheet");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_claimsummarysheet");

        //field Application for DFA
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_applicationfordfa");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_applicationfordfa");

        //field Evaluator’s notes
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_evaluatorsnotes");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_evaluatorsnotes");

        //field Scope and calculations sheet
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_scopeandcalculationssheet");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_scopeandcalculationssheet");

        //field Content calculations sheet
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_contentcalculationssheet");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_contentcalculationssheet");

        //field Diagram
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_diagram");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_diagram");

        //field Receipts or invoices paid by applicant
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_receiptsorinvoicespaidbyapplicant");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_receiptsorinvoicespaidbyapplicant");

        //field Others
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_others");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_others");

        //field Photos
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_photos");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_photos");

        //field Insurance Letter
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_insuranceletter");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_insuranceletter");

        //field Home Owner Grant eligibility confirmed
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_homeownergranteligibilityconfirmed_1");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_homeownergranteligibilityconfirmed_1");

        //field Land Title document or Mobile Home Registry info
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_landtitledocumentormobilehomeregistryinfo_1");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_landtitledocumentormobilehomeregistryinfo_1");

        //field Covenant(s)
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_covenants_1");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_covenants_1");

        //field BCAA Property Assessment - Assessed Value Property Class
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_bcaapropertyassessment");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_bcaapropertyassessment");

        //field Rental or Lease agreement
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_rentalorleaseagreement_1");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_rentalorleaseagreement_1");

        //field Financial statements
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_financialstatements_1");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_financialstatements_1");

        //field Income Tax Statement
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_mostrecentlyfiledfinancialstatements");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_mostrecentlyfiledfinancialstatements");

        //field Central securities register
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_centralsecuritiesregister_1");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_centralsecuritiesregister_1");

        //field List of Directors
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_listofdirectors_1");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_listofdirectors_1");

        //field BC Societies Registration
        dfa_Utility.showHide(executionContext, false, "header_process_dfa_bcsocietiesregistration_1");
        dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_bcsocietiesregistration_1");


        //"Home Owner"
        if (applicantType == 222710000) {
            //Show & Require: Claim summary sheet
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_claimsummarysheet");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_claimsummarysheet");

            //Show & Require: Application for DFA
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_applicationfordfa");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_applicationfordfa");

            //Show & Require: Evaluator’s notes
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_evaluatorsnotes");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_evaluatorsnotes");

            //Show & Require: Scope and calculations sheet
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_scopeandcalculationssheet");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_scopeandcalculationssheet");

            //Show & Require: Content calculations sheet
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_contentcalculationssheet");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_contentcalculationssheet");

            //Show & Require: Diagram
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_diagram");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_diagram");

            //Show & Require: Receipts or invoices paid by applicant
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_receiptsorinvoicespaidbyapplicant");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_receiptsorinvoicespaidbyapplicant");

            //Show & Require: Others
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_others");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_others");

            //Show & Require: Photos
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_photos");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_photos");

            //Show & Require: Insurance Letter
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_insuranceletter");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_insuranceletter");

            //Show & Require: Home Owner Grant eligibility confirmed
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_homeownergranteligibilityconfirmed_1");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_homeownergranteligibilityconfirmed_1");

            //Show & Require: Land Title document or Mobile Home Registry info
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_landtitledocumentormobilehomeregistryinfo_1");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_landtitledocumentormobilehomeregistryinfo_1");

            //Show & Require: Covenant(s)
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_covenants_1");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_covenants_1");

            //Show & Require: BCAA Property Assessment - Assessed Value Property Class
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_bcaapropertyassessment");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_bcaapropertyassessment");
        }

        //"Residential Tenant"
        else if (applicantType == 222710001) {
            //Show & Require: Claim summary sheet
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_claimsummarysheet");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_claimsummarysheet");

            //Show & Require: Application for DFA
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_applicationfordfa");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_applicationfordfa");

            //Show & Require: Evaluator’s notes
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_evaluatorsnotes");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_evaluatorsnotes");

            //Show & Require: Scope and calculations sheet
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_scopeandcalculationssheet");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_scopeandcalculationssheet");

            //Show & Require: Content calculations sheet
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_contentcalculationssheet");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_contentcalculationssheet");

            //Show & Require: Diagram
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_diagram");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_diagram");

            //Show & Require: Receipts or invoices paid by applicant
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_receiptsorinvoicespaidbyapplicant");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_receiptsorinvoicespaidbyapplicant");

            //Show & Require: Others
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_others");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_others");

            //Show & Require: Photos
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_photos");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_photos");

            //Show & Require: Insurance Letter
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_insuranceletter");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_insuranceletter");

            //field Rental or Lease agreement
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_rentalorleaseagreement_1");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_rentalorleaseagreement_1");
        }

        //"Small Business Owner" or "Farm Owner"
        else if ((applicantType == 222710003) || (applicantType == 222710002)) {
            //Show & Require: Claim summary sheet
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_claimsummarysheet");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_claimsummarysheet");

            //Show & Require: Application for DFA
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_applicationfordfa");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_applicationfordfa");

            //Show & Require: Evaluator’s notes
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_evaluatorsnotes");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_evaluatorsnotes");

            //Show & Require: Scope and calculations sheet
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_scopeandcalculationssheet");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_scopeandcalculationssheet");

            //Show & Require: Content calculations sheet
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_contentcalculationssheet");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_contentcalculationssheet");

            //Show & Require: Diagram
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_diagram");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_diagram");

            //Show & Require: Receipts or invoices paid by applicant
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_receiptsorinvoicespaidbyapplicant");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_receiptsorinvoicespaidbyapplicant");

            //Show & Require: Others
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_others");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_others");

            //Show & Require: Financial statements
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_financialstatements_1");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_financialstatements_1");

            //Show & Require: Income Tax Statement
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_mostrecentlyfiledfinancialstatements");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_mostrecentlyfiledfinancialstatements");

            //Show & Require: field Central securities register
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_centralsecuritiesregister_1");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_centralsecuritiesregister_1");

            //Show & Require: Photos
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_photos");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_photos");

            //Show & Require: Insurance Letter
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_insuranceletter");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_insuranceletter");

            //field Rental or Lease agreement
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_rentalorleaseagreement_1");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_rentalorleaseagreement_1");

            //Show & Require: Land Title document or Mobile Home Registry info
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_landtitledocumentormobilehomeregistryinfo_1");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_landtitledocumentormobilehomeregistryinfo_1");

            //Show & Require: Covenant(s)
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_covenants_1");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_covenants_1");

        }

        //"Charitable / Volunteer Organization"
        else if (applicantType == 222710004) {
            //Show & Require: Claim summary sheet
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_claimsummarysheet");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_claimsummarysheet");

            //Show & Require: Application for DFA
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_applicationfordfa");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_applicationfordfa");

            //Show & Require: Evaluator’s notes
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_evaluatorsnotes");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_evaluatorsnotes");

            //Show & Require: Scope and calculations sheet
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_scopeandcalculationssheet");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_scopeandcalculationssheet");

            //Show & Require: Content calculations sheet
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_contentcalculationssheet");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_contentcalculationssheet");

            //Show & Require: Diagram
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_diagram");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_diagram");

            //Show & Require: Receipts or invoices paid by applicant
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_receiptsorinvoicespaidbyapplicant");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_receiptsorinvoicespaidbyapplicant");

            //Show & Require: Others
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_others");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_others");

            //Show & Require: Financial statements
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_financialstatements_1");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_financialstatements_1");

            //Show & Require: List of Directors
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_listofdirectors_1");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_listofdirectors_1");

            //Show & Require: BC Societies Registration
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_bcsocietiesregistration_1");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_bcsocietiesregistration_1");

            //Show & Require: Photos
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_photos");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_photos");

            //Show & Require: Insurance Letter
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_insuranceletter");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_insuranceletter");

            //field Rental or Lease agreement
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_rentalorleaseagreement_1");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_rentalorleaseagreement_1");

            //Show & Require: Land Title document or Mobile Home Registry info
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_landtitledocumentormobilehomeregistryinfo_1");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_landtitledocumentormobilehomeregistryinfo_1");

            //Show & Require: Covenant(s)
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_covenants_1");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_covenants_1");

            //Show & Require: BCAA Property Assessment - Assessed Value Property Class
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_bcaapropertyassessment");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_bcaapropertyassessment");

        }
    },

    //Added by Forrest on Sep 9, 2022.
    //For task: EMBCDFA-254.
    //Purpose: Set up checklist for "Review Report" stage.
    CaseSetupChecklistForReviewReportStage: function (executionContext, applicantType) {
        var formContext = executionContext.getFormContext();

        //"Residential Tenant"
        if (applicantType == 222710001) {
            //Hide & Optional: Compliance
            dfa_Utility.showHide(executionContext, false, "header_process_dfa_compliance");
            dfa_Utility.setBPFRequiredLevel(executionContext, "none", "header_process_dfa_compliance");
        }
        else {
            //Show & Required: Compliance
            dfa_Utility.showHide(executionContext, true, "header_process_dfa_compliance");
            dfa_Utility.setBPFRequiredLevel(executionContext, "required", "header_process_dfa_compliance");
        }
    },

    //Added by Forrest on Sep 6, 2022.
    //For task: EMBCDFA-227.
    //Purpose: Save Case Record
    CaseSaveForm: function (executionContext) {
        var formContext = executionContext.getFormContext();

        debugger;

        if (formContext.data.entity.getIsDirty()) {
            formContext.data.entity.save();
        }

    },


    //Added by Forrest on Sep 8, 2022.
    //For task: EMBCDFA-239
    //Purpose: Resolve Case through custom button
    CaseResolve: function (formContext) {
        //Create a Incident Resolution record to pair with this Case
        var caseId = formContext.data.entity.getId().replace('{', '').replace('}', '');

        //Get case title value
        var caseTitleAttr = formContext.getAttribute("title");
        var caseTitleValue = null;
        if (dfa_Utility.isValid(caseTitleAttr)) {
            caseTitleValue = caseTitleAttr.getValue();
        }

        //Get case eligibility
        var eligibilityStatusAttr = formContext.getAttribute("dfa_eligibilitystatus");
        var eligibilityStatusValue = null;
        if (dfa_Utility.isValid(eligibilityStatusAttr)) {
            eligibilityStatusValue = eligibilityStatusAttr.getValue();
        }
        var eligibilityStatusLabel = null;
        switch (eligibilityStatusValue) {
            case 222710000:
                eligibilityStatusLabel = "Eligible";
                break;

            case 222710001:
                eligibilityStatusLabel = "Ineligible";
                break;

            case 222710003:
                eligibilityStatusLabel = "Withdrawn";
                break;
        }

        var incidentresolution = {
            "subject": "Case Closed - " + caseTitleValue,
            "incidentid@odata.bind": "/incidents(" + caseId + ")",   //Id of incident
            "@odata.type": "Microsoft.Dynamics.CRM.incidentresolution",
            "timespent": 0,
            "description": eligibilityStatusLabel
        };

        var newEntityId = "";
        Xrm.WebApi.online.createRecord("incidentresolution", incidentresolution).then(
            function success(result) {
                newEntityId = result.id;
                var parameters = {};

                incidentresolution.activityid = newEntityId;
                parameters.IncidentResolution = incidentresolution;
                parameters.Status = 5;//Closed

                var closeIncidentRequest = {
                    IncidentResolution: parameters.IncidentResolution,
                    Status: parameters.Status,

                    getMetadata: function () {
                        return {
                            boundParameter: null,
                            parameterTypes: {
                                "IncidentResolution": {
                                    "typeName": "mscrm.incidentresolution",
                                    "structuralProperty": 5
                                },
                                "Status": {
                                    "typeName": "Edm.Int32",
                                    "structuralProperty": 1
                                }
                            },
                            operationType: 0,
                            operationName: "CloseIncident"
                        };
                    }
                };

                Xrm.WebApi.online.execute(closeIncidentRequest).then(
                    function success(result) {
                        if (result.ok) {
                            formContext.data.refresh(false);
                        }
                    },
                    function (error) {
                        Xrm.Utility.alertDialog(error.message);
                    }
                );

            },
            function (error) {
                Xrm.Utility.alertDialog(error.message);
            }
        );
    },

    hasAppeal: function () {
        // Use from Ribbon Button to check if there is already an Appeal
        var formContext = this.project_ExecutionContext.getFormContext();
        var entityId = formContext.data.entity.getId().replace('{', '').replace('}', '');
        var options = "?$filter=_dfa_caseid_value eq " + entityId;
        var results = dfa_Utility.retrieveMultipleCustom("dfa_appeal", options);
        return results.length > 0;
    },


    //Added by Forrest on Sep 16, 2022.
    //For task: EMBCDFA-203
    //Purpose: Populate Primary Contact based on Applicant(Account)
    CasePopulatePrimaryContact: function (executionContext) 
    {
        var formContext = executionContext.getFormContext();
        //debugger;

        var customer = dfa_Utility.getAttributeValue(executionContext, "customerid");
        if(customer == null)
        {
            return;
        }

        //Lookup is an array. Need to get first object.
        var customerType = customer[0].entityType;

        //If Applianct is account, retrieve the primary contact of this account, then populate the primary contact field in the case form
        if (customerType == "account")
        {
            Xrm.WebApi.retrieveRecord("account", customer[0].id.replace("{","").replace("}",""), "?$select=_primarycontactid_value").then
            (
               function success(result) 
               {
                   //debugger;
                   
                   primaryContactId = result._primarycontactid_value;                   
       
                   //if Acount Primary Contact is null, clear primary contact field.
                   if(primaryContactId == null)
                   {
                       dfa_Utility.clearFieldData(executionContext, "dfa_primarycontactid");     
                       dfa_Utility.clearFieldData(executionContext, "emailaddress"); 
                   }
                   //if Account Primary Contact is not null, retrieve Account primary contact details, then set primary contact field of the form.            
                   else
                   {
                       Xrm.WebApi.retrieveRecord("contact", primaryContactId, "?$select=fullname, emailaddress1").then
                       (
                          function success(result) 
                          {
                                //debugger;   

                                var object = dfa_Utility.generateLookupObject("contact", primaryContactId, result.fullname);
                                dfa_Utility.setLookupValue(executionContext, "dfa_primarycontactid", object);
                                dfa_Utility.setFieldValue(executionContext, "emailaddress", result.emailaddress1);
                          },
                          function (error) 
                          {
                                dfa_Utility.clearFieldData(executionContext, "dfa_primarycontactid");     
                                dfa_Utility.clearFieldData(executionContext, "emailaddress"); 
                                dfa_Utility.showMessage(error.message);
                          }    
                       );
                   }                                                    
               },
               function (error) 
               {
                    dfa_Utility.clearFieldData(executionContext, "dfa_primarycontactid");     
                    dfa_Utility.clearFieldData(executionContext, "emailaddress"); 
                    dfa_Utility.showMessage(error.message);
               }  
            )  
        }
        //If Applianct is contact, retrieve the email of this contact, then populate the emailaddress field in the case form
        else if (customerType = "contact")
        {
            dfa_Utility.clearFieldData(executionContext, "dfa_primarycontactid");

            Xrm.WebApi.retrieveRecord("contact", customer[0].id.replace("{","").replace("}",""), "?$select=emailaddress1").then
            (
                function success(result) 
                {
                    //debugger;  
                    dfa_Utility.setFieldValue(executionContext, "emailaddress", result.emailaddress1);
                },
                function (error) 
                {
                    dfa_Utility.clearFieldData(executionContext, "emailaddress");
                    dfa_Utility.showMessage(error.message);
                }    
            );           
        }
        else
        {
            dfa_Utility.clearFieldData(executionContext, "dfa_primarycontactid");     
            dfa_Utility.clearFieldData(executionContext, "emailaddress"); 

            //Show Error Message
            dfa_Utility.showMessage("Wrong Cusotmer Type: " + customerType);
        }        
    }
};
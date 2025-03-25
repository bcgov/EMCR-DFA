// JavaScript source code
// Create a unique namespace for this library to prevent name collisions
if (typeof (IsoFairlens) === "undefined") {
    IsoFairlens = {
        namespace: true
    };
}
if (typeof (IsoFairlens.iso_case) === "undefined") {
    IsoFairlens.iso_case = {
        namespace: true,
        iso_sqr_curr_onsite_review_date: null
    };
}
IsoFairlens.iso_case = {

    FormOnLoad: function (context) {
        var formContext = context.getFormContext();
        var formMode = formContext.ui.getFormType();

        var caseType = formContext.getAttribute("iso_case_type");
        if (caseType != null && caseType.getValue() == null) {
            caseType.setValue(863050000);
        } else {
            if (formContext.getControl("iso_case_type") != null) {
                formContext.getControl("iso_case_type").setDisabled(true);
            }
            if (formContext.getControl("header_iso_case_type") != null) {
                formContext.getControl("header_iso_case_type").setDisabled(true);
            }
            if (caseType.getValue() === 863050001) {
                if (formContext.getControl("header_process_iso_early_resolution") != null) {
                    formContext.getControl("header_process_iso_early_resolution").setDisabled(true);
                }
            }
        }

        if (formContext.getControl("header_process_iso_case_type") != null) {
            formContext.getControl("header_process_iso_case_type").setVisible(false);
        }
        if (formContext.getControl("header_process_iso_incident_date") != null) {
            formContext.getControl("header_process_iso_incident_date").setVisible(false);

        }
        if (formContext.getControl("header_process_iso_cir_type") != null) {
            formContext.getControl("header_process_iso_cir_type").setVisible(false);

        }
        if (formContext.getControl("header_process_iso_chair") != null) {
            formContext.getControl("header_process_iso_chair").setVisible(false);

        }
        if (formContext.getControl("header_process_iso_report_due_date") != null) {
            formContext.getControl("header_process_iso_report_due_date").setVisible(false);

        }
        if (formContext.getControl("header_process_iso_onsite_review_date") != null) {
            formContext.getControl("header_process_iso_onsite_review_date").setVisible(false);

        }
        if (formContext.getControl("header_process_iso_previous_sqr_case") != null) {
            formContext.getControl("header_process_iso_previous_sqr_case").setVisible(false);

        }
        if (formContext.getControl("header_process_iso_summer_flex") != null) {
            formContext.getControl("header_process_iso_summer_flex").setVisible(false);

        }
        if (formContext.getControl("header_process_iso_is_si_subject") != null) {
            formContext.getControl("header_process_iso_is_si_subject").setVisible(false);

        }
        var indexBPFStage = "";
        var tabObj = formContext.ui.tabs.get("SUMMARY_TAB");

        if (tabObj != null) {
            formContext.getAttribute("iso_case_issue").setRequiredLevel("none");
            formContext.getAttribute("iso_initiator").setRequiredLevel("none");
            // Add onLoad and onChange event to the iso_case_issue field to handle auto-setting fields

            formContext.getAttribute("iso_case_issue").addOnChange(this.caseIssueOnChange);

            switch (caseType.getValue()) {
                case 863050000:
                    tabObj.sections.get("case_calls_section").setVisible(true);
                    indexBPFStage = "";
                    break;
                case 863050001:
                    tabObj.sections.get("case_calls_section").setVisible(true);
                    indexBPFStage = "";
                    break;
                case 863050002:
                    IsoFairlens.iso_case.toggleComplaintsOrUseofForceCaseSection(context);
                    if (formContext.getControl("header_process_iso_letter_approved") != null) {
                        formContext.getControl("header_process_iso_letter_approved").setDisabled(true);
                    }
                    indexBPFStage = "_1";
                    break;
                case 863050003:
                    IsoFairlens.iso_case.toggleDisciplinaryHearingReviews(context);
                    if (formContext.getControl("header_process_iso_letter_approved_2") != null) {
                        formContext.getControl("header_process_iso_letter_approved_2").setDisabled(true);
                    }
                    indexBPFStage = "_3";
                    break;
                case 863050004:
                    IsoFairlens.iso_case.toggleComplaintsOrUseofForceCaseSection(context);
                    if (formContext.getControl("header_process_iso_letter_approved_1") != null) {
                        formContext.getControl("header_process_iso_letter_approved_1").setDisabled(true);
                    }
                    indexBPFStage = "_2";
                    break;
                case 863050005:
                    IsoFairlens.iso_case.toggelCIRCaseSection(context, tabObj);
                    indexBPFStage = "_4";
                    break;
                case 863050006:
                    IsoFairlens.iso_case.toggelSQRCaseSection(context, tabObj);
                    indexBPFStage = "_5";
                    break;
                case 863050008:
                    IsoFairlens.iso_case.toggelSICaseSection(context, tabObj);
                    indexBPFStage = "_6";
                    break;
                case 863050007:
                    IsoFairlens.iso_case.toggelTRCaseSection(context, tabObj);
                    indexBPFStage = "_7";
                    break;
                default:
                    return;
            }
            //Check if user have Admin or System Admin role
            if (formContext.getControl("header_process_iso_case_review" + indexBPFStage) != null) {
                formContext.getControl("header_process_iso_case_review" + indexBPFStage).setDisabled(true);
            }
            if (formContext.getControl("header_process_iso_final_documentation" + indexBPFStage) != null) {
                formContext.getControl("header_process_iso_final_documentation" + indexBPFStage).setDisabled(true);
            }
            if (formContext.getControl("header_process_iso_resolve_close_case" + indexBPFStage) != null) {
                formContext.getControl("header_process_iso_resolve_close_case" + indexBPFStage).setDisabled(true);
            }
            if (formContext.getControl("header_process_customerid") != null) {
                formContext.getControl("header_process_customerid").setEntityTypes(["account"]);
                formContext.getControl("header_process_customerid").addPreSearch(filterAccounts);
                formContext.getControl("header_process_customerid").getAttribute().disableMru = true;
            }
            if (formContext.getControl("header_process_iso_case_client_name") != null) {
                formContext.getControl("header_process_iso_case_client_name").addPreSearch(filterContacts);
                formContext.getControl("header_process_iso_case_client_name").getAttribute().disableMru = true;
            }
            if ((caseType.getValue() === 863050000 || caseType.getValue() === 863050001 || caseType.getValue() === 863050002 || caseType.getValue() === 863050003 || caseType.getValue() === 863050004)) {
                formContext.getAttribute("iso_case_client_name").setRequiredLevel("required");
                if (formContext.getControl("header_process_iso_case_client_name") != null && formContext.getControl("header_process_iso_case_client_name").getAttribute() != null) {
                    formContext.getControl("header_process_iso_case_client_name").getAttribute().setRequiredLevel("required");
                }
            }
            if (formContext.getAttribute("iso_is_child_case") != null && formContext.getAttribute("iso_is_child_case").getValue()) {

                formContext.getAttribute("iso_is_child_case").setValue(false);
                if (formContext.getAttribute("iso_previous_sqr_case") != null && formContext.getAttribute("iso_previous_sqr_case").getValue() != null) {
                    formContext.getAttribute("iso_previous_sqr_case").setValue(null);
                }

            }
        } else { //quick create form
            var caseoptionSetControl = formContext.getControl("iso_case_type");
            caseoptionSetControl.removeOption(863050006);
            caseoptionSetControl.removeOption(863050007);
            caseoptionSetControl.removeOption(863050009);
            switch (caseType.getValue()) {
                case 863050000:
                case 863050002:
                case 863050004:
                    formContext.getAttribute("iso_case_issue").setRequiredLevel("required");
                    formContext.getAttribute("iso_initiator").setRequiredLevel("required");
                    formContext.getAttribute("iso_case_client_name").setRequiredLevel("required");
                    break;
                case 863050003:
                    formContext.getControl("iso_case_issue").setVisible(false);
                    formContext.getAttribute("iso_case_issue").setRequiredLevel("none");
                    formContext.getAttribute("iso_initiator").setRequiredLevel("required");
                    formContext.getAttribute("iso_case_client_name").setRequiredLevel("required");
                    break;
                case 863050005:
                    var tabqcObj = formContext.ui.tabs.get("tab_qc_case");
                    var sectsqrObj = tabqcObj.sections.get("sqr_section");
                    var sectotherdetailObj = tabqcObj.sections.get("other_details_section");
                    sectsqrObj.setVisible(false);
                    sectotherdetailObj.setVisible(true);
                    formContext.getAttribute("iso_case_issue").setRequiredLevel("none");
                    formContext.getAttribute("iso_initiator").setRequiredLevel("none");
                    formContext.getAttribute("iso_case_client_name").setRequiredLevel("none");
                    formContext.getControl("iso_case_issue").setVisible(false);
                    formContext.getControl("iso_initiator").setVisible(false);
                    formContext.getControl("iso_case_client_name").setVisible(false);
                    formContext.getAttribute("iso_incident_date").setRequiredLevel("required");
                    formContext.getAttribute("iso_cir_type").setRequiredLevel("required");
                    formContext.getAttribute("iso_report_due_date").setRequiredLevel("required");
                    formContext.getAttribute("iso_chair").setRequiredLevel("required");
                    break;
                case 863050008:
                    var tabqcObj = formContext.ui.tabs.get("tab_qc_case");
                    var sectsqrObj = tabqcObj.sections.get("sqr_section");
                    var sectotherdetailObj = tabqcObj.sections.get("other_details_section");
                    sectsqrObj.setVisible(false);
                    sectotherdetailObj.setVisible(true);
                    formContext.getAttribute("iso_case_issue").setRequiredLevel("none");
                    formContext.getAttribute("iso_initiator").setRequiredLevel("none");
                    formContext.getAttribute("iso_case_client_name").setRequiredLevel("none");
                    formContext.getAttribute("iso_cir_type").setRequiredLevel("none");
                    formContext.getControl("iso_case_issue").setVisible(false);
                    formContext.getControl("iso_initiator").setVisible(false);
                    formContext.getControl("iso_cir_type").setVisible(false);
                    formContext.getControl("iso_case_client_name").setVisible(false);
                    formContext.getAttribute("iso_incident_date").setRequiredLevel("required");
                    formContext.getAttribute("iso_report_due_date").setRequiredLevel("required");
                    formContext.getAttribute("iso_chair").setRequiredLevel("required");
                    break;
                default:
                    return;
            }
        }

        formContext.data.process.removeOnPreStageChange(IsoFairlens.iso_case.CheckMoveStage);
        formContext.data.process.addOnPreStageChange(IsoFairlens.iso_case.CheckMoveStage);
        formContext.data.process.removeOnPreProcessStatusChange(IsoFairlens.iso_case.BpfPreStatusChange);
        formContext.data.process.addOnPreProcessStatusChange(IsoFairlens.iso_case.BpfPreStatusChange);
        //set customer lookup only for account type
        formContext.getControl("customerid").setEntityTypes(["account"]);
        //add pre search
        formContext.getControl("customerid").addPreSearch(filterAccounts);
        formContext.getControl("iso_case_client_name").addPreSearch(filterContacts);
        var caseoptionSetControl = formContext.getControl("iso_case_type");
        var caseheaderoptionSetControl = formContext.getControl("header_iso_case_type");

        //remove Early Resolution from option
        if (caseoptionSetControl) {
            caseoptionSetControl.removeOption(863050001);
            caseoptionSetControl.removeOption(863050009);
        }
        if (caseheaderoptionSetControl) {
            caseheaderoptionSetControl.removeOption(863050001);
            caseheaderoptionSetControl.removeOption(863050009);
        }
        //disable customer number for update record      

        if (formMode >= 2) //update mode
        {
            if (formContext.getControl("customerid").getAttribute() != null) {
                formContext.getControl("iso_case_client_name").setDisabled(true);
                formContext.getControl("customerid").setDisabled(false);
                formContext.getControl("header_iso_case_type").setDisabled(true);
            }

            //setting for quick create
            window.top.attribute_iso_client = formContext.getAttribute("iso_case_client_name").getValue();
            window.top.attribute_iso_organization = formContext.getAttribute("customerid").getValue();
            window.top.attribute_iso_case_type = caseType.getValue();
            if (formMode === 2) //update mode
            {
                IsoFairlens.iso_case.CheckForUserSecurityRole(formContext);
            }
            if (caseType.getValue() === 863050006) {
                var caseOnsiteReviewDate = formContext.getAttribute("iso_onsite_review_date").getValue();
                IsoFairlens.iso_case.iso_sqr_curr_onsite_review_date = caseOnsiteReviewDate;
                if (formContext.getAttribute("iso_is_sqr_consideration_generated").getValue()) {
                    formContext.ui.setFormNotification("Kindly Press the Generate SQR Consideration to get the latest data", "INFO", "isqrconsinfo");
                }
            }
            if (formContext.getAttribute("iso_is_convert_from_phonecall") != null && formContext.getAttribute("iso_is_convert_from_phonecall").getValue()) {
                //create generic task     
                var caseid = formContext.data.entity.getId();
                var lookupcase = caseid.replace("{", "").replace("}", "");
                IsoFairlens.iso_common.GenerateTask(formContext, lookupcase, "case", 863050001, new Date(), false, true);
                formContext.getAttribute("iso_is_convert_from_phonecall").setValue(false);
                formContext.data.save();
            }

        }

        if (window.top.attribute_iso_client != null) {
            formContext.getAttribute("iso_case_client_name").setValue(window.top.attribute_iso_client);

        }
        if (window.top.attribute_iso_organization != null) {
            formContext.getAttribute("customerid").setValue(window.top.attribute_iso_organization);
        }

        //filter accounts based on Case Type selected
        function filterAccounts() {
            var accountFilter = "<filter type='and'><condition attribute='iso_account_type' operator='eq' value='863050000'/></filter>";
            formContext.getControl("customerid").addCustomFilter(accountFilter, "account");
            if (formContext.getControl("header_process_customerid") != null) {
                formContext.getControl("header_process_customerid").addCustomFilter(accountFilter, "account");
            }
        }
        //filter accounts based on Case Type selected
        function filterContacts() {
            var contactFilter = "<filter type='and'><condition attribute='iso_contact_type' operator='eq' value='863050000'/></filter>";
            formContext.getControl("iso_case_client_name").addCustomFilter(contactFilter, "contact");
            if (formContext.getControl("header_process_iso_case_client_name") != null) {
                formContext.getControl("header_process_iso_case_client_name").addCustomFilter(contactFilter, "contact");
            }
        }

        if (formContext.getAttribute("iso_account_name") != null && formContext.getAttribute("iso_account_name").getValue() == null && formMode === 2) {
            IsoFairlens.iso_case.OnCustomerSelect(context);
        }


        // Initial call to update contact address on form load
        if (formMode === 2) //update mode
        {
            this.updateClientAddress(context);
        }


        this.LoadBusinessHolidayList(context);


        // JSBATD-171 Contact - Default lookup to Correctional Centre should be "Active Correctional Centres"
        this.setDefaultCustomerLookupView(context);

    },

    // On save function
    FormOnSave: function (context) {

        var formContext = context.getFormContext ? context.getFormContext() : context;
        var formMode = formContext.ui.getFormType();
        if (formMode === 4) return;
        var caseType = formContext.getAttribute("iso_case_type").getValue();
        var tabObj = formContext.ui.tabs.get("SUMMARY_TAB");
        if (caseType === 863050005 || caseType === 863050008) {
            if (formMode === 1) {
                var casecreationdate = new Date(new Date().toLocaleDateString()).getTime();
            } else {
                var casecreationdate = formContext.getAttribute("createdon").getValue() ? new Date(new Date(formContext.getAttribute("createdon").getValue()).toLocaleDateString()).getTime() : null;
            }
            var incidentdate = formContext.getAttribute("iso_incident_date").getValue() ? new Date(new Date(formContext.getAttribute("iso_incident_date").getValue()).toLocaleDateString()).getTime() : null;
            var reportduedate = formContext.getAttribute("iso_report_due_date").getValue() ? new Date(new Date(formContext.getAttribute("iso_report_due_date").getValue()).toLocaleDateString()).getTime() : null;
            if (tabObj != null) {
                var isextendedduedate = formContext.getAttribute("iso_extension_requested").getValue();
                if (isextendedduedate) {
                    var extendedreportduedate = formContext.getAttribute("iso_extended_report_due_date").getValue() ? new Date(new Date(formContext.getAttribute("iso_extended_report_due_date").getValue()).toLocaleDateString()).getTime() : null;
                }
            }
            if (incidentdate && (incidentdate > casecreationdate)) {
                IsoFairlens.iso_case.OpenAlert("Incident Date should be earlier or equal than the Case Creation Date.  Kindly provide the correct date.");
                context.getEventArgs().preventDefault(); // Prevent Save
            } else if (reportduedate && !isextendedduedate && (reportduedate < casecreationdate)) {
                IsoFairlens.iso_case.OpenAlert("Report Due Date should be later or equal than the Case Creation Date.  Kindly provide the correct date.");
                context.getEventArgs().preventDefault(); // Prevent Save
            } else if ((tabObj != null) && isextendedduedate && (extendedreportduedate <= reportduedate)) {
                IsoFairlens.iso_case.OpenAlert("Report Due Date should be earlier than Extended Report Due Date.  Kindly provide the correct date.");
                context.getEventArgs().preventDefault(); // Prevent Save
            }
        }
    },

    // create case
    CreateCase: function (caseType) {
        if (caseType != "") {
            var entityFormOptions = {};
            var formParameters = {};
            window.top.attribute_iso_client = null;
            window.top.attribute_iso_organization = null;
            formParameters["iso_case_type"] = caseType;
            formParameters["header_process_iso_case_type"] = caseType;
            //formParameters["iso_is_parent"] = 863050000;
            entityFormOptions["entityName"] = "incident";
            Xrm.Navigation.openForm(entityFormOptions, formParameters).then(

                function (success) {
                    console.log(success);
                },

                function (error) {
                    console.log(error);
                });
        }
    },

    // case quick create
    QuickCreateCase: function (caseType, primaryControl) {
        if (caseType != "") {
            var entityFormOptions = {};
            var formParameters = {};
            window.top.attribute_iso_client = null;
            window.top.attribute_iso_organization = null;
            //create child case
            if (caseType == "1") {
                entityFormOptions["useQuickCreateForm"] = true;
                formParameters["iso_case_type"] = primaryControl.getAttribute("iso_case_type").getValue();
                formParameters["iso_case_client_name"] = primaryControl.getAttribute("iso_case_client_name").getValue();
                formParameters["customerid"] = primaryControl.getAttribute("customerid").getValue();
                formParameters["parentcaseid"] = primaryControl.data.entity.getId();
                formParameters["iso_notes"] = primaryControl.getAttribute("iso_notes").getValue();
                formParameters["iso_is_child_case"] = true;
                formParameters["iso_case_issues"] = 1;
                if (primaryControl.getAttribute("iso_case_type").getValue() === 863050000) {
                    if (primaryControl.getAttribute("iso_advice") != null && primaryControl.getAttribute("iso_advice").getValue() != null) {
                        formParameters["iso_advice"] = primaryControl.getAttribute("iso_advice").getValue();
                    }
                    if (primaryControl.getAttribute("iso_action") != null && primaryControl.getAttribute("iso_action").getValue() != null) {
                        formParameters["iso_action"] = primaryControl.getAttribute("iso_action").getValue();
                    }

                }

            } else {
                formParameters["iso_case_type"] = caseType;
                // formParameters["iso_is_parent"] = 863050000;
            }
            entityFormOptions["entityName"] = "incident";

            if (caseType == "863050005" || caseType == "863050006" || caseType == "863050007" || caseType == "863050008") {
                var lookupAccount = [{
                    "entityType": "account",
                    "name": primaryControl.getAttribute("name").getValue(),
                    "id": primaryControl.data.entity.getId()
                }]
                formParameters["customerid"] = lookupAccount;
                if (primaryControl.getAttribute("primarycontactid") != null && primaryControl.getAttribute("primarycontactid").getValue() != null) {
                    formParameters["iso_case_client_name"] = primaryControl.getAttribute("primarycontactid").getValue();
                }
            }
            if (caseType != "863050005" && caseType != "863050006" && caseType != "863050007" && caseType != "863050008" && caseType != 1) {
                var lookupContact = [{
                    "entityType": "contact",
                    "name": primaryControl.getAttribute("firstname").getValue() + " " + primaryControl.getAttribute("lastname").getValue(),
                    "id": primaryControl.data.entity.getId()
                }]
                formParameters["iso_case_client_name"] = lookupContact;
                if (primaryControl.getAttribute("parentcustomerid") != null && primaryControl.getAttribute("parentcustomerid").getValue() != null) {
                    formParameters["customerid"] = primaryControl.getAttribute("parentcustomerid").getValue();
                }

            }

            Xrm.Navigation.openForm(entityFormOptions, formParameters).then(

                function (success) {
                    console.log(success);
                },

                function (error) {
                    console.log(error);
                });
        }
    },

    EarlyResolution: function (caseType, context) {
        var formContext = context;
        if (formContext.getAttribute("iso_case_issue") == null || formContext.getAttribute("iso_case_issue").getValue() == null) {
            //alert("Please select Issue");
            IsoFairlens.iso_case.OpenAlert("Please select Issue");
            return false;
        }

        if (formContext.getAttribute("iso_initiator") == null || formContext.getAttribute("iso_initiator").getValue() == null) {
            // alert("Please select Initiator");
            IsoFairlens.iso_case.OpenAlert("Please select Initiator");
            return false;
        }

        var accountdata = formContext.getAttribute("customerid").getValue();
        var contactdata = formContext.getAttribute("iso_case_client_name").getValue();
        var issuedata = formContext.getAttribute("iso_case_issue").getValue();
        var caseid = formContext.data.entity.getId();
        // define the data to clone case
        var lookupAccount = accountdata[0].id.replace("{", "").replace("}", "");
        var lookupContact = contactdata[0].id.replace("{", "").replace("}", "");
        var lookupIssue = issuedata[0].id.replace("{", "").replace("}", "");
        var lookupcase = caseid.replace("{", "").replace("}", "");
        IsoFairlens.iso_case.CalculateServiceStd(formContext, lookupcase);
        var data = {
            "title": formContext.getAttribute("title").getValue(),
            "iso_Case_Client_Name@odata.bind": "/contacts(" + lookupContact + ")",
            "customerid_account@odata.bind": "/accounts(" + lookupAccount + ")",
            "iso_case_type": "863050001",
            "iso_action": formContext.getAttribute("iso_action").getValue(),
            "iso_advice": formContext.getAttribute("iso_advice").getValue(),
            "iso_account_name": formContext.getAttribute("iso_account_name").getValue(),
            "iso_notes": formContext.getAttribute("iso_notes").getValue(),
            "description": formContext.getAttribute("description").getValue(),
            "iso_sqr_consideration": formContext.getAttribute("iso_sqr_consideration").getValue(),
            "iso_case_issue@odata.bind": "/iso_issues(" + lookupIssue + ")",
            "iso_initiator": formContext.getAttribute("iso_initiator").getValue(),
            "parentcaseid@odata.bind": "/incidents(" + lookupcase + ")",
            "iso_is_child_case": true
        }
        //var jsonData = JSON.stringify(data);

        // create case record
        Xrm.WebApi.createRecord("incident", data).then(
            function success(result) {
                console.log("Clone Case created with ID: " + result.id);

                ////create generic task               
                IsoFairlens.iso_common.GenerateTask(formContext, result.id, "case", 863050001, new Date(), false, true);
                // perform operations to open record
                var entityFormOptions = {};
                entityFormOptions["entityName"] = "incident";
                entityFormOptions["entityId"] = result.id;

                // Open the form.
                Xrm.Navigation.openForm(entityFormOptions).then(
                    function (success) {
                        formContext.data.refresh();

                    },
                    function (error) {
                        console.log(error);
                    });
            },
            function (error) {
                console.log(error);
                // handle error conditions
            }
        );
    },

    CalculateServiceStd: async function (formContext, lookupcase) {
        var casetype = formContext.getAttribute("iso_case_type").getValue();
        if (casetype === 863050005 || casetype === 863050006 || casetype === 863050007 || casetype === 863050008) {
            return true;
        }

        var createdondt = new Date(formContext.getAttribute("createdon").getValue()).toLocaleDateString();
        var todaydt = new Date().toLocaleDateString();
        if (createdondt == todaydt) {
            var prevdata = {
                "iso_holiday": 0

            }
            // update previous case case record                
            Xrm.WebApi.updateRecord("incident", lookupcase, prevdata).then(
                function success(result) {
                    // res = JSON.parse(result);
                    console.log("Contact updated with ID: " + result.id);
                    return;
                    //the record is updated
                },
                function (error) {
                    console.log(error);
                    //handle error conditions
                }
            );
            return true;
        }
        //calculate weekends        
        var nonworkingdays = 0;
        for (var currentDate = new Date(createdondt); currentDate <= new Date(todaydt); currentDate.setDate(currentDate.getDate() + 1)) {
            if (currentDate.getDay() == 0 || currentDate.getDay() == 6) {
                nonworkingdays++;
            }
        }
        var holidays = 0;
        //check for holidays
        var holidayoption = formContext.getControl("iso_business_holidays");
        var options = holidayoption.getOptions();
        if (options.length > 0) {
            var holidays = IsoFairlens.iso_common.GetHolidays(formContext.getAttribute("createdon").getValue(), new Date(), options);

        }
        var actualholidays = (nonworkingdays + holidays);
        var prevdata = {
            "iso_holiday": actualholidays

        }
        // update previous case case record                
        Xrm.WebApi.updateRecord("incident", lookupcase, prevdata).then(
            function success(result) {
                //res = JSON.parse(result);
                console.log("Contact updated with ID: " + result.id);
                //the record is updated
            },
            function (error) {
                console.log(error);
                //handle error conditions
            }
        );

    },

    OnCustomerSelect: function (context) {
        var formContext = context.getFormContext();
        if (formContext.getAttribute("customerid") != null && formContext.getAttribute("customerid").getValue() != null) {
            var acountnamelookup = formContext.getAttribute("customerid").getValue();
            if (acountnamelookup.length == 0) {
                return false;
            }
            var lookupAccount = acountnamelookup[0].id.replace("{", "").replace("}", "");
            // Fetch the account short name

            var fetchXml =
                "<fetch version='1.0' >" +
                "<entity name='account'>" +
                "<attribute name='iso_short_name' /> " +
                "<filter>" +
                "<condition attribute='accountid' operator='eq' value='" + lookupAccount + "'/>" +
                "</filter>" +
                "</entity>" +
                "</fetch>";

            Xrm.WebApi.retrieveMultipleRecords("account", "?fetchXml=" + encodeURIComponent(fetchXml)).then(
                function success(results) {
                    var accountName = results.entities[0]["iso_short_name"];
                    formContext.getAttribute("iso_account_name").setValue(accountName);

                },
                function (error) {
                    console.log("Error fetching account short name: " + error.message);
                }
            );
        }
    },

    OpenAlert: function (msgstring) {
        var alertStrings = {
            confirmButtonLabel: "Ok",
            text: msgstring,
            title: "Case"
        };
        var alertOptions = {
            height: 120,
            width: 260
        };
        Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(
            function (success) {
                console.log("Alert dialog closed");
            },
            function (error) {
                console.log(error.message);
            }
        );
    },

    IsEarlyResolveCase: function (context) {
        var formContext = context.getFormContext();
        formContext.ui.refreshRibbon(false);
    },

    BpfPreResolveValidation: function (executionContext) {
        var formContext = null;
        if (executionContext !== null) {
            if (typeof executionContext.getAttribute === 'function') {
                formContext = executionContext; //most likely called from the ribbon.
            } else if (typeof executionContext.getFormContext === 'function' &&
                typeof (executionContext.getFormContext()).getAttribute === 'function') {
                formContext = executionContext.getFormContext(); // most likely called from the form via a handler
            }
        }
        // var formContext = executionContext.getFormContext();
        var casename = formContext.getAttribute("title").getValue();
        var encodedItem = encodeURIComponent(casename);
        var caseid = formContext.data.entity.getId();
        var lookupcase = caseid.replace("{", "").replace("}", "");
        var isResolve = formContext.getAttribute("iso_resolve_close_case").getValue();
        if (isResolve) {
            var fetchquery = "<fetch> " +
                "  <entity name='activitypointer'> " +
                "    <attribute name='subject' /> " +
                "    <attribute name='activitytypecode' /> " +
                "    <filter> " +
                "      <condition attribute='regardingobjectid' operator='eq' value='" + lookupcase + "' uiname='" + encodedItem + "' uitype='incident' /> " +
                "      <filter type='or'> " +
                "        <condition attribute='statecode' operator='eq' value='0' /> " +
                "        <condition attribute='statecode' operator='eq' value='3' /> " +
                "      </filter> " +
                "    </filter> " +
                "  </entity> " +
                "</fetch> ";
            Xrm.WebApi.retrieveMultipleRecords("activitypointer", "?fetchXml=" + encodeURIComponent(fetchquery)).then(
                function success(results) {
                    var recordcount = results.entities.length;
                    if (recordcount >= 1) {
                        IsoFairlens.iso_case.OpenAlert("Kindly complete all activities before resolving case");
                        formContext.getAttribute("iso_resolve_close_case").setValue(false);
                        return false;
                    } else {
                        IsoFairlens.iso_case.CalculateServiceStd(formContext, lookupcase);
                        //await delayer(2000); 
                        return true;
                    }

                },
                function (error) {
                    console.log("Error fetching activity associated with case: " + error.message);
                }
            );

        }
    },

    CheckForUserSecurityRole: function (context) {
        var gcontext = Xrm.Utility.getGlobalContext();
        context.getAttribute("iso_is_admin_user").setValue(false);
        var caseType = context.getAttribute("iso_case_type");
        //Store Security Roles
        var loggedUserRoles = gcontext.userSettings.roles;
        var isAdmin = true;
        var isManager = true;
        //Looping Through User's Security Roles.
        var bpfindex = "";
        switch (caseType.getValue()) {
            case 863050000:
            case 863050001:
                bpfindex = "";
                break;
            case 863050002:
                bpfindex = "_1";
                break;
            case 863050003:
                bpfindex = "_3";
                break;
            case 863050004:
                bpfindex = "_2";
                break;
            case 863050005:
                bpfindex = "_4";
                break;
            case 863050006:
                bpfindex = "_5";
                break;
            case 863050008:
                bpfindex = "_6";
                break;
            case 863050007:
                bpfindex = "_7";
                break;
            default:
                return;
        }
        loggedUserRoles.forEach(function hasRoleName(item, index) {
            if (((item.name == "ISO FairLens - Admin") || (item.name == "System Administrator") || (item.name == "ISO FairLens - Superuser")) && isAdmin) {
                context.getAttribute("iso_is_admin_user").setValue(true);
                if (context.getControl("header_process_iso_case_review" + bpfindex) != null) {
                    context.getControl("header_process_iso_case_review" + bpfindex).setDisabled(false);
                }
                if (context.getControl("header_process_iso_final_documentation" + bpfindex) != null) {
                    context.getControl("header_process_iso_final_documentation" + bpfindex).setDisabled(false);
                }
                if (context.getControl("header_process_iso_resolve_close_case" + bpfindex) != null) {
                    context.getControl("header_process_iso_resolve_close_case" + bpfindex).setDisabled(false);
                }
                isAdmin = false;
            }

        });
        loggedUserRoles.forEach(function hasRoleName(item, index) {
            if (((item.name == "ISO FairLens - Manager") || (item.name == "System Administrator") || (item.name == "ISO FairLens - Superuser")) && isManager) {
                if (caseType.getValue() === 863050002) {
                    if (context.getControl("header_process_iso_letter_approved") != null) {
                        context.getControl("header_process_iso_letter_approved").setDisabled(false);
                    }
                } else if (caseType.getValue() === 863050003) {
                    if (context.getControl("header_process_iso_letter_approved_2") != null) {
                        context.getControl("header_process_iso_letter_approved_2").setDisabled(false);
                    }
                } else if (caseType.getValue() === 863050004) {
                    if (context.getControl("header_process_iso_letter_approved_1") != null) {
                        context.getControl("header_process_iso_letter_approved_1").setDisabled(false);
                    }
                }
                isManager = false;
            }
        });
    },

    //JSBATD-120 Data Fields - case records (Disciplinary Hearing Reviews)
    toggleDisciplinaryHearingReviews: function (executionContext) {
        var formContext = executionContext.getFormContext();
        var caseTypeValue = formContext.getAttribute("iso_case_type").getValue();
        var isDisciplinaryCaseType = caseTypeValue === 863050003;
        var tabObj = formContext.ui.tabs.get("SUMMARY_TAB");

        if (tabObj) {
            // Toggle visibility and requirement of fields and sections based on caseTypeValue
            var complaintsSection = tabObj.sections.get("case_complaint_section");
            var caseNotesSection = tabObj.sections.get("case_notes");
            var caseDispositionSection = tabObj.sections.get("diciplinary_hearing_section");

            if (complaintsSection) {
                complaintsSection.setVisible(isDisciplinaryCaseType);
            }
            if (caseNotesSection) {
                caseNotesSection.setVisible(!isDisciplinaryCaseType);
            }
            if (caseDispositionSection) {
                caseDispositionSection.setVisible(isDisciplinaryCaseType);
            }

            // iso_case_issue field
            var caseIssueField = formContext.getAttribute("iso_case_issue");
            if (caseIssueField) {
                caseIssueField.setValue(isDisciplinaryCaseType ? null : caseIssueField.getValue());
                caseIssueField.setRequiredLevel(!isDisciplinaryCaseType ? "required" : "none");
                formContext.getControl("iso_case_issue").setVisible(!isDisciplinaryCaseType);
            }

            // iso_case_disposition field
            var caseDispositionField = formContext.getAttribute("iso_case_disposition");
            if (caseDispositionField) {
                caseDispositionField.setRequiredLevel(isDisciplinaryCaseType ? "required" : "none");
                if (!isDisciplinaryCaseType) {
                    caseDispositionField.setValue(null);
                }
            }

            // Fields moved to diciplinary_hearing_section
            // iso_case_charge field
            var caseCharge = formContext.getAttribute("iso_case_charge");
            if (caseCharge) {
                caseCharge.setRequiredLevel(isDisciplinaryCaseType ? "required" : "none");
                formContext.getControl("iso_case_charge").setVisible(isDisciplinaryCaseType);
            }

            // independent adjudicator field
            var indAdjudicator = formContext.getAttribute("iso_case_independent_adjudicator");
            if (indAdjudicator) {
                indAdjudicator.setRequiredLevel(isDisciplinaryCaseType ? "required" : "none");
                formContext.getControl("iso_case_independent_adjudicator").setVisible(isDisciplinaryCaseType);
                formContext.getControl("iso_case_independent_adjudicator").addPreSearch(filterIndeAdjustContacts);

                function filterIndeAdjustContacts() {
                    var contactFilter = "<filter type='and'><condition attribute='iso_contact_type' operator='eq' value='863050010'/> " +
                        " <condition attribute='statecode' operator='eq' value='0' /> " +
                        " </filter > ";
                    formContext.getControl("iso_case_independent_adjudicator").addCustomFilter(contactFilter, "contact");
                }
            }

            // Fields moved to case_details
            // iso_date_requested field
            var dateRequested = formContext.getAttribute("iso_date_requested");
            if (dateRequested) {
                //dateRequested.setRequiredLevel(isDisciplinaryCaseType ? "required" : "none");
                formContext.getControl("iso_date_requested").setVisible(isDisciplinaryCaseType);
            }

            // iso_date_received field
            var dateRecieved = formContext.getAttribute("iso_date_received");
            if (dateRecieved) {
                //dateRecieved.setRequiredLevel(isDisciplinaryCaseType ? "required" : "none");
                formContext.getControl("iso_date_received").setVisible(isDisciplinaryCaseType);
            }

            // iso_time_elapsed field
            var timeElapsed = formContext.getAttribute("iso_time_elapsed");
            if (timeElapsed) {
                //timeElapsed.setRequiredLevel(isDisciplinaryCaseType ? "required" : "none");
                formContext.getControl("iso_time_elapsed").setVisible(isDisciplinaryCaseType);
            }

            // Toggle option set values for iso_decision_type
            var decisionControl = formContext.getControl("iso_decision_type");
            if (decisionControl) {
                var existingOptions = decisionControl.getAttribute().getOptions();
                decisionControl.clearOptions();

                existingOptions.forEach(function (option) {
                    if (isDisciplinaryCaseType) {
                        if (option.value >= 863050006 && option.value <= 863050011) {
                            decisionControl.addOption(option);
                        }
                    } else {
                        if (option.value < 863050006 || option.value > 863050011) {
                            decisionControl.addOption(option);
                        }
                    }
                });
            }
        }
    },

    //JSBATD-117 &121 Data Fields - case records (Complaints & Use of Force)
    toggleComplaintsOrUseofForceCaseSection: function (executionContext) {
        var formContext = executionContext.getFormContext();
        var caseTypeValue = formContext.getAttribute("iso_case_type").getValue();
        var isSpecificCaseType = (caseTypeValue === 863050002 || caseTypeValue === 863050004);
        var tabObj = formContext.ui.tabs.get("SUMMARY_TAB");

        if (tabObj) {
            // Toggle visibility of case_complaint_section
            var complaintsSection = tabObj.sections.get("case_complaint_section");
            if (complaintsSection) {
                complaintsSection.setVisible(isSpecificCaseType);
            }
        }

        // Toggle option set values
        var decisionControl = formContext.getControl("iso_decision_type");
        if (decisionControl) {
            decisionControl.clearOptions();
            var allOptions = decisionControl.getAttribute().getOptions();

            allOptions.forEach(function (option) {
                if (isSpecificCaseType) {
                    // include "Multiple Issues"
                    if (option.value === 863050012 || (option.value >= 863050000 && option.value <= 863050005)) {
                        decisionControl.addOption(option);
                    }
                } else {
                    // exclude "Multiple Issues"
                    if (option.value !== 863050012) {
                        decisionControl.addOption(option);
                    }
                }
            });
        }
    },

    // function
    ReactivateCase: function (context) {
        var formContext = context;

        var caseid = formContext.data.entity.getId();
        var lookupcase = caseid.replace("{", "").replace("}", "");
        var prevdata = {
            "statecode": 0,
            "statuscode": 1

        }
        // update previous case case record                
        Xrm.WebApi.updateRecord("incident", lookupcase, prevdata).then(
            function success(result) {

                console.log("case updated with ID: " + result.id);
                formContext.data.process.setStatus("active", RefreshFormCallback);

                function RefreshFormCallback() {
                    formContext.data.refresh(true);
                    IsoFairlens.iso_case.CheckForUserSecurityRole(formContext);
                    formContext.getAttribute("iso_resolve_close_case").setValue(false);
                }

                //the record is updated
            },
            function (error) {
                console.log(error);
                //handle error conditions
            }
        );


    },

    // function
    toggelCIRCaseSection: function (executionContext, tabObj) {
        var formContext = executionContext.getFormContext();
        var sectpartylistObj = tabObj.sections.get("ORG_CLIENT_CASE_PL_SEC");
        var sectclientObj = tabObj.sections.get("client");
        var recommendationabObj = formContext.ui.tabs.get("recommendation_tab");
        var considersecObj = tabObj.sections.get("case_client_consideration_section");
        if (formContext.getAttribute("iso_case_client_name") != null) {
            formContext.getAttribute("iso_case_client_name").setValue(null);
        }
        if (formContext.getControl("header_process_iso_case_client_name") != null) {
            formContext.getControl("header_process_iso_case_client_name").setVisible(false);
            formContext.getControl("header_process_iso_case_client_name").getAttribute().setValue(null);

        }

        if (formContext.getControl("header_process_iso_incident_date") != null) {
            formContext.getControl("header_process_iso_incident_date").setVisible(true);
            formContext.getControl("header_process_iso_incident_date").getAttribute().setRequiredLevel("required");

        }
        if (formContext.getControl("header_process_iso_cir_type") != null) {
            formContext.getControl("header_process_iso_cir_type").setVisible(true);
            formContext.getControl("header_process_iso_cir_type").getAttribute().setRequiredLevel("required");

        }
        if (formContext.getControl("header_process_iso_chair") != null) {
            formContext.getControl("header_process_iso_chair").setVisible(true);
            formContext.getControl("header_process_iso_chair").getAttribute().setRequiredLevel("required");
            formContext.getControl("header_process_iso_chair").addPreSearch(filterChairContacts);
            formContext.getControl("header_process_iso_chair").getAttribute().disableMru = true;

        }
        if (formContext.getControl("header_process_iso_report_due_date") != null) {
            formContext.getControl("header_process_iso_report_due_date").setVisible(true);
            formContext.getControl("header_process_iso_report_due_date").getAttribute().setRequiredLevel("required");

        }
        recommendationabObj.setVisible(true);
        sectpartylistObj.setVisible(true);
        considersecObj.setVisible(false);
        sectclientObj.setVisible(false);

        formContext.getControl("iso_initiator").setVisible(false);
        formContext.getControl("iso_case_issue").setVisible(false);
        formContext.getControl("iso_sqr_consideration").setVisible(false);
        formContext.getControl("description").setVisible(false);
        formContext.getControl("iso_historical_iso_case_number").setVisible(false);
        formContext.getControl("iso_service_standards").setVisible(false);

        formContext.getControl("iso_incident_date").setVisible(true);
        formContext.getControl("iso_cir_type").setVisible(true);
        formContext.getControl("iso_report_due_date").setVisible(true);
        formContext.getControl("iso_extension_requested").setVisible(true);
        formContext.getControl("iso_report_agreement").setVisible(true);
        formContext.getControl("iso_chair").setVisible(true);

        formContext.getAttribute("iso_incident_date").setRequiredLevel("required");
        formContext.getAttribute("iso_cir_type").setRequiredLevel("required");
        formContext.getAttribute("iso_report_due_date").setRequiredLevel("required");
        formContext.getAttribute("iso_chair").setRequiredLevel("required");


        formContext.getControl("iso_chair").addPreSearch(filterChairContacts);

        function filterChairContacts() {
            var contactFilter = "<filter type='and'><condition attribute='iso_contact_type' operator='neq' value='863050000'/> " +
                " <condition attribute='statecode' operator='eq' value='0' /> " +
                " </filter > ";
            formContext.getControl("iso_chair").addCustomFilter(contactFilter, "contact");
        }


    },

    // function
    toggelSQRCaseSection: function (executionContext, tabObj) {
        var formContext = executionContext.getFormContext();
        var sqrsection = tabObj.sections.get("sqr_section_summary_tab");
        var notessection = tabObj.sections.get("case_notes");
        var sectclientObj = tabObj.sections.get("client");
        var considersecObj = tabObj.sections.get("case_client_consideration_section");
        var recommendationabObj = formContext.ui.tabs.get("recommendation_tab");
        var sqrconsiderationbObj = formContext.ui.tabs.get("sqr_connection_tab");
        var casereltab = formContext.ui.tabs.get("CASERELATIONSHIP_TAB");
        var casetype = formContext.getAttribute("iso_case_type").getValue();

        if (formContext.getAttribute("iso_case_client_name") != null) {
            formContext.getAttribute("iso_case_client_name").setValue(null);
        }
        if (formContext.getControl("header_process_iso_case_client_name") != null) {
            formContext.getControl("header_process_iso_case_client_name").setVisible(false);
            formContext.getControl("header_process_iso_case_client_name").getAttribute().setValue(null);

        }
        if (formContext.getControl("header_process_iso_onsite_review_date") != null) {
            formContext.getControl("header_process_iso_onsite_review_date").setVisible(true);
            formContext.getControl("header_process_iso_onsite_review_date").getAttribute().setRequiredLevel("required");


        }
        if (formContext.getControl("header_process_iso_previous_sqr_case") != null) {
            formContext.getControl("header_process_iso_previous_sqr_case").setVisible(true);
            formContext.getControl("header_process_iso_previous_sqr_case").setLabel("Previous SQR Case");
            formContext.getControl("header_process_iso_previous_sqr_case").addPreSearch(filterSQRCasesWithCust);
            formContext.getControl("header_process_iso_previous_sqr_case").getAttribute().disableMru = true;


        }
        if (formContext.getControl("header_process_iso_summer_flex") != null) {
            formContext.getControl("header_process_iso_summer_flex").setVisible(true);

        }
        sectclientObj.setVisible(false);
        notessection.setVisible(false);
        sqrsection.setVisible(true);
        recommendationabObj.setVisible(true);
        sqrconsiderationbObj.setVisible(true);
        casereltab.setVisible(false);
        considersecObj.setVisible(false);

        formContext.getControl("iso_initiator").setVisible(false);
        formContext.getControl("iso_case_issue").setVisible(false);
        formContext.getControl("iso_sqr_consideration").setVisible(false);
        formContext.getControl("description").setVisible(false);
        formContext.getControl("iso_historical_iso_case_number").setVisible(false);
        formContext.getControl("iso_service_standards").setVisible(false);

        formContext.getControl("iso_previous_sqr_case").setVisible(true);
        formContext.getControl("iso_previous_sqr_case").setLabel("Previous SQR Case");
        formContext.getControl("iso_onsite_review_date").setVisible(true);
        formContext.getControl("iso_summer_flex").setVisible(true);
        formContext.getAttribute("iso_onsite_review_date").setRequiredLevel("required");


        formContext.getControl("iso_previous_sqr_case").addPreSearch(filterSQRCasesWithCust);

        function filterSQRCasesWithCust() {
            if (formContext.getAttribute("customerid") != null && formContext.getAttribute("customerid").getValue() != null) {
                var formMode = formContext.ui.getFormType();
                var customerdata = formContext.getAttribute("customerid").getValue();
                var custid = customerdata[0].id.replace("{", "").replace("}", "");
                var custname = customerdata[0].name;
                var encodedItem = encodeURIComponent(custname);
                if (formMode >= 2) //update mode
                {

                    var casename = formContext.getAttribute("title").getValue();
                    var encodedcasename = encodeURIComponent(casename);
                    var caseid = formContext.data.entity.getId();
                    var lookupcase = caseid.replace("{", "").replace("}", "");
                    var sqrcaseFilter = "<filter type='and'><condition attribute='iso_case_type' operator='eq' value='" + casetype + "'/>  " +
                        "      <condition attribute='customerid' operator='eq' value='" + custid + "' uiname='" + encodedItem + "' uitype='account' /> " +
                        "  <condition attribute='incidentid' operator='ne' uiname='" + encodedcasename + "' uitype='incident' value='" + lookupcase + "' /> " +
                        "  </filter > ";
                    formContext.getControl("iso_previous_sqr_case").addCustomFilter(sqrcaseFilter, "incident");
                    formContext.getControl("header_process_iso_previous_sqr_case").addCustomFilter(sqrcaseFilter, "incident");
                } else {
                    var sqrcaseFilter = "<filter type='and'><condition attribute='iso_case_type' operator='eq' value='" + casetype + "'/>  " +
                        "      <condition attribute='customerid' operator='eq' value='" + custid + "' uiname='" + encodedItem + "' uitype='account' /> " +
                        "  </filter > ";
                    formContext.getControl("iso_previous_sqr_case").addCustomFilter(sqrcaseFilter, "incident");
                    formContext.getControl("header_process_iso_previous_sqr_case").addCustomFilter(sqrcaseFilter, "incident");
                }
            } else {
                var sqrcaseFilter = "<filter type='and'><condition attribute='iso_case_type' operator='eq' value='" + casetype + "'/>  </filter >";
                formContext.getControl("iso_previous_sqr_case").addCustomFilter(sqrcaseFilter, "incident");
                formContext.getControl("header_process_iso_previous_sqr_case").addCustomFilter(sqrcaseFilter, "incident");
            }
        }
        if (casetype === 863050006) {
            formContext.getAttribute("customerid").addOnChange(this.sqrPreviousCaseFilter);
        }

    },

    sqrPreviousCaseFilter: function (context) {
        var formContext = context.getFormContext ? context.getFormContext() : context;

        if (formContext.getAttribute("iso_previous_sqr_case") != null && formContext.getAttribute("iso_previous_sqr_case").getValue() != null) {
            formContext.getAttribute("iso_previous_sqr_case").setValue(null);
        }



    },

    // New function to update contact address
    updateClientAddress: function (executionContext) {
        var formContext = executionContext.getFormContext();
        var contactField = formContext.getAttribute("iso_case_client_name");

        if (contactField && contactField.getValue() != null) {
            var contactId = contactField.getValue()[0].id.replace("{", "").replace("}", ""); // Clean GUID
            Xrm.WebApi.retrieveRecord("contact", contactId, "?$select=_parentcustomerid_value").then(
                function (contactRecord) {
                    var accountId = contactRecord["_parentcustomerid_value"];
                    if (accountId) {
                        Xrm.WebApi.retrieveRecord("account", accountId, "?$select=address1_composite").then(
                            function (accountRecord) {
                                var address = accountRecord["address1_composite"];
                                if (address) {
                                    formContext.getAttribute("iso_client_address").setValue(address);
                                }
                            },
                            function (error) {
                                console.log("Error retrieving Account: " + error.message);
                            }
                        );
                    }
                },
                function (error) {
                    console.log("Error retrieving Contact: " + error.message);
                }
            );
        }
    },

    // function
    OnCaseTypeChange: function (context) {
        var formContext = context.getFormContext();
        var tabObj = formContext.ui.tabs.get("tab_qc_case");
        var sectsqrObj = tabObj.sections.get("sqr_section");
        var sectotherdetailObj = tabObj.sections.get("other_details_section");
        var caseType = formContext.getAttribute("iso_case_type");
        switch (caseType.getValue()) {
            case 863050000:
            case 863050002:
            case 863050003:
            case 863050004:
                formContext.getAttribute("iso_case_issue").setRequiredLevel("required");
                formContext.getAttribute("iso_initiator").setRequiredLevel("required");
                formContext.getAttribute("iso_case_client_name").setRequiredLevel("required");

                formContext.getAttribute("iso_incident_date").setRequiredLevel("none");
                formContext.getAttribute("iso_cir_type").setRequiredLevel("none");
                formContext.getAttribute("iso_report_due_date").setRequiredLevel("none");
                formContext.getAttribute("iso_chair").setRequiredLevel("none");
                formContext.getControl("iso_case_issue").setVisible(true);
                formContext.getControl("iso_initiator").setVisible(true);
                formContext.getControl("iso_case_client_name").setVisible(true);

                sectsqrObj.setVisible(true);
                sectotherdetailObj.setVisible(false);
                break;
            case 863050005:

                formContext.getAttribute("iso_case_issue").setRequiredLevel("none");
                formContext.getAttribute("iso_initiator").setRequiredLevel("none");
                formContext.getAttribute("iso_case_client_name").setRequiredLevel("none");

                formContext.getAttribute("iso_incident_date").setRequiredLevel("required");
                formContext.getAttribute("iso_cir_type").setRequiredLevel("required");
                formContext.getAttribute("iso_report_due_date").setRequiredLevel("required");
                formContext.getAttribute("iso_chair").setRequiredLevel("required");
                formContext.getControl("iso_case_issue").setVisible(false);
                formContext.getControl("iso_initiator").setVisible(false);
                formContext.getControl("iso_case_client_name").setVisible(false);
                sectsqrObj.setVisible(false);
                sectotherdetailObj.setVisible(true);
                formContext.getControl("iso_chair").addPreSearch(filterChairContacts);

                function filterChairContacts() {
                    var contactFilter = "<filter type='and'><condition attribute='iso_contact_type' operator='neq' value='863050000'/> " +
                        " <condition attribute='statecode' operator='eq' value='0' /> " +
                        " </filter > ";
                    formContext.getControl("iso_chair").addCustomFilter(contactFilter, "contact");
                }
                break;
            case 863050008:

                formContext.getAttribute("iso_case_issue").setRequiredLevel("none");
                formContext.getAttribute("iso_initiator").setRequiredLevel("none");
                formContext.getAttribute("iso_case_client_name").setRequiredLevel("none");
                formContext.getAttribute("iso_cir_type").setRequiredLevel("none");
                formContext.getAttribute("iso_incident_date").setRequiredLevel("required");
                formContext.getAttribute("iso_report_due_date").setRequiredLevel("required");
                formContext.getAttribute("iso_chair").setRequiredLevel("required");
                formContext.getControl("iso_case_issue").setVisible(false);
                formContext.getControl("iso_cir_type").setVisible(false);
                formContext.getControl("iso_initiator").setVisible(false);
                formContext.getControl("iso_case_client_name").setVisible(false);
                sectsqrObj.setVisible(false);
                sectotherdetailObj.setVisible(true);
                formContext.getControl("iso_chair").addPreSearch(filterChairContacts);

                function filterChairContacts() {
                    var contactFilter = "<filter type='and'><condition attribute='iso_contact_type' operator='neq' value='863050000'/> " +
                        " <condition attribute='statecode' operator='eq' value='0' /> " +
                        " </filter > ";
                    formContext.getControl("iso_chair").addCustomFilter(contactFilter, "contact");
                }
                break;
            default:
                return;
        }
    },

    // function
    CheckMoveStage: function (context) {
        var formContext = context.getFormContext ? context.getFormContext() : context;
        var targetStageName = formContext.data.process.getActiveStage().getName();
        var tabObj = formContext.ui.tabs.get("SUMMARY_TAB");
        if (targetStageName === "DOCUMENT" || targetStageName === "IDENTIFY") {

            var caseType = formContext.getAttribute("iso_case_type").getValue();
            if (caseType === 863050000 || caseType === 863050001 || caseType === 863050002 || caseType === 863050003 || caseType === 863050004) return;

            var casecreationdate = formContext.getAttribute("createdon").getValue() ? new Date(new Date(formContext.getAttribute("createdon").getValue()).toLocaleDateString()).getTime() : null;

            var incidentdate = formContext.getAttribute("iso_incident_date").getValue() ? new Date(new Date(formContext.getAttribute("iso_incident_date").getValue()).toLocaleDateString()).getTime() : null;
            var reportduedate = formContext.getAttribute("iso_report_due_date").getValue() ? new Date(new Date(formContext.getAttribute("iso_report_due_date").getValue()).toLocaleDateString()).getTime() : null;
            if (tabObj != null) {
                var isextendedduedate = formContext.getAttribute("iso_extension_requested").getValue();
                if (isextendedduedate) {
                    var extendedreportduedate = formContext.getAttribute("iso_extended_report_due_date").getValue() ? new Date(new Date(formContext.getAttribute("iso_extended_report_due_date").getValue()).toLocaleDateString()).getTime() : null;
                }
            }
            if (incidentdate && (incidentdate > casecreationdate)) {
                IsoFairlens.iso_case.OpenAlert("Incident Date should be earlier or equal than the Case Creation Date.  Kindly provide the correct date.");
                context.getEventArgs().preventDefault(); // Prevent Save
            } else if (reportduedate && !isextendedduedate && (reportduedate < casecreationdate)) {
                IsoFairlens.iso_case.OpenAlert("Report Due Date should be later or equal than the Case Creation Date.  Kindly provide the correct date.");
                context.getEventArgs().preventDefault(); // Prevent Save
            } else if ((tabObj != null) && isextendedduedate && (extendedreportduedate <= reportduedate)) {
                IsoFairlens.iso_case.OpenAlert("Report Due Date should be earlier than Extended Report Due Date.  Kindly provide the correct date.");
                context.getEventArgs().preventDefault(); // Prevent Save
            }
        }
    },

    BpfPreStatusChange: async function (context) {
        var formContext = context.getFormContext ? context.getFormContext() : context;
        var bpfStatus = formContext.data.process.getStatus();
        var caseid = formContext.data.entity.getId();
        var lookupcase = caseid.replace("{", "").replace("}", "");
        if (bpfStatus === "active") {
            IsoFairlens.iso_case.CalculateServiceStd(formContext, lookupcase);
            await delayer(4000);
            console.log("service Standard Calculated")
        }
    },


    // New function to handle changes to the iso_case_issue field
    caseIssueOnChange: function (context) {
        var formContext = context.getFormContext ? context.getFormContext() : context;
        var caseIssue = formContext.getAttribute("iso_case_issue");
        var decisionType = formContext.getAttribute("iso_decision_type");
        var decisionDate = formContext.getAttribute("iso_decision_date");
        var headercaseType = formContext.getAttribute("iso_case_type");

        if (formContext.getControl("header_process_iso_decision") != null) {
            var bpfDecision = formContext.getControl("header_process_iso_decision").getAttribute();
        }
        if (formContext.getControl("header_process_iso_is_decision_date") != null) {
            var bpfDecisionDate = formContext.getControl("header_process_iso_is_decision_date").getAttribute();
        }

        if (!decisionType) {
            console.error("Decision type attribute is not available on the form.");
            return;
        }

        var caseTypeValue = headercaseType.getValue();
        var selectedIssue = caseIssue.getValue();
        var selectedDecisionType = decisionType.getValue();
        var selectedDecisionDate = decisionDate.getValue();

        if (caseTypeValue === 863050002) {
            var isMultipleIssues = selectedIssue && selectedIssue[0] && selectedIssue[0].name === "Multiple Issues";

            if (isMultipleIssues) {
                decisionType.setValue(863050012); // "Multiple Issues"
                decisionDate.setValue(new Date());
                if (bpfDecision != null) {
                    bpfDecision.setValue(true);
                }
                if (bpfDecisionDate != null) {
                    bpfDecisionDate.setValue(true);
                }
            } else {
                //decisionType.setValue(null);
                //decisionDate.setValue(null);
                // Only set BPF fields to false if both decisionType and decisionDate are blank
                //bpfDecision.setValue(selectedDecisionType !== null && selectedDecisionDate !== null);
                //bpfDecisionDate.setValue(selectedDecisionType !== null && selectedDecisionDate !== null);
                if (decisionType.getValue() == null && bpfDecision != null) {
                    bpfDecision.setValue(false);
                }
                if (decisionDate.getValue() == null && bpfDecisionDate != null) {
                    bpfDecisionDate.setValue(false);
                }
            }
        }
    },

    CreateTask: function (primaryControl) {
        var formContext = primaryControl;
        var caseid = formContext.data.entity.getId();
        var lookupcase = caseid.replace("{", "").replace("}", "");
        var caseTypeValue = formContext.getAttribute("iso_case_type").getValue();
        var caseOnsiteReviewDate = formContext.getAttribute("iso_onsite_review_date").getValue();
        var issummerflex = formContext.getAttribute("iso_summer_flex").getValue();
        var casename = formContext.getAttribute("title").getValue();
        var encodedcasename = encodeURIComponent(casename);
        var confirmStrings = {
            text: "Do you also wish to update all deadline dates and task due dates ?",
            title: "Case"
        };
        var fetchquery = "<fetch> " +
            "  <entity name='task'> " +
            "    <attribute name='scheduledend' /> " +
            "    <attribute name='iso_setting_name' /> " +
            "    <filter> " +
            "      <condition attribute='statecode' operator='eq' value='0' /> " +
            "      <condition attribute='regardingobjectid' operator='eq' value='" + lookupcase + "' uiname='" + encodedcasename + "' uitype='incident' /> " +
            "      <condition attribute='iso_is_task_created' operator='eq' value='1'/> " +
            "       <condition attribute='iso_setting_name' operator='not-null' /> " +
            "    </filter> " +
            "  </entity> " +
            "</fetch>"

        Xrm.WebApi.retrieveMultipleRecords("task", "?fetchXml=" + encodeURIComponent(fetchquery)).then(
            function success(results) {
                if (results.entities.length === 0) {
                    IsoFairlens.iso_common.GenerateTask(formContext, lookupcase, "case", caseTypeValue, caseOnsiteReviewDate, issummerflex, false);
                } else {
                    Xrm.Navigation.openConfirmDialog(confirmStrings, null).then(
                        function (success) {
                            if (success.confirmed)
                                IsoFairlens.iso_common.CalculateTaskDueDate(formContext, lookupcase, casename, 0, caseTypeValue, caseOnsiteReviewDate, issummerflex);
                            else
                                return;
                        });

                }
            },
            function (error) {
                console.log("Error fetching activity associated with case: " + error.message);
            }
        );


    },

    SQROnsiteReviewDateOnChange: function (context) {
        var formContext = context.getFormContext ? context.getFormContext() : context;
        var formMode = formContext.ui.getFormType();
        var confirmStrings = {
            text: "Do you also wish to update all deadline dates and task due dates for this SQR ?",
            title: "Case"
        };
        formContext.getAttribute("iso_is_sqr_consideration_generated").setValue(true);

        var caseTypeValue = formContext.getAttribute("iso_case_type").getValue();
        var caseOnsiteReviewDate = formContext.getAttribute("iso_onsite_review_date").getValue();
        var issummerflex = formContext.getAttribute("iso_summer_flex").getValue();
        if (formMode === 1) {
            IsoFairlens.iso_case.iso_sqr_curr_onsite_review_date = caseOnsiteReviewDate;
            IsoFairlens.iso_common.UpdateFormFieldData(formContext, caseTypeValue, caseOnsiteReviewDate, issummerflex);
        }
        if (formMode >= 2) //update mode
        {
            Xrm.Navigation.openConfirmDialog(confirmStrings, null).then(
                function (success) {
                    if (success.confirmed) {
                        if (caseTypeValue === 863050006) {
                            formContext.ui.setFormNotification("Kindly Press the Generate SQR Consideration to get the latest data", "INFO", "isqrconsinfo");
                        }
                        IsoFairlens.iso_case.iso_sqr_curr_onsite_review_date = caseOnsiteReviewDate;
                        IsoFairlens.iso_common.UpdateFormFieldData(formContext, caseTypeValue, caseOnsiteReviewDate, issummerflex);
                        var casename = formContext.getAttribute("title").getValue();
                        var caseid = formContext.data.entity.getId();
                        var lookupcase = caseid.replace("{", "").replace("}", "");
                        IsoFairlens.iso_common.CalculateTaskDueDate(formContext, lookupcase, casename, 0, caseTypeValue, caseOnsiteReviewDate, issummerflex);
                    } else {
                        formContext.getAttribute("iso_onsite_review_date").setValue(IsoFairlens.iso_case.iso_sqr_curr_onsite_review_date);
                        formContext.data.save();
                    }
                });
        }
    },

    LoadBusinessHolidayList: function (context) {
        var formContext = context.getFormContext ? context.getFormContext() : context;
        var bussholiOptionSet = formContext.getControl("iso_business_holidays");
        var options = bussholiOptionSet.getOptions();
        var fetchXml =
            "<fetch > " +
            "<entity name='iso_business_closure'>" +
            " <attribute name='iso_name' /> " +
            "  <attribute name='iso_date' /> " +
            "  <filter type='and'> " +
            "  <condition attribute='statecode' operator='eq' value='0' /> " +
            "  <filter type='or'> " +
            "  <condition attribute='iso_date' operator='this-year'  /> " +
            "  <condition attribute='iso_date' operator='next-year'  /> " +
            "  <condition attribute='iso_date' operator='last-year'  /> " +
            " </filter> " +
            " </filter> " +
            " </entity> " +
            "</fetch>"

        Xrm.WebApi.retrieveMultipleRecords("iso_business_closure", "?fetchXml=" + encodeURIComponent(fetchXml)).then(
            function success(results) {
                if (options.length === results.entities.length) {
                    return;
                }
                for (var i = 0; i < results.entities.length; i++) {
                    var p = results.entities[i];
                    var obj = {};
                    obj["text"] = new Date(p.iso_date).toLocaleDateString();
                    obj["value"] = i;
                    if (bussholiOptionSet != null) {
                        bussholiOptionSet.addOption(obj);
                    }
                }

            },
            function (error) {
                console.log("Error fetching business holiday: " + error.message);
            }
        );
    },

    CheckDecisionValues: function (context) {
        var formContext = context.getFormContext ? context.getFormContext() : context;
        var caseTypeValue = formContext.getAttribute("iso_case_type").getValue();
        var bpfindex = "";
        switch (caseTypeValue) {
            case 863050002:
                bpfindex = "";
                if (formContext.getAttribute("iso_decision_type") != null && formContext.getAttribute("iso_decision_type").getValue() != null) {
                    formContext.getControl("header_process_iso_decision" + bpfindex).getAttribute().setValue(true);
                } else {
                    formContext.getControl("header_process_iso_decision" + bpfindex).getAttribute().setValue(false);
                }
                if (formContext.getAttribute("iso_decision_rational") != null && formContext.getAttribute("iso_decision_rational").getValue() != null) {
                    formContext.getControl("header_process_iso_is_decision_rational" + bpfindex).getAttribute().setValue(true);
                } else {
                    formContext.getControl("header_process_iso_is_decision_rational" + bpfindex).getAttribute().setValue(false);
                }
                if (formContext.getAttribute("iso_decision_date") != null && formContext.getAttribute("iso_decision_date").getValue() != null) {
                    formContext.getControl("header_process_iso_is_decision_date" + bpfindex).getAttribute().setValue(true);
                } else {
                    formContext.getControl("header_process_iso_is_decision_date" + bpfindex).getAttribute().setValue(false);
                }
                break;
            case 863050003:
                bpfindex = "_2";
                if (formContext.getAttribute("iso_decision_type") != null && formContext.getAttribute("iso_decision_type").getValue() != null) {
                    formContext.getControl("header_process_iso_decision" + bpfindex).getAttribute().setValue(true);
                } else {
                    formContext.getControl("header_process_iso_decision" + bpfindex).getAttribute().setValue(false);
                }
                if (formContext.getAttribute("iso_decision_rational") != null && formContext.getAttribute("iso_decision_rational").getValue() != null) {
                    formContext.getControl("header_process_iso_is_decision_rational" + bpfindex).getAttribute().setValue(true);
                } else {
                    formContext.getControl("header_process_iso_is_decision_rational" + bpfindex).getAttribute().setValue(false);
                }
                if (formContext.getAttribute("iso_decision_date") != null && formContext.getAttribute("iso_decision_date").getValue() != null) {
                    formContext.getControl("header_process_iso_is_decision_date" + bpfindex).getAttribute().setValue(true);
                } else {
                    formContext.getControl("header_process_iso_is_decision_date" + bpfindex).getAttribute().setValue(false);
                }
                break;
                break;
            case 863050004:
                bpfindex = "_1";
                if (formContext.getAttribute("iso_decision_type") != null && formContext.getAttribute("iso_decision_type").getValue() != null) {
                    formContext.getControl("header_process_iso_decision" + bpfindex).getAttribute().setValue(true);
                } else {
                    formContext.getControl("header_process_iso_decision" + bpfindex).getAttribute().setValue(false);
                }
                if (formContext.getAttribute("iso_decision_rational") != null && formContext.getAttribute("iso_decision_rational").getValue() != null) {
                    formContext.getControl("header_process_iso_is_decision_rational" + bpfindex).getAttribute().setValue(true);
                } else {
                    formContext.getControl("header_process_iso_is_decision_rational" + bpfindex).getAttribute().setValue(false);
                }
                if (formContext.getAttribute("iso_decision_date") != null && formContext.getAttribute("iso_decision_date").getValue() != null) {
                    formContext.getControl("header_process_iso_is_decision_date" + bpfindex).getAttribute().setValue(true);
                } else {
                    formContext.getControl("header_process_iso_is_decision_date" + bpfindex).getAttribute().setValue(false);
                }

                break;
            default:
                return;
        }

    },

    CreateSettingTask: function (context) {
        var formContext = context.getFormContext ? context.getFormContext() : context;
        var formMode = formContext.ui.getFormType();
        var casetype = formContext.getAttribute("iso_case_type").getValue();
        var tasknotification = formContext.getAttribute("iso_is_notification_task_created").getValue();
        var caseid = formContext.data.entity.getId();
        var lookupcase = caseid.replace("{", "").replace("}", "");
        if (formMode === 2 && tasknotification) {
            IsoFairlens.iso_common.GenerateTask(formContext, lookupcase, "case", casetype, new Date(), false, true);

        }
    },

    GenerateSQRConsideration: function (primarycontrol) {
        var formContext = primarycontrol;
        if (formContext.getAttribute("iso_onsite_review_date") != null && formContext.getAttribute("iso_onsite_review_date").getValue() != null) {
            var caseid = formContext.data.entity.getId();
            var casename = formContext.getAttribute("title").getValue();
            var lookupcase = caseid.replace("{", "").replace("}", "");

            var data = {
                "caseid": lookupcase,
                "casename": casename

            };
            formContext.getAttribute("iso_is_sqr_consideration_generated").setValue(false);
            formContext.ui.clearFormNotification("isqrconsinfo");
            formContext.ui.setFormNotification("SQR Consideration Updation is in Progress", "INFO", "info");
            IsoFairlens.iso_common.GenerateCaseLinkWithSQRCases(formContext, data);
            setTimeout(function () {
                formContext.ui.clearFormNotification("info");
                formContext.getControl("sqr_consideration_grid").refresh();
            }, 5000);
        } else {
            this.OpenAlert("Kindly select Onsite Review Date");
        }
    },

    toggelSICaseSection: function (executionContext, tabObj) {
        var formContext = executionContext.getFormContext();
        var sectpartylistObj = tabObj.sections.get("ORG_CLIENT_CASE_PL_SEC");
        var sectclientObj = tabObj.sections.get("client");
        var recommendationabObj = formContext.ui.tabs.get("recommendation_tab");
        var considersecObj = tabObj.sections.get("case_client_consideration_section");
        if (formContext.getAttribute("iso_case_client_name") != null) {
            formContext.getAttribute("iso_case_client_name").setValue(null);
        }
        if (formContext.getControl("header_process_iso_case_client_name") != null) {
            formContext.getControl("header_process_iso_case_client_name").setVisible(false);
            formContext.getControl("header_process_iso_case_client_name").getAttribute().setValue(null);

        }
        if (formContext.getControl("header_process_iso_incident_date") != null) {
            formContext.getControl("header_process_iso_incident_date").setVisible(true);
            formContext.getControl("header_process_iso_incident_date").getAttribute().setRequiredLevel("required");
        }
        if (formContext.getControl("header_process_iso_chair") != null) {
            formContext.getControl("header_process_iso_chair").setVisible(true);
            formContext.getControl("header_process_iso_chair").getAttribute().setRequiredLevel("required");
            formContext.getControl("header_process_iso_chair").addPreSearch(filterChairContacts);
            formContext.getControl("header_process_iso_chair").getAttribute().disableMru = true;
        }
        if (formContext.getControl("header_process_iso_report_due_date") != null) {
            formContext.getControl("header_process_iso_report_due_date").setVisible(true);
            formContext.getControl("header_process_iso_report_due_date").getAttribute().setRequiredLevel("required");
        }
        if (formContext.getControl("header_process_iso_is_si_subject") != null) {
            formContext.getControl("header_process_iso_is_si_subject").setVisible(true);
        }
        recommendationabObj.setVisible(true);
        sectpartylistObj.setVisible(true);
        considersecObj.setVisible(false);
        sectclientObj.setVisible(false);

        formContext.getControl("iso_initiator").setVisible(false);
        formContext.getControl("iso_case_issue").setVisible(false);
        formContext.getControl("iso_sqr_consideration").setVisible(false);
        formContext.getControl("description").setVisible(false);
        formContext.getControl("iso_historical_iso_case_number").setVisible(false);
        formContext.getControl("iso_service_standards").setVisible(false);

        formContext.getControl("iso_incident_date").setVisible(true);
        formContext.getControl("iso_report_due_date").setVisible(true);
        formContext.getControl("iso_extension_requested").setVisible(true);
        formContext.getControl("iso_chair").setVisible(true);
        formContext.getControl("iso_si_subject").setVisible(true);

        formContext.getAttribute("iso_incident_date").setRequiredLevel("required");
        formContext.getAttribute("iso_report_due_date").setRequiredLevel("required");
        formContext.getAttribute("iso_chair").setRequiredLevel("required");


        formContext.getControl("iso_chair").addPreSearch(filterChairContacts);

        function filterChairContacts() {
            var contactFilter = "<filter type='and'><condition attribute='iso_contact_type' operator='neq' value='863050000'/> " +
                " <condition attribute='statecode' operator='eq' value='0' /> " +
                " </filter > ";
            formContext.getControl("iso_chair").addCustomFilter(contactFilter, "contact");
        }
    },

    // Set default lookup to 'Active Correctional Centres'
    setDefaultCustomerLookupView: function (context) {
        var formContext = context.getFormContext ? context.getFormContext() : context;
        var customerLookup = formContext.getControl("customerid");
        var defaultViewId = "a81fbe6b-86d4-ee11-b84a-00505683fbf4"; // GUID of the 'Active Correctional Centres' view

        // Check if the form is in a state where the default view can be set (i.e., Create or Update)
        if (formContext.ui.getFormType() === 1 || formContext.ui.getFormType() === 2) {
            customerLookup.setDefaultView(defaultViewId);
        }
    },
    OnSISubjectChange: function (context) {
        var formContext = context.getFormContext ? context.getFormContext() : context;
        var formContext = context.getFormContext ? context.getFormContext() : context;
        var caseTypeValue = formContext.getAttribute("iso_case_type").getValue();
        switch (caseTypeValue) {
            case 863050008:
                if (formContext.getAttribute("iso_si_subject") != null && formContext.getAttribute("iso_si_subject").getValue() != null) {
                    formContext.getControl("header_process_iso_is_si_subject").getAttribute().setValue(true);
                } else {
                    formContext.getControl("header_process_iso_is_si_subject").getAttribute().setValue(false);
                }
        }
    },
    toggelTRCaseSection: function (executionContext, tabObj) {
        var formContext = executionContext.getFormContext();
        var sectclientObj = tabObj.sections.get("client");
        var recommendationabObj = formContext.ui.tabs.get("recommendation_tab");
        var considersecObj = tabObj.sections.get("case_client_consideration_section");
        var casereltab = formContext.ui.tabs.get("CASERELATIONSHIP_TAB");

        if (formContext.getAttribute("iso_case_client_name") != null) {
            formContext.getAttribute("iso_case_client_name").setValue(null);
        }
        if (formContext.getControl("header_process_iso_case_client_name") != null) {
            formContext.getControl("header_process_iso_case_client_name").setVisible(false);

            formContext.getControl("header_process_iso_case_client_name").getAttribute().setValue(null);


        }
        if (formContext.getControl("header_process_iso_onsite_review_date") != null) {
            formContext.getControl("header_process_iso_onsite_review_date").setVisible(true);
            formContext.getControl("header_process_iso_onsite_review_date").getAttribute().setRequiredLevel("required");
        }
        if (formContext.getControl("header_process_iso_previous_sqr_case") != null) {
            formContext.getControl("header_process_iso_previous_sqr_case").setVisible(true);
            formContext.getControl("header_process_iso_previous_sqr_case").setLabel("Previous TR Case");
            formContext.getControl("header_process_iso_previous_sqr_case").addPreSearch(filterTRCasesWithCust);
            formContext.getControl("header_process_iso_previous_sqr_case").getAttribute().disableMru = true;


        }

        sectclientObj.setVisible(false);
        recommendationabObj.setVisible(true);
        casereltab.setVisible(false);
        considersecObj.setVisible(false);

        formContext.getControl("iso_initiator").setVisible(false);
        formContext.getControl("iso_case_issue").setVisible(false);
        formContext.getControl("iso_sqr_consideration").setVisible(false);
        formContext.getControl("description").setVisible(false);
        formContext.getControl("iso_historical_iso_case_number").setVisible(false);
        formContext.getControl("iso_service_standards").setVisible(false);


        formContext.getControl("iso_onsite_review_date").setVisible(true);
        formContext.getControl("iso_case_section").setLabel("Review Topic");
        formContext.getControl("iso_description").setVisible(true);
        formContext.getControl("iso_summer_flex").setVisible(true);
        formContext.getAttribute("iso_onsite_review_date").setRequiredLevel("required");
        formContext.getAttribute("iso_description").setRequiredLevel("required");
        formContext.getAttribute("iso_case_section").setRequiredLevel("required");
        formContext.getControl("iso_previous_sqr_case").setVisible(true);
        formContext.getControl("iso_previous_sqr_case").setLabel("Previous TR Case");

        formContext.getControl("iso_previous_sqr_case").addPreSearch(filterTRCasesWithCust);
        var casetype = formContext.getAttribute("iso_case_type").getValue();

        function filterTRCasesWithCust() {
            if (formContext.getAttribute("customerid") != null && formContext.getAttribute("customerid").getValue() != null) {
                var formMode = formContext.ui.getFormType();
                var customerdata = formContext.getAttribute("customerid").getValue();
                var custid = customerdata[0].id.replace("{", "").replace("}", "");
                var custname = customerdata[0].name;
                var encodedItem = encodeURIComponent(custname);
                if (formMode >= 2) //update mode
                {

                    var casename = formContext.getAttribute("title").getValue();
                    var encodedcasename = encodeURIComponent(casename);
                    var caseid = formContext.data.entity.getId();
                    var lookupcase = caseid.replace("{", "").replace("}", "");
                    var trcaseFilter = "<filter type='and'><condition attribute='iso_case_type' operator='eq' value='" + casetype + "'/>  " +
                        "      <condition attribute='customerid' operator='eq' value='" + custid + "' uiname='" + encodedItem + "' uitype='account' /> " +
                        "  <condition attribute='incidentid' operator='ne' uiname='" + encodedcasename + "' uitype='incident' value='" + lookupcase + "' /> " +
                        "  </filter > ";
                    formContext.getControl("iso_previous_sqr_case").addCustomFilter(trcaseFilter, "incident");
                    formContext.getControl("header_process_iso_previous_sqr_case").addCustomFilter(trcaseFilter, "incident");
                } else {
                    var trcaseFilter = "<filter type='and'><condition attribute='iso_case_type' operator='eq' value='" + casetype + "'/>  " +
                        "      <condition attribute='customerid' operator='eq' value='" + custid + "' uiname='" + encodedItem + "' uitype='account' /> " +
                        "  </filter > ";
                    formContext.getControl("iso_previous_sqr_case").addCustomFilter(trcaseFilter, "incident");
                    formContext.getControl("header_process_iso_previous_sqr_case").addCustomFilter(trcaseFilter, "incident");
                }


            } else {
                var trcaseFilter = "<filter type='and'><condition attribute='iso_case_type' operator='eq' value='" + casetype + "'/>  </filter >";
                formContext.getControl("iso_previous_sqr_case").addCustomFilter(trcaseFilter, "incident");
                formContext.getControl("header_process_iso_previous_sqr_case").addCustomFilter(trcaseFilter, "incident");
            }
        }
        if (casetype === 863050007) {
            formContext.getAttribute("customerid").addOnChange(this.sqrPreviousCaseFilter);
        }

    },
    onSQRPreviousCase: function (context) {
        var formContext = context.getFormContext ? context.getFormContext() : context;
        formContext.getAttribute("iso_last_review_date").setValue(null);
        if (formContext.getAttribute("iso_previous_sqr_case") != null && formContext.getAttribute("iso_previous_sqr_case").getValue() != null) {
            var prevSQRCase = formContext.getAttribute("iso_previous_sqr_case").getValue();
            var prevcaseId = prevSQRCase[0].id.replace("{", "").replace("}", ""); // Clean GUID
            Xrm.WebApi.retrieveRecord("incident", prevcaseId, "?$select=iso_onsite_review_date").then(
                function (caseRecord) {

                    var reviewdate = caseRecord["iso_onsite_review_date"];
                    if (reviewdate != null) {
                        formContext.getAttribute("iso_last_review_date").setValue(new Date(reviewdate));
                    }

                },
                function (error) {
                    console.log("Error retrieving Contact: " + error.message);
                }
            );
        }
    },
    CheckIfAdmin: function (primarycontrol) {
        var gcontext = Xrm.Utility.getGlobalContext();
        var loggedUserRoles = gcontext.userSettings.roles;
        var isAdmin = true;
        loggedUserRoles.forEach(function hasRoleName(item, index) {
            if ((item.name == "ISO FairLens - Admin") || (item.name == "System Administrator") || (item.name == "ISO FairLens - Superuser") && isAdmin) {

                isAdmin = false;
            }
        });
        if (!isAdmin) {
            return true;
        }
        else {
            return false;
        }
    }

}
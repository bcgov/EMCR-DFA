function switchForm(executionContext) {
    var formContext = executionContext.getFormContext();
    var formType = formContext.ui.getFormType();

    var form_mainlocalgov = "448093dc-5406-4163-b517-fcee5ab55d48";
    var form_main = "a1818f1e-2eb3-41bc-9885-8f6cfd66c15f";
    var form_datafix_BCLocalGov = "bf51d520-f8ad-4fcc-bb2a-4876d509f1d3";
    var form_datafix_LocalGov ="ce46f33a-9bf4-4635-9002-1809be39a411";

    var createdOnPortal = false; // Default value
    if (formContext.getAttribute("dfa_createdonportal")) {
        createdOnPortal = formContext.getAttribute("dfa_createdonportal").getValue();
    }

    console.log("createdOnPortal:", createdOnPortal);
    console.log("formType:", formType);

    var currentForm = formContext.ui.formSelector.getCurrentItem().getId();
    var listOfAvailableForms = formContext.ui.formSelector.items.get();

    // Hide forms based on the value of createdOnPortal
    listOfAvailableForms.forEach(function (item) {
        if (createdOnPortal && item.getLabel().includes("Main")) {
            item.setVisible(false);
        } else if (!createdOnPortal && item.getLabel().includes("Portal")) {
            item.setVisible(false);
        }
    });

    // Return early if the form is already correct
    if (createdOnPortal && (currentForm === form_mainlocalgov || currentForm === form_datafix_BCLocalGov) && formType === 2) {
        return;
    }

    if (!createdOnPortal && (currentForm === form_main || currentForm === form_datafix_LocalGov) && formType === 2) {
        return;
    }
    // Navigate to the correct form based on createdOnPortal
    if (createdOnPortal && currentForm !== form_mainlocalgov && formType === 2) {
        console.log("switch localgov");
        listOfAvailableForms.forEach(function (element) {
            if (element.getId() === form_mainlocalgov) {
                element.navigate();
            }
        });
    } else if (!createdOnPortal && currentForm !== form_main && formType === 2) {
        console.log("switch main");
        listOfAvailableForms.forEach(function (element) {
            if (element.getId() === form_main) {
                element.navigate();
            }
        });
    }
}



function enableControls(executionContext) {
    var formContext = executionContext.getFormContext();
    const controlIds = [
        "header_process_dfa_assignedtoevaluator",
        "header_process_dfa_underreviewadditionalinforequested",
        "header_process_dfa_approvalpendinginprogress",
        "header_process_dfa_approvalpendingadditionalinforequested",
        "header_process_dfa_requiresdecisionnote",
        "header_process_dfa_waitingonpolicylegal",
        "header_process_dfa_compliancecheckccadditionalinforequested",
        "header_process_dfa_compliancecheckeligibleprojectscope",
        "header_process_dfa_compliancecheckbackupdocuments",
        "header_process_dfa_compliancecheckpreexistingcondition",
        "header_process_dfa_compliancecheckstampalldocs",
        "header_process_dfa_compliancecheckgenerateapprovalletter",
        "header_process_dfa_compliancecheckadjustmentsforigbletter",
        "header_process_dfa_qualifiedreceiverqradditionalinforeques",
        "header_process_dfa_expenseauthorityeaadditionalinforequest",
        "header_process_dfa_projectdecision",
        "header_process_dfa_emcrapprovalcomments",
        "header_process_dfa_projectdecision_1",
        "dfa_createdonportal",
        "header_process_dfa_submitted",
        "header_process_dfa_assignedtoadjudicator",
        "header_process_dfa_adjudicatoradditionalinforequested",
        "header_process_dfa_adjudicatoreligibleprojectscope",
        "header_process_dfa_adjudicatorbackupdocuments",
        "header_process_dfa_adjudicatorpreexistingcondition",
        "header_process_dfa_adjudicatorstampalldocs",
        "header_process_dfa_adjudicatorgenerateapprovalletter",
        "header_process_dfa_adjudicatoradjustmentsforigbletter",
        "header_process_dfa_projectnumber"
    ];

    controlIds.forEach(function(controlId) {
        let control = formContext.getControl(controlId);
        if (control != null) {
            control.setDisabled(false);
        }
    });
}


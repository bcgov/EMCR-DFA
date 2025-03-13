function onLoad(executionContext) {
    var formContext = executionContext.getFormContext();

    // Get the value of the boolean field (dfa_adjustmentclaimunderreview)
    var underreview = formContext.getAttribute("dfa_adjustmentclaimunderreview").getValue();

    // Get the value of the option set field (dfa_decision)
    var decision = formContext.getAttribute("dfa_decision").getValue();

    // Get the value of the boolean field (dfa_isadjustmentclaim)
    var adjustment = formContext.getAttribute("dfa_isadjustmentclaim").getValue();
    // Check the conditions: dfa_adjustmentclaimunderreview is true and dfa_decision is null
    if (underreview === true && decision === null && adjustment === false) {
        // Display a form notification
        formContext.ui.setFormNotification("There is an adjustment claim pending decision related to the same project, and it has to be processed before this claim's decision gets modified. Meanwhile, the claim can be proceeeded but not approved/rejected!", "INFO", "formLockedNotification");

        // Optionally, you could disable the entire form or specific fields
        // formContext.ui.controls.forEach(function (control) {
        //     control.setDisabled(true);
        // });
    }
}

// Attach the onLoad function to the form's OnLoad event
// Make sure to register the function in the form's event handler in the Dynamics 365 interface

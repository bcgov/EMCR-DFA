function switchForm(executionContext) {
    var formContext = executionContext.getFormContext();
    var formType = formContext.ui.getFormType();

    var form_mainlocalgov = "728de221-da7e-4806-962c-c2f4a709efa1";
    var form_main = "63c1e447-2cf2-4631-8409-f2434ffa54f2";
    var form_datafix_LocalGov = "00e9387d-19d0-4c8d-b7b6-99fc87dbd705";
    var form_datafix_BCLocalGov = "7f87f279-1e30-49e6-96e4-51cbd85f0d32";

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
            console.log("hide main");
        } else if (!createdOnPortal && item.getLabel().includes("Portal")) {
            item.setVisible(false);
            console.log("hide portal");
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


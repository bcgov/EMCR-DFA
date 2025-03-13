function switchForm(executionContext) {
    var formContext = executionContext.getFormContext();
    var formType = formContext.ui.getFormType();

    var form_mainlocalgov = "f0b5d034-d649-4dd7-b720-6102e5a6513e";
    var form_main = "852826c0-e4cd-4d20-a7b8-deca17146830";
    var form_datafix_BCLocalGov = "eadf2252-e790-4e28-bd2b-e65e86de7b1f";
    var form_datafix_LocalGov = "d4a56b81-70f3-4dd3-b66a-99ba2d97af20";

    var createdOnPortal = false;
    if (formContext.getAttribute("dfa_createdonportal")) {
        createdOnPortal = formContext.getAttribute("dfa_createdonportal").getValue();
    }

    console.log("createdOnPortal:", createdOnPortal);
    console.log("formType:", formType);

    var currentForm = formContext.ui.formSelector.getCurrentItem();
    if (!currentForm) {
        console.log("No current form selected.");
        return;
    }
    var currentFormId = currentForm.getId();
    var listOfAvailableForms = formContext.ui.formSelector.items.get();

    if (!listOfAvailableForms || listOfAvailableForms.length === 0) {
        console.log("No available forms found.");
        return;
    }

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
    if (createdOnPortal && (currentFormId === form_mainlocalgov || currentFormId === form_datafix_BCLocalGov) && formType === 2) {
        console.log("Form already correct for LocalGov.");
        return;
    }

    if (!createdOnPortal && (currentFormId === form_main || currentFormId === form_datafix_LocalGov) && formType === 2) {
        console.log("Form already correct for Main.");
        return;
    }

    // Navigate to the correct form based on createdOnPortal
    if (createdOnPortal && currentFormId !== form_mainlocalgov && formType === 2) {
        console.log("Switching to LocalGov form.");
        listOfAvailableForms.forEach(function (element) {
            if (element.getId() === form_mainlocalgov) {
                element.navigate();
            }
        });
    } else if (!createdOnPortal && currentFormId !== form_main && formType === 2) {
        console.log("Switching to Main form.");
        listOfAvailableForms.forEach(function (element) {
            if (element.getId() === form_main) {
                element.navigate();
            }
        });
    }
}

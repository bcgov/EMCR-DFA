function switchForm(executionContext) {
	console.log("entering:");

	var formContext = executionContext.getFormContext();
	var formType = formContext.ui.getFormType();
	var form_mainlocalgov = "67b6dd4f-a099-4a06-a923-c121e872f188";
	var form_main = "c89efb5e-733a-4b00-b287-873ddf906010";
        var applicantTypeValue =  formContext.getAttribute("dfa_applicanttype").getValue();
	console.log(applicantTypeValue);


	if (formContext.data.entity.getEntityName() == "dfa_appapplication" && applicantTypeValue == 222710005) {
		var listOfAvailableForms = formContext.ui.formSelector.items.get();
		var currentForm = formContext.ui.formSelector.getCurrentItem().getId();
		var createdOnPortal = false;

		if (formContext.getAttribute("dfa_createdonportal")) {
					createdOnPortal = formContext.getAttribute("dfa_createdonportal").getValue();
		};
		console.log("createdonportal:",createdOnPortal);
		console.log("formType:",formType);
		console.log("currentForm :",currentForm );

			if (createdOnPortal == true && currentForm != form_mainlocalgov && formType == 2) {
				console.log("switch to localgov");
				listOfAvailableForms.forEach(element => {
					if (element.getId() == form_mainlocalgov)
						element.navigate();
				});
			} else if ((createdOnPortal == false || createdOnPortal == null)  && currentForm != form_main && formType == 2) {
				console.log("switch to main");
				listOfAvailableForms.forEach(element => {
					if (element.getId() == form_main)
						element.navigate();
				});
			}
 
	}
}

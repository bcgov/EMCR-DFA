function SwitchBPF(primaryControl) {
	var formContext = primaryControl;
	if (formContext.data.entity.getEntityName() == 'incident') {
		var activeProcess = formContext.data.process.getActiveProcess();
		if (formContext.data.process.getActiveProcess() != null) {
			var currentBpfID = formContext.data.process.getActiveStage().getId();
			if (formContext.getAttribute('statuscode')) {
				var caseStatus = formContext.getAttribute('statuscode').getValue();
				var stat = formContext.data.process.getStatus();
				if (caseStatus == '5' && formContext.data.process.getStatus() != 'active') {
					var currentBPFInstanceID = formContext.data.process.getInstanceId();
					formContext.data.process.setStatus("active");
					var Develop_Files = "47d2f52d-6667-4557-9935-e1081c6f050e";
					var Checking_Criteria = "c2db4a71-2df2-45c2-863e-40f8e104b415";
					var Assess_Damage = "7ba026a3-7c9f-4295-beec-fefb479436d4";
					var Review_Report = "2f6a30d2-608a-446a-9eee-4e8f1fa04020";
					var Close_Pay = "76d89669-ecbd-4cdf-a607-41f75b9d3597";
					var App_Received = "ac84d950-ce80-4e80-b2fe-8f7982af3107";
					var App_In_Prog = "63242c20-36a9-4783-af7b-966d88412bbd";

					var entity = {};
					entity['activestageid@odata.bind'] = "/processstages(" + App_Received + ")";
					entity.traversedpath = Develop_Files + "," + Checking_Criteria + "," + Assess_Damage + "," + Review_Report + "," + Close_Pay + "," + App_Received;
					var req = new XMLHttpRequest();
					req.open('PATCH', `${Xrm.Utility.getGlobalContext().getClientUrl()}/api/data/v9.1/dfa_casebusinessprocesses(${currentBPFInstanceID})`, true);
					req.setRequestHeader('OData-MaxVersion', '4.0');
					req.setRequestHeader('OData-Version', '4.0');
					req.setRequestHeader('Accept', 'application/json');
					req.setRequestHeader('Content-Type', 'application/json; charset=utf-8');
					req.onreadystatechange = function () {
						if (this.readyState === 4) {
							req.onreadystatechange = null;

							if (this.status === 204) {
								console.log('Success updating');
							} else {
								console.log('Error');
							}
						}
					};
					setTimeout(() => {
						req.send(JSON.stringify(entity));
					}, 1000);
				}
			}
		}
	}
}




function MoveToAssessDamage(executionContext) {
	var formContext = executionContext.getFormContext(); // get formContext

	if (formContext.data.entity.getEntityName() == 'incident') {
		//dfa_finalappealstatus	222710002

		var FinalAppealStatus = formContext.getAttribute('dfa_finalappealstatus').getValue();

		if (FinalAppealStatus == '222710002') {
			formContext.data.process.setStatus("active");

			var currentBPFInstanceID = formContext.data.process.getInstanceId();
			var Develop_Files = "47d2f52d-6667-4557-9935-e1081c6f050e";
			var Checking_Criteria = "c2db4a71-2df2-45c2-863e-40f8e104b415";
			var Assess_Damage = "7ba026a3-7c9f-4295-beec-fefb479436d4";
			var Review_Report = "2f6a30d2-608a-446a-9eee-4e8f1fa04020";
			var Close_Pay = "76d89669-ecbd-4cdf-a607-41f75b9d3597";
			var App_Received = "ac84d950-ce80-4e80-b2fe-8f7982af3107";
			var App_In_Prog = "63242c20-36a9-4783-af7b-966d88412bbd";

			var entity = {};
			entity['activestageid@odata.bind'] = "/processstages(" + Assess_Damage + ")";
			entity.traversedpath = Develop_Files + "," + Checking_Criteria + "," + Assess_Damage;
			var req = new XMLHttpRequest();
			req.open('PATCH', `${Xrm.Utility.getGlobalContext().getClientUrl()}/api/data/v9.1/dfa_casebusinessprocesses(${currentBPFInstanceID})`, true);
			req.setRequestHeader('OData-MaxVersion', '4.0');
			req.setRequestHeader('OData-Version', '4.0');
			req.setRequestHeader('Accept', 'application/json');
			req.setRequestHeader('Content-Type', 'application/json; charset=utf-8');
			req.onreadystatechange = function () {
				if (this.readyState === 4) {
					req.onreadystatechange = null;

					if (this.status === 204) {
						console.log('Success updating');
					} else {
						console.log('Error');
					}
				}
			};
			setTimeout(() => {
				req.send(JSON.stringify(entity));
				formContext.data.save();
				//         formContext.data.refresh();
			}, 1000);
		}
	}


}
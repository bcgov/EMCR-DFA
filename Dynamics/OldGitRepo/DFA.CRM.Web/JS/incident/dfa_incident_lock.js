// JavaScript source code
// Added as separate because another developer is working on the main one.  Will merge them when it is completed

if (typeof DFA === "undefined") {
    var DFA = {};
}

if (typeof DFA.Jscripts === "undefined") {
    DFA.Jscripts = {};
}

DFA.Jscripts.CaseLock = {
    incidentLock_ExecutionContext: null,

    OnLoad: function (executionContext) {
        this.incidentLock_ExecutionContext = executionContext;
        var isUCI = Xrm.Internal.isUci();
        if (!isUCI) {
            return; // Don't run anything else if not UCI.
        }
        var formContext = executionContext.getFormContext();
        var entityName = formContext.data.entity.getEntityName();
        formItem = formContext.ui.formSelector.getCurrentItem();
        if (formItem != null && formItem.getLabel().includes("DataFix")) {
            return; // Exit and don't run any 
        }
        var formType = formContext.ui.getFormType();
        if (formType == 2) {
            if (entityName == "incident") {
                DFA.Jscripts.CaseLock.PromptIfRecordLockAndNotCurrentUser(executionContext);
            }
            else {
                DFA.Jscripts.CaseLock.LockControlIfNotCurrentUser(executionContext);
            }
        }
    },

    // For Ribbon Display Rule
    CheckIfRecordIsLock: function () {
        var checkOutByLookupValue = dfa_Utility.getAttributeValue(this.incidentLock_ExecutionContext, "dfa_checkoutbyid");
        var isLock = (checkOutByLookupValue != null);
        return isLock;
    },

    CheckIfRecordLockByCurrentUser: function () {
        var checkOutByLookupValue = dfa_Utility.getAttributeValue(this.incidentLock_ExecutionContext, "dfa_checkoutbyid");
        var isLockByCurrentUser = false;
        if (checkOutByLookupValue != null) {
            var userSettings = Xrm.Utility.getGlobalContext().userSettings;
            var currentuserid = userSettings.userId.replace("{", "").replace("}", "").toLowerCase();
            var checkOutUserId = checkOutByLookupValue[0].id.replace("{", "").replace("}", "").toLowerCase();
            isLockByCurrentUser = currentuserid == checkOutUserId;
        }
        return isLockByCurrentUser;
    },

    // For Child Entity Ribbon Enable/Disable
    CheckIfCheckedOutByCurrentUser: function () {
        var formContext = this.incidentLock_ExecutionContext.getFormContext();
        formItem = formContext.ui.formSelector.getCurrentItem();
        if (formItem != null && formItem.getLabel().includes("DataFix")) {
            return true; // Exit and don't run any 
        }
        var entityName = formContext.data.entity.getEntityName();
        var isCheckOutByCurrentUser = false;
        var checkOutByLookupValue = null;
        var checkOutById = null;
        switch (entityName) {
            case "incident":
                checkOutByLookupValue = formContext.getAttribute("dfa_checkoutbyid").getValue();
                checkOutById = checkOutByLookupValue[0].id.replace("{", "").replace("}", "").toLowerCase();
                break;
            case "dfa_project":
            case "dfa_appeal":
            case "dfa_additionalapplicant":
            case "dfa_applicationassignee":
            case "dfa_cheque":
            case "dfa_claimsummary":
            case "dfa_othercontact":
            case "dfa_projectstatusreport":
            case "dfa_projectclaim":
            case "dfa_occupant":
            case "dfa_cleanuplog":
            case "dfa_damageditem":
            // Other children tab which were hidden and did not add previously.  Occupants, Clean Up Log, Damaged Items, 
                var caseValue = formContext.getAttribute("dfa_caseid").getValue();
                if (caseValue == null) {
                    return isCheckOutByCurrentUser; // Nothing to be based for the search
                }
                var filter = "?$select=_dfa_checkoutbyid_value";
                var caseId = caseValue[0].id.replace('{', '').replace('}', '');
                var caseRecord = DFA.Jscripts.CaseLock.retrieveRecordCustom(caseId, "incident", filter);
                checkOutByLookupValue = caseRecord._dfa_checkoutbyid_value;
                checkOutById = checkOutByLookupValue;
                break;
        }
        if (checkOutByLookupValue != null) {
            var userSettings = Xrm.Utility.getGlobalContext().userSettings;
            var currentuserid = userSettings.userId.replace("{", "").replace("}", "").toLowerCase();
            if (checkOutById == currentuserid) {
                isCheckOutByCurrentUser = true;
            }
        }
        return isCheckOutByCurrentUser;
    },


    // Call On Load
    PromptIfRecordLockAndNotCurrentUser: function (executionContext) {
        var checkOutByLookupValue = dfa_Utility.getAttributeValue(this.incidentLock_ExecutionContext, "dfa_checkoutbyid");
        if (checkOutByLookupValue == null) {
            var message = "Please pick the record before starting to work on it.  It will now be switched to read-only mode.";
            dfa_Utility.showMessage(message);
            dfa_Utility.DisableAllFields(executionContext);
            return;
        }
        if (checkOutByLookupValue != null) {
            var checkOutById = checkOutByLookupValue[0].id.replace("{", "").replace("}", "").toLowerCase();
            var userSettings = Xrm.Utility.getGlobalContext().userSettings;
            var currentuserid = userSettings.userId.replace("{", "").replace("}", "").toLowerCase();
            if (checkOutById != currentuserid) {
                var message2 = "This record is being worked by " + checkOutByLookupValue[0].name + ".  It will now be switched into read-only mode.";
                dfa_Utility.showMessage(message2);
                dfa_Utility.DisableAllFields(executionContext);
            }
        }
    },

    LockControlIfNotCurrentUser: function (executionContext) {
        var isCheckOutByCurrentUser = DFA.Jscripts.CaseLock.CheckIfCheckedOutByCurrentUser();
        if (!isCheckOutByCurrentUser) {
            dfa_Utility.DisableAllFields(executionContext);
        }
    },

    // For Ribbon Button Call
    CheckOutRecord: function () {
        var formContext = this.incidentLock_ExecutionContext.getFormContext();
        var entityId = formContext.data.entity.getId().replace('{', '').replace('}', '');
        var entityName = formContext.data.entity.getEntityName();
        var userSettings = Xrm.Utility.getGlobalContext().userSettings;
        var currentuserid = userSettings.userId.replace("{", "").replace("}", "").toLowerCase();
        var currentuserName = userSettings.userName;
        var lookupArray = new Array();
        lookupArray[0] = new Object();
        // Treat the entity record to include curly braces if needed
        if (currentuserid.indexOf("{") === -1) {
            currentuserid = "{" + currentuserid + "}";
        }
        lookupArray[0].id = currentuserid;
        lookupArray[0].name = currentuserName;
        lookupArray[0].entityType = "systemuser";
        var checkOutByAttribute = formContext.getAttribute("dfa_checkoutbyid");
        if (checkOutByAttribute != null) {
            checkOutByAttribute.setValue(lookupArray);
            formContext.data.entity.save();
            setTimeout(function () {
                dfa_Utility.RefreshCurrentForm(entityName, entityId);
            }, 2000);
        }
    },

    // For Ribbon Button Call
    ReleaseRecord: function () {
        var formContext = this.incidentLock_ExecutionContext.getFormContext();
        var entityId = formContext.data.entity.getId().replace('{', '').replace('}', '');
        var entityName = formContext.data.entity.getEntityName();
        var checkOutByAttribute = formContext.getAttribute("dfa_checkoutbyid");
        if (checkOutByAttribute != null) {
            checkOutByAttribute.setValue(null);
            formContext.data.entity.save();
            setTimeout(function () {
                dfa_Utility.RefreshCurrentForm(entityName, entityId);
            }, 2000);
            
        }
    },

    // Call On Save
    PreventSaveIfLockButNotByCurrentUser: function (executionContext) {
        var formContext = executionContext.getFormContext();
        formItem = formContext.ui.formSelector.getCurrentItem();
        if (formItem != null && formItem.getLabel().includes("DataFix")) {
            return; // Exit and don't run any 
        }
        var formType = formContext.ui.getFormType();
        if (formType == 2) {
            var checkOutByLookupValue = dfa_Utility.getAttributeValue(this.incidentLock_ExecutionContext, "dfa_checkoutbyid");
            if (checkOutByLookupValue == null) {
                //dfa_Utility.showMessage("This record is locked because it has not been picked by a user. Saving is not allowed.");
                //executionContext.getEventArgs().preventDefault();
                return; // Ignore if there is no lock
            }
            if (DFA.Jscripts.CaseLock.CheckIfRecordLockByCurrentUser() == false) {
                dfa_Utility.showMessage("This record is checked out by another user and saving is not allowed.");
                executionContext.getEventArgs().preventDefault();
            }
        }
    },
    getPluralEntityLogicalName: function (entityLogicalName) {

        if (!entityLogicalName) {
            alert("entityLogicalName is required.");
            return;
        }

        // Treat entity logical name from legacy query.
        // Legacy query uses AccountSet <entityLogicalName>Set for the OrganizationData.svc 
        var last3Characters = entityLogicalName.substring(entityLogicalName.length - 3);
        if (last3Characters == "Set") {
            entityLogicalName = entityLogicalName.substring(0, entityLogicalName.length - 3);
        }

        // new web api takes all entity logical name in lower case only
        entityLogicalName = entityLogicalName.toLowerCase();

        var sOrES = "s";
        var lastCharacterOfEntitySchemaName = entityLogicalName.substring(entityLogicalName.length - 1);
        if (lastCharacterOfEntitySchemaName == "s") {
            sOrES = "es";
        }
        if (lastCharacterOfEntitySchemaName == "y") {
            sOrES = "ies";
        }
        return entityLogicalName + sOrES;
    },
    retrieveRecordCustom: function (id, entityLogicalName) {
        //id is required
        if (!id) {
            alert("record id is required.");
            return;
        }
        //entityLogicalName is required, i.e. "account"
        if (!entityLogicalName) {
            alert("entityLogicalName is required.");
            return;
        }
        // Remove the curly braces, if any
        id = id.replace('{', '').replace('}', '');
        var entityPluralName = DFA.Jscripts.CaseLock.getPluralEntityLogicalName(entityLogicalName);
        var returnValue;
        //Build the URI
        var odataUri;
        var globalContext = Xrm.Utility.getGlobalContext();
        // Helper Methods and Variables

        //Retrieve the server url, which differs on-premise from on-line and 
        //shouldn't be hard-coded.
        var serverUrl = globalContext.getClientUrl();
        if (serverUrl.match(/\/$/)) {
            serverUrl = serverUrl.substring(0, serverUrl.length - 1);
        }
        //The XRM OData end-point
        // OrganizationData.Svc is obsoleted
        // var ODATA_ENDPOINT = "/XRMServices/2011/OrganizationData.svc/";
        var WEBAPI_ENDPOINT = "/api/data/v9.0/";
        // will become something like this
        // https://DynamicsURL/api/data/v9.0/accounts(<guid>)
        odataUri = serverUrl + WEBAPI_ENDPOINT + entityPluralName + "(" + id + ")";

        // Load JQuery for AJAX call, if not already
        // Dynamics has JQuery loaded on the form somewhere
        if (typeof ($) === 'undefined') {
            $ = parent.$;
            jQuery = parent.jQuery;
        }

        $.ajax({
            type: "GET",
            async: false,
            contentType: "application/json; charset=utf-8",
            datatype: "json",
            url: odataUri,
            beforeSend: function (XMLHttpRequest) {
                XMLHttpRequest.setRequestHeader("Accept", "application/json");
            },
            success: function (data, textStatus, XmlHttpRequest) {
                returnValue = data;
            },
            error: function (XmlHttpRequest, textStatus, errorThrown) {
                // Do Nothing
            }
        });
        return returnValue;
    },
};
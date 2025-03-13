
var CrmJS;
(function (CrmJS) {
    var Utility = /** @class */ (function () {
        function Utility() {
            this._crmConfig = "";
            this._crmAPI = "";
        }
        Utility.prototype.getRecordId = function (executionContext) {
            var functionName = "getRecordId";
            try {
                var formContext = executionContext.getFormContext();
                var entityId = formContext.data.entity.getId().replace('{', '').replace('}', '');
                return entityId;
            }
            catch (ex) {
                dfa_Utility.throwError(functionName, ex);
            }
        };
        Utility.prototype.setRequiredLevel = function (executionContext, requiredLevel, attribute) {
            /// <summary>
            /// This function is used to set Required Level of the attribute passed to it.
            /// </summary>
            /// <param name="attribute" type="string">
            /// <param name="attribute" type="string">
            /// The attribute that needs to be validated.
            /// </param>
            /// <returns type="void" />
            var functionName = "setRequiredLevels";
            var formContext = executionContext.getFormContext();
            try {
                var splitAttribute = attribute.split(",");
                for (var fieldName in splitAttribute) {
                    var theAttribute = formContext.getAttribute(splitAttribute[fieldName]);
                    if (dfa_Utility.isValid(theAttribute)) {
                        theAttribute.setRequiredLevel(requiredLevel);
                    }
                }
            }
            catch (ex) {
                dfa_Utility.throwError(functionName, ex);
            }
        };
        Utility.prototype.setBPFRequiredLevel = function (executionContext, requiredLevel, attribute)
        {
            /// <summary> Added by Forrest on Sep,2022
            /// This function is used to set Required Level of the attribute in business process flow stages.
            /// </summary>
            /// <param name="attribute" type="string">
            /// <param name="attribute" type="string">
            /// The attribute that needs to be validated.
            /// </param>
            /// <returns type="void" />
            var functionName = "setBPFRequiredLevels";
            var formContext = executionContext.getFormContext();
            try
            {
                var splitAttribute = attribute.split(",");
                for (var fieldName in splitAttribute)
                {
                    var theAttribute = formContext.getControl(splitAttribute[fieldName]).getAttribute();
                    if (dfa_Utility.isValid(theAttribute))
                    {
                        theAttribute.setRequiredLevel(requiredLevel);
                    }
                }
            }
            catch (ex)
            {
                dfa_Utility.throwError(functionName + " >> " + attribute, ex);
            }
        };
        Utility.prototype.clearFieldData = function (executionContext, attribute) {
            /// <summary>
            /// This function is used to clear the field data.
            /// </summary>       
            /// <param name="attribute" type="string">
            /// The attribute that needs to be validated.
            /// </param>          
            var functionName = "clearFieldData >>";
            var formContext = executionContext.getFormContext();
            try {
                var splitAttribute = attribute.split(",");
                for (var fieldName in splitAttribute) {
                    var theAttribute = formContext.getAttribute(splitAttribute[fieldName]);
                    if (dfa_Utility.isValid(theAttribute)) {
                        theAttribute.setValue(null);
                    }
                }
            }
            catch (ex) {
                dfa_Utility.throwError(functionName, ex);
            }
        };
        Utility.prototype.getFormType = function (executionContext) {
            /// <summary>
            /// This function is used to get the form type.
            /// </summary>                  
            var formType;
            try {
                var formContext = executionContext.getFormContext();
                formType = formContext.ui.getFormType();
            }
            catch (ex) {
                dfa_Utility.throwError("getFormType >>", ex);
            }
            return formType;
        };
        Utility.prototype.enableDisable = function (executionContext, isDisable, attribute) {
            /// <summary>
            /// This function executes on Load of Interaction Type Entity and set disability for Parameters passed to it.
            /// </summary>
            /// <param name="attribute" type="boolean">
            /// <param name="attribute" type="string">
            /// The attribute that needs to be validated
            /// </param>
            /// <returns type="void" />
            var functionName = "enableDisable";
            var formContext = executionContext.getFormContext();
            var splitAttribute;
            try {
                splitAttribute = attribute.split(",");
                for (var fieldName in splitAttribute) {
                    //To check atrributes exist or not
                    var attributeCtrl = formContext.getControl(splitAttribute[fieldName]);
                    if (dfa_Utility.isValid(attributeCtrl)) {
                        //To Enable/Disable Attributes/Webresources
                        attributeCtrl.setDisabled(isDisable);
                    }
                }
            }
            catch (e) {
                dfa_Utility.throwError(functionName, e);
            }
        };
        Utility.prototype.refreshWebResource = function (executionContext, webResourceName) {
            /// <summary>
            /// This functionrefresh the web resource which is passed as a Parameter to it.
            /// </summary>
            /// <param name="attribute" type="string">
            /// The attribute that needs to be refresh
            /// </param>
            /// <returns type="void" />
            var functionName = "refreshWebResource";
            var formContext = executionContext.getFormContext();
            var splitResource;
            try {
                //validate web resource
                if (!dfa_Utility.isValid(webResourceName)) {
                    return;
                }
                splitResource = webResourceName.split(",");
                if (!dfa_Utility.isValid(formContext)) {
                    return;
                }
                //refresh web resource
                for (var webresource in splitResource) {
                    //get web resource control
                    var webResourceControl = formContext.getControl(splitResource[webresource]);
                    if (!dfa_Utility.isValid(webResourceControl)) {
                        return;
                    }
                    //get source of web resource
                    var src = webResourceControl.getSrc();
                    //set source of web resource to null
                    webResourceControl.setSrc(null);
                    //validate source
                    if (!dfa_Utility.isValid(src)) {
                        return;
                    }
                    //set source of web resource to src
                    webResourceControl.setSrc(src);
                }
            }
            catch (e) {
                dfa_Utility.throwError(functionName, e);
            }
        };
        Utility.prototype.showHide = function (executionContext, isVisible, attribute) {
            /// <summary>
            /// This function executes on Load of Interaction Type Entity and set Visibilty for Parameters passed to it.
            /// </summary>
            /// <param name="attribute" type="boolean">
            /// <param name="attribute" type="string">
            /// The attribute that needs to be validated.
            /// </param>
            /// <returns type="void" />
            var functionName = "showHide >>";
            var splitTabSection;
            var tabname = "";
            var sectionname = "";
            try {
                var formContext = executionContext.getFormContext();
                var splitAttribute = attribute.split(",");
                for (var fieldName in splitAttribute) {
                    fieldName = fieldName.trim();
                    //To check Attributes/WebResources exists or not.
                    var theControl = formContext.getControl(splitAttribute[fieldName]);
                    var theTab = formContext.ui.tabs.get(splitAttribute[fieldName]);
                    if (dfa_Utility.isValid(theControl)) {
                        //To hide Attributes/Webresources
                        theControl.setVisible(isVisible);
                    }
                    ////To check Tab exists or not
                    else if (dfa_Utility.isValid(theTab)) {
                        //To hide Tabs
                        theTab.setVisible(isVisible);
                    }
                    //To hide & show Tab Sections.
                    else {
                        if (splitAttribute[fieldName].search(":") > 1) {
                            splitTabSection = splitAttribute[fieldName].split(":");
                            tabname = splitTabSection[0];
                            sectionname = splitTabSection[1];
                            var sectionTab = formContext.ui.tabs.get(tabname);
                            if (dfa_Utility.isValid(sectionTab)) {
                                //To hide section
                                var theSection = sectionTab.sections.get(sectionname);
                                if (dfa_Utility.isValid(theSection)) {
                                    theSection.setVisible(isVisible);
                                }
                            }
                        }
                    }
                }
            }
            catch (ex) {
                dfa_Utility.throwError(functionName, ex);
            }
        };
        Utility.prototype.isValid = function (attribute) {
            /// <summary>
            /// This function is used to validate the attribute passed to it.
            /// </summary>
            /// <param name="attribute" type="any">
            /// The attribute that needs to be validated.
            /// </param>
            /// <returns type="boolean" />
            var functionName = "isValid";
            var isValid = false;
            try {
                //Validate the attribute and return True or False accordingly
                if (attribute != null && attribute != undefined && attribute != "undefined" && attribute != "null" && attribute != "")
                    isValid = true;
            }
            catch (ex) {
                dfa_Utility.throwError(functionName, ex);
            }
            return isValid;
        };
        Utility.prototype.throwError = function (functionNameParam, error) {
            /// <summary>
            /// This function is used to show error Message if any error occurs during the processing
            /// </summary>
            /// <param name="functionName" type="string">
            /// Function Name
            /// </param>
            /// <param name="error" type="any">
            /// The error that occured during processing
            /// </param>
            /// <returns type="void" />
            var functionName = "throwError";
            var errorMessage = "";
            try {
                //Concatenate the Message together
                errorMessage = functionNameParam + ": Error: " + (error.description || error.message);
                //Show Error Message
                dfa_Utility.showMessage(errorMessage);
            }
            catch (ex) {
                //Concatenate the Message together
                errorMessage = functionNameParam + ": Error: " + (error.description || error.message);
                //Show Error Message
                dfa_Utility.showMessage(functionName + " Error: " + (ex.description || ex.message));
            }
        };
        Utility.prototype.showMessage = function (message) {
            /// <summary>
            /// This function is used to show Message
            /// </summary>
            /// <param name="message" type="string">
            /// Message that needs to be shown
            /// </param>
            /// <returns type="void" />
            var functionName = "showMessage";
            try {
                //Validate Xrm.Utility and proceed
                if (dfa_Utility.isValid(Xrm.Navigation)) {
                    var alertStrings = { confirmButtonLabel: "OK", text: message, title: "" };
                    var alertOptions = { height: 120, width: 260 };
                    Xrm.Navigation.openAlertDialog(alertStrings, alertOptions);
                }
                else {
                    alert(message);
                }
            }
            catch (ex) {
                dfa_Utility.throwError(functionName, ex);
            }
        };
        Utility.prototype.setDisplayState = function (executionContext, state, attribute) {
            /// <summary>
            /// This function executes on Load of Interaction Type Entity and set display state of tabs.
            /// </summary>
            /// <param name="state" type="string">
            /// <param name="attribute" type="any">
            /// </param>
            /// <returns type="void" />
            var functionName = "setDisplayState >>";
            var splitTabs;
            var tabname = "";
            var formContext = executionContext.getFormContext();
            try {
                splitTabs = attribute;
                for (var i = 0; i < splitTabs.length; i++) {
                    tabname = splitTabs[i];
                    if (tabname.search(':') > -1) {
                        var tabAndSection = tabname.split(':');
                        var sectionTab = formContext.ui.tabs.get(tabAndSection[0]);
                        if (dfa_Utility.isValid(sectionTab)) {
                            sectionTab.setDisplayState(state);
                        }
                    }
                    else {
                        var theTab = formContext.ui.tabs.get(tabname);
                        theTab.setDisplayState(state);
                    }
                }
            }
            catch (ex) {
                dfa_Utility.throwError(functionName, ex);
            }
        };
        Utility.prototype.setLabelsValue = function (executionContext, attribute, labelsValue) {
            /// <summary>
            /// This function used to set label depends on data type
            /// </summary>
            /// <param name="fieldsName" type="any">
            /// <returns type="void" />
            var functionName = "setLabelsValue";
            var formContext = executionContext.getFormContext();
            try {
                var splitAttribute = attribute.split(",");
                var labelValueList = labelsValue.split(',');
                for (var fieldName in splitAttribute) {
                    fieldName = fieldName.trim();
                    //To check Attributes/WebResources exists or not.
                    var theControl = formContext.getControl(splitAttribute[fieldName]);
                    var theTab = formContext.ui.tabs.get(splitAttribute[fieldName]);
                    if (dfa_Utility.isValid(theControl)) {
                        //To hide Attributes/Webresources
                        theControl.setLabel(labelValueList[fieldName]);
                    }
                    ////To check Tab exists or not
                    else if (dfa_Utility.isValid(theTab)) {
                        //To hide Tabs
                        theTab.setLabel(labelValueList[fieldName]);
                    }
                    //To hide & show Tab Sections.
                    else {
                        if (splitAttribute[fieldName].search(":") > 1) {
                            var splitTabSection = splitAttribute[fieldName].split(":");
                            var tabname = splitTabSection[0];
                            var sectionname = splitTabSection[1];
                            var sectionTab = formContext.ui.tabs.get(tabname);
                            if (dfa_Utility.isValid(sectionTab)) {
                                //To hide section
                                var theSection = sectionTab.sections.get(sectionname);
                                if (dfa_Utility.isValid(theSection)) {
                                    theSection.setLabel(labelValueList[fieldName]);
                                }
                            }
                        }
                    }
                }
            }
            catch (ex) {
                dfa_Utility.throwError(functionName, ex);
            }
        };
        Utility.prototype.navigateToFormByName = function (executionContext, formName) {
            /// <summary>
            /// Navigate to a Form By Name
            /// </summary>
            /// <param name="formName" type="string">
            /// <returns type="void" />
            var functionName = "navigateToFormByName";
            var formContext = executionContext.getFormContext();
            var clientContext = Xrm.Utility.getGlobalContext().client;
            try {
                // Mobile App only have 1 form.
                if (clientContext.getClient() == "Mobile") {
                    return;
                }
                if (formContext.ui.formSelector.getCurrentItem().getLabel() !== formName) {
                    var items = formContext.ui.formSelector.items;
                    items.forEach(function (item, index) {
                        var itemLabel = item.getLabel();
                        if (itemLabel === formName) {
                            //navigate to the form
                            item.navigate();
                        } //endif
                    });

                } //endif
            }
            catch (ex) {
                dfa_Utility.throwError(functionName + " >>", ex);
            }
        };

        Utility.prototype.DisableAllFields = function (executionContext) {
            var formContext = executionContext.getFormContext();
            formContext.ui.controls.forEach(function (control, i) {
                if (control && control.getDisabled && !control.getDisabled()) {
                    control.setDisabled(true);
                }
            });
        };

        Utility.prototype.getAttributeValue = function (executionContext, attributeSchemaName) {
            functionName = "getAttributeValue";
            try {
                var attributeValue = null;
                var formContext = executionContext.getFormContext();
                var attribute = formContext.getAttribute(attributeSchemaName);
                if (dfa_Utility.isValid(attribute)) {
                    attributeValue = attribute.getValue();
                }
                return attributeValue;
            }
            catch (ex) {
                dfa_Utility.throwError(functionName + " >>", ex);
            }
        };
        Utility.prototype.setFieldsValue = function (executionContext, fieldsName, fieldsValue) {
            /// <summary>
            /// This function used to set value depends on data type
            /// </summary>
            /// <param name="fieldsName" type="any">
            /// <returns type="void" />
            var functionName = "setFieldsValue";
            var fieldsList;
            var fieldValueList;
            var controlType = "";
            var formContext = executionContext.getFormContext();
            try {
                if (dfa_Utility.isValid(fieldsName)) {
                    if (fieldsName.id)
                    fieldsList = fieldsName.split(',');
                    fieldValueList = fieldsValue.split(',');
                    for (var field in fieldsList) {
                        var theAttribute = formContext.getAttribute(fieldsList[field]);
                        if (dfa_Utility.isValid(theAttribute)) {
                            controlType = theAttribute.getAttributeType();
                            switch (controlType) {
                                case "boolean":
                                    theAttribute.setValue(fieldValueList[field] === "true");
                                    break;
                                case "string":
                                case "memo":
                                case "String":
                                    var fieldValue = fieldValueList[field];
                                    if (fieldValue === "") {
                                        theAttribute.setValue(null);
                                    } else {
                                        theAttribute.setValue(fieldValue);
                                    }
                                    break;
                                case "optionset":
                                case "Integer":
                                case "double":
                                case "decimal":
                                case "money":
                                    var theFieldValue = fieldValueList[field];
                                    if (theFieldValue === "") {
                                        theAttribute.setValue(null);
                                    } else {
                                        theAttribute.setValue(Number(theFieldValue));
                                    }
                                    break;

                            }
                            theAttribute.fireOnChange();
                        }
                    }
                }
            }
            catch (e) {
                dfa_Utility.throwError(functionName, e);
            }
        };
        Utility.prototype.setFieldValue = function (executionContext, fieldName, fieldValue) {
            /// <summary>  Added by Forrest on Sep16,2022
            /// This function used to set value depends on data type
            /// </summary>
            /// <param name="fieldName" type="any">
            /// <returns type="void" />
            var functionName = "setFieldValue"; 
            var controlType = "";
            var formContext = executionContext.getFormContext();
            try {
                if (dfa_Utility.isValid(fieldName)) 
                {              
                    var theAttribute = formContext.getAttribute(fieldName);
                    if (dfa_Utility.isValid(theAttribute)) 
                    {
                        controlType = theAttribute.getAttributeType();
                        switch (controlType) {
                            case "boolean":
                                theAttribute.setValue(fieldValue === "true");
                                break;
                            case "string":
                            case "memo":
                            case "String":
                                var fieldVal = fieldValue;
                                if (fieldVal === "") {
                                    theAttribute.setValue(null);
                                } else {
                                    theAttribute.setValue(fieldVal);
                                }
                                break;
                            case "optionset":
                            case "Integer":
                            case "double":
                            case "decimal":
                            case "money":
                                var theFieldValue = fieldValue;
                                if (theFieldValue === "") {
                                    theAttribute.setValue(null);
                                } else {
                                    theAttribute.setValue(Number(theFieldValue));
                                }
                                break;

                        }
                        theAttribute.fireOnChange();
                    }
                   
                }
            }
            catch (e) {
                dfa_Utility.throwError(functionName, e);
            }
        };
        Utility.prototype.filterOutOptionSet = function (executionContext, attributeSchemaName, hidOptionValues) {
            /// <summary>
            /// This function is to remove any options from Option Set from CRM Form
            /// if values are in the list for visible Option Values
            /// </summary>
            /// <param type="string" name="attributeSchemaName" />
            /// <param type="string" name="hidOptionValues" />
            /// <returns type="void" />
            var functionName = "filterOptionSet";
            var formContext = executionContext.getFormContext();
            try {
                var targetControl = formContext.getControl(attributeSchemaName);
                if (dfa_Utility.isValid(targetControl)) {
                    var optionValuesArray = hidOptionValues.split(",");
                    var options = targetControl.getOptions();
                    for (var i = 0; i < options.length; i++) {
                        var currentOption = options[i].value;
                        if (currentOption !== currentOption) {
                            // currentOption is NaN
                            continue;
                        }
                        var hasMatch = false;
                        for (var j = 0; j < optionValuesArray.length; j++) {
                            var optionToCompare = parseInt(optionValuesArray[j]);
                            if (currentOption == optionToCompare) {
                                hasMatch = true;
                                break;
                            }
                        }

                        if (hasMatch) {
                            targetControl.removeOption(currentOption);
                        }
                    }
                }
            }
            catch (e) {
                dfa_Utility.throwError(functionName, e);
            }
        };
        Utility.prototype.filterOptionSet = function (executionContext, attributeSchemaName, visibleOptionValues) {
            /// <summary>
            /// This function is to remove any options from Option Set from CRM Form
            /// if values are not in the list for visible Option Values
            /// </summary>
            /// <param type="string" name="attributeSchemaName" />
            /// <param type="string" name="visibleOptionValues" />
            /// <returns type="void" />
            var functionName = "filterOptionSet";
            var formContext = executionContext.getFormContext();
            try {
                var targetControl = formContext.getControl(attributeSchemaName);
                if (dfa_Utility.isValid(targetControl)) {
                    var optionValuesArray = visibleOptionValues.split(",");
                    var options = targetControl.getOptions();
                    for (var i = 0; i < options.length; i++) {
                        var currentOption = options[i].value;
                        if (currentOption !== currentOption) {
                            // currentOption is NaN
                            continue;
                        }
                        var hasMatch = false;
                        for (var j = 0; j < optionValuesArray.length; j++) {
                            var optionToCompare = parseInt(optionValuesArray[j]);
                            if (currentOption == optionToCompare) {
                                hasMatch = true;
                                break;
                            }
                        }

                        if (!hasMatch) {
                            targetControl.removeOption(currentOption);
                        }
                    }
                }
            }
            catch (e) {
                dfa_Utility.throwError(functionName, e);
            }
        };
        Utility.prototype.addOptionSet = function (executionContext, attributeSchemaName, visibleOptionValues, optionObject) {
            /// <summary>
            /// This function is to add any options to the Option Set control which might have values removed previously
            /// </summary>
            /// <param type="string" name="attributeSchemaName" />
            /// <param type="string" name="visibleOptionValues" />
            /// <param type="object" name="optionObject" />
            /// <returns type="void" />
            var functionName = "addOptionSet";
            var formContext = executionContext.getFormContext();
            try {
                var targetControl = formContext.getControl(attributeSchemaName);
                if (dfa_Utility.isValid(targetControl)) {
                    var optionValuesArray = visibleOptionValues.split(",");
                    var options = targetControl.getOptions();
                    for (var j = 0; j < optionValuesArray.length; j++) {
                        var optionToCompare = parseInt(optionValuesArray[j]);
                        var hasMatch = false;
                        for (var i = 0; i < options.length; i++) {
                            var currentOption = options[i].value;
                            if (currentOption !== currentOption) {
                                // currentOption is NaN
                                continue;
                            }

                            if (currentOption == optionToCompare) {
                                hasMatch = true;
                                break;
                            }
                        }

                        if (!hasMatch) {
                            targetControl.addOption(optionObject);
                        }
                    }
                }
            }
            catch (e) {
                dfa_Utility.throwError(functionName, e);
            }
        };
        Utility.prototype.checkUserRole = function (roleName, userID) {
            userID = userID.replace('{', '').replace('}', '');
            var parameters = {
                "RoleName": roleName,
                "UserID": userID
            };
            var customActionName = "dfa_ValidateUserHasSecurityRoleAction";
            var response = dfa_Utility.callCustomAction(customActionName, parameters);
            if (response != null) {
                return response.HasRole;
            }
            
        };
        // Check User Security RoleId
        Utility.prototype.checkCurrentUserRole = function (roleName) {
            /// <summary>
            /// This Function check Current User has certain security role by name
            /// 
            /// </srummary>
            /// <param type="string" name="roleName" />
            /// <returns type="bool" />
            /* v9.1
            var userSettings = Xrm.Utility.getGlobalContext().userSettings;
            var userRoles = userSettings.securityRoles;
            var hasRole = false;
            if (userRoles != null && userRoles != undefined) {
                userRoles.forEach(function (item) {
                    if (item.name.toLowerCase() === roleName.toLowerCase()) {
                        hasRole = true;
                    }
                });
            }
            return hasRole;
            */
            var globalContext = Xrm.Utility.getGlobalContext();
            var filter = "?$select=name";
            var currentUserRoles = globalContext.userSettings.securityRoles; // v9
            for (var i = 0; i < currentUserRoles.length; i++) {
                var userRoleId = currentUserRoles[i];
                var userRole = dfa_Utility.retrieveRecordCustom(userRoleId, "role", filter);
                var userRoleName = userRole.name;
                if (userRoleName == roleName) {
                    return true;
                }
            }
            return false;
        };
        Utility.prototype.getRoleName = function (globalContext, userRoleId) {
            /// <summary>
            /// This Function check Current User has certain security role by name
            /// 
            /// </srummary>
            /// <param type="string" name="userRoleId" />
            /// <returns type="roleName" />
            if (typeof ($) === 'undefined') {
                $ = parent.$;
                jQuery = parent.jQuery;
            }
            var selectQuery = "/api/data/v9.0/roles(" + userRoleId + ")?$select=name";
            var serverUrl = globalContext.getClientUrl();
            var odataSelect = serverUrl + selectQuery;

            var roleName = null;
            $.ajax({
                type: "GET",
                async: false,
                contentType: "application/json; charset=utf-8",
                datatype: "json",
                url: odataSelect,
                beforeSend: function (XMLHttpRequest) { XMLHttpRequest.setRequestHeader("Accept", "application/json"); },
                success: function (data, textStatus, XmlHttpRequest) {
                    roleName = data.name;
                },
                error: function (XmlHttpRequest, textStatus, errorThrown) {
                }
            });
            return roleName;
        };

        Utility.prototype.getEntityRecordName = function (globalContext, entitySchemaName, entityRecordId, attributeName) {
            /// <summary>
            /// This Function gets Entity Record value of given entity / Primary Key and attribute name
            /// By query via Web Api
            /// </srummary>
            /// <param type="string" name="entitySchemaName" />
            /// <param type="string" name="entityRecordId" />
            /// <param type="string" name="attributeName" />
            /// <returns type="entityRecordName" />
            if (typeof ($) === 'undefined') {
                $ = parent.$;
                jQuery = parent.jQuery;
            }
            var sOrES = "s";
            var lastCharacterOfEntitySchemaName = entitySchemaName.substring(entitySchemaName.length - 1);
            if (lastCharacterOfEntitySchemaName == "s") {
                sOrES = "es";
            } else if (lastCharacterOfEntitySchemaName == "y") {
                sOrES = "ies";
            }
            var selectQuery = "/api/data/v9.0/" + entitySchemaName + sOrES + "(" + entityRecordId + ")?$select=" + attributeName;
            var entityRecordName = null;
            var serverUrl = globalContext.getClientUrl();
            var odataSelect = serverUrl + selectQuery;

            $.ajax({
                type: "GET",
                async: false,
                contentType: "application/json; charset=utf-8",
                datatype: "json",
                url: odataSelect,
                beforeSend: function (XMLHttpRequest) { XMLHttpRequest.setRequestHeader("Accept", "application/json"); },
                success: function (data, textStatus, XmlHttpRequest) {
                    entityRecordName = data[attributeName];
                },
                error: function (XmlHttpRequest, textStatus, errorThrown) {
                }
            });

            return entityRecordName;
        };
        Utility.prototype.generateLookupObject = function (entitySchemaName, entityRecordId, entityRecordName) {
            /// <summary>
            /// This Function instantiate a Lookup Object (Array) with given entitySchemaName, entityRecordId, entityRecordName
            /// By query via Web Api
            /// </srummary>
            /// <param type="string" name="entitySchemaName" />
            /// <param type="string" name="entityRecordId" />
            /// <param type="string" name="entityRecordName" />
            /// <returns type="lookupArray" />
            var lookupArray = new Array();
            lookupArray[0] = new Object();
            // Treat the entity record to include curly braces if needed
            if (entityRecordId.indexOf("{") === -1) {
                entityRecordId = "{" + entityRecordId + "}";
            }
            lookupArray[0].id = entityRecordId;
            lookupArray[0].name = entityRecordName;
            lookupArray[0].entityType = entitySchemaName;
            return lookupArray;
        };
        Utility.prototype.setLookupValue = function (executionContext, attributeSchemaName, lookupObj) {
            /// <summary>
            /// This Function set Lookup Value
            /// By query via Web Api
            /// </srummary>
            /// <param type="string" name="attributeSchemaName" />
            /// <param type="array" name="lookupObj" />
            /// <returns type="void" />
            if (!dfa_Utility.isValid(attributeSchemaName)) {
                return;
            }
            var formContext = executionContext.getFormContext();
            var theAttribute = formContext.getAttribute(attributeSchemaName);
            if (!dfa_Utility.isValid(theAttribute)) {
                return;
            }
            theAttribute.setValue(lookupObj);
        };
        Utility.prototype.retrieveMultipleCustom = function (entityLogicalName, filter) {
            //entityLogicalName is required, in lower case i.e. "account"

            if (!entityLogicalName) {
                alert("entityLogicalName is required.");
                return;
            }

            var returnValue = null;

            var entityPluralName = dfa_Utility.getPluralEntityLogicalName(entityLogicalName);

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
            // https://DynamicsURL/api/data/v9.0/accounts
            odataUri = serverUrl + WEBAPI_ENDPOINT + entityPluralName;

            //If a filter is supplied, append it to the OData URI
            if (filter) {
                odataUri += filter;
            }

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
                    if (data && data.value) {
                        // If there is more than 1 row returned;
                        returnValue = data.value;
                    }
                    else {
                        returnValue = data;
                    }
                },
                error: function (XmlHttpRequest, textStatus, errorThrown) {
                    //alert('OData Select Failed: ' + odataSelect);
                }
            });
            return returnValue;
        };
        Utility.prototype.retrieveRecordCustom = function (id, entityLogicalName) {
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
            var entityPluralName = dfa_Utility.getPluralEntityLogicalName(entityLogicalName);
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
        };
       
        Utility.prototype.callCustomAction = function (customActionName, parameters) {
            var returnValue = null;
            // Load JQuery for AJAX call, if not already
            // Dynamics has JQuery loaded on the form somewhere
            if (typeof ($) === 'undefined') {
                $ = parent.$;
                jQuery = parent.jQuery;
            }
            var globalContext = Xrm.Utility.getGlobalContext();
            var strinifyParameters = window.JSON.stringify(parameters);
            var serverUrl = globalContext.getClientUrl();
            if (serverUrl.match(/\/$/)) {
                serverUrl = serverUrl.substring(0, serverUrl.length - 1);
            }

            var url = serverUrl + "/api/data/v9.0/" + customActionName;
            $.ajax({
                type: "POST",
                async: false,
                contentType: "application/json; charset=utf-8",
                datatype: "json",
                data: strinifyParameters,
                url: url,
                beforeSend: function (XMLHttpRequest) {
                    XMLHttpRequest.setRequestHeader("Content-Type", "application/json; charset=utf-8");
                    XMLHttpRequest.setRequestHeader("Accept", "application/json");
                },
                success: function (data) {
                    returnValue = data;
                },
                error: function (XmlHttpRequest, textStatus, errorThrown) {
                    errorHandler(XmlHttpRequest, textStatus, errorThrown);
                }
            });
            return returnValue;
        };
        Utility.prototype.getPluralEntityLogicalName =function(entityLogicalName) {

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
        };
        Utility.prototype.isEntityNameSomething = function (primaryControl, expectedEntityName) {
            // Used in subgrid button display rule
            var functionName = "isEntityNameSomething";
            try {
                if (primaryControl == null || expectedEntityName == null || expectedEntityName.trim() == "") {
                    return false;
                }
                var formContext = primaryControl;
                var entityName = formContext.data.entity.getEntityName();
                return entityName == expectedEntityName;
            }
            catch (error) {
                dfa_Utility.throwError(functionName + " -> " + error.message, e);
                return false;
            }
        };
		Utility.prototype.triggerOnDemandWorkflowFromForm = function(primaryControl, workflowId) {
			var functionName = "triggerOnDemandWorkflowFromForm";
			var formContext = primaryControl;
			var recordId = formContext.data.entity.getId().replace('{', '').replace('}', '');
			var entityName = formContext.data.entity.getEntityName();
			var executeWorkflowRequest = {
				entity: {
					id: workflowId,
					entityType: "workflow"
				},
				EntityId: { guid: recordId},
				getMetadata: function() {
					return {
						boundParameter: "entity",
						parameterTypes: {
								"entity": {
									"typeName": "mscrm.workflow",
									"structuralProperty":5
								},
								"EntityId": {
									"typeName": "Edm.Guid",
									"structuralProperty":1
								}
						},							
						operationType:0,
						operationName: "ExecuteWorkflow"
					};
				}
			};
			Xrm.WebApi.online.execute(executeWorkflowRequest).then (
				function success(result) {
					if (result.ok) {
						// Refresh current form
						// formContext.data.refresh();
                        var entityFormOptions = {};
                        entityFormOptions["entityName"] = entityName;
                        entityFormOptions["entityId"] = recordId;
                        Xrm.Navigation.openForm(entityFormOptions).then(
                            function (success) {
                                console.log(success);
                            },
                            function (error) {
                                alert(error.message);
                                console.log(error);
                            }
                        );
					}
				},
				function(error) {
					
					dfa_Utility.throwError(functionName + " -> " + error.message, e);
				}
			);
        };
        Utility.prototype.RefreshCurrentForm = function (entityName, entityId) {
            //var formContext = executionContext.getFormContext();
            //var entityId = formContext.data.entity.getId().replace('{', '').replace('}', '');
            //var entityName = formContext.data.entity.getEntityName();
            var entityFormOptions = {};
            entityFormOptions["entityName"] = entityName;
            entityFormOptions["entityId"] = entityId;
            Xrm.Navigation.openForm(entityFormOptions).then(
                function (success) {
                    console.log(success);
                },
                function (error) {
                    alert(error.message);
                    console.log(error);
                }
            );
        };
        Utility.prototype.RefreshFromData = function (executionContext, delay) {

            var formContext = executionContext.getFormContext();
            if (delay == null || delay == NaN || delay == 0) {
                formContext.data.refresh();
            }
            else {
                setTimeout(function () {
                    formContext.data.refresh();
                }, delay);
            }
        };
        Utility.prototype.ValidateDateNotInFuture = function (executionContext, fieldName, fieldDisplayName, notificationName) {
            // For Date Only ONLY
            // dfa_Utility.ValidateDateNotInFuture
            var formContext = executionContext.getFormContext();
            // formContext.getControl(fieldName).clearNotification("101");
            formContext.ui.clearFormNotification(notificationName);
            var oField = formContext.getAttribute(fieldName);

            if (typeof (oField) != "undefined" && oField != null && oField.getValue() != null && oField.getValue() != "undefined") {
                var dateToValidate = new Date(oField.getValue());
                var todayDate = new Date();
                if (todayDate <= dateToValidate) {
                    // https://docs.microsoft.com/en-us/powerapps/developer/model-driven-apps/clientapi/reference/controls/setnotification
                    //alert("Date of Birth cannot be in the future");
                    formContext.ui.setFormNotification((fieldDisplayName + " cannot be in the future."),
                        "WARNING",
                        notificationName);
                    oField.setValue(null);
                }
            }
        };
        Utility.prototype.UnadjustUserLocalTimeInUCI = function (date) {
            if (!Xrm.Internal.isUci()) {
                return date;
            }
            var systemTzOffsetMs = date.getTimezoneOffset() * 60000;
            var userTzOffsetMs = Xrm.Utility.getGlobalContext().userSettings.getTimeZoneOffsetMinutes() * 60000;
            var unadjusted = new Date(date.getTime() + systemTzOffsetMs + userTzOffsetMs);
            return unadjusted;
        };
        Utility.prototype.RefreshTimeLine = function (executionContext, timelineControlName) {
            var formContext = executionContext.getFormContext();
            var timelineControl = formContext.getControl(timelineControlName);
            if (dfa_Utility.isValid(timelineControl)) {
                timelineControl.refresh();
            }
        };
        Utility.prototype.RunReportFromForm = function (formContext, reportId, entityTypeCode) {
            // The goal: Get Below URL
            // https://embc-dfa.dev.jag.gov.bc.ca
            // /crmreports/viewer/viewer.aspx
            //?id=%7bC745C493-4429-ED11-B834-00505683FBF4%7d          <- Report Id
            //&records=%7b5c3dc001-11d3-ec11-b832-00505683fbf4%7d     <- Record ID  
            //&recordstype=112                                        <- Entity Type Code
            //&action=run
            //&context=records

            // Get Entity Type Code :  https://embc-dfa.dev.jag.gov.bc.ca/api/data/v9.0/EntityDefinitions?$select=LogicalName,%20ObjectTypeCode&$filter=LogicalName%20eq%20%27incident%27
            // Note! It needs those curly braces to run
            // But Do HTML Encode them before navigate
            var entityId = formContext.data.entity.getId().replace('{', '').replace('}', '');
            var globalContext = Xrm.Utility.getGlobalContext();
            var serverUrl = globalContext.getClientUrl();
            if (serverUrl.match(/\/$/)) {
                serverUrl = serverUrl.substring(0, serverUrl.length - 1);
            }
            var reportViewer = "/crmreports/viewer/viewer.aspx";
            var url = serverUrl + reportViewer + "?id=%7b" + reportId + "%7d&records=%7b" + entityId + "%7d&recordstype=" + entityTypeCode + "&action=run&context=records";
            window.open(url, '_blank');
        };
        Utility.prototype.AddSubgridEventListener = function (executionContext, subgridControlName, onSubgridRefreschFunction) {
            var formContext = executionContext.getFormContext();
            var gridControl = formContext.getControl(subgridControlName);
            if (gridControl == null) {
                setTimeout(function () {
                    dfa_Utility.AddSubgridEventListener(executionContext, subgridControlName, onSubgridRefreschFunction)
                }, 500);
            }
            else {
                // Bind the event listener when subgrid is loaded
                gridControl.addOnLoad(onSubgridRefreschFunction);
            }
        };
        Utility.prototype.OpenDocumentsTab = function (executionContext, mainTabName, triedTimes) {
            // This doesn't work with v9.0 On Premise.  Despite waiting for 10 seconds
            // the navSPDocuments is not available until clicked upon manually
            if (triedTimes > 10) {
                return;
            }
            triedTimes++;
            var formContext = executionContext.getFormContext();
            var formType = formContext.ui.getFormType();
            if (formType != 1) {
                var navDocumentsItem = formContext.ui.navigation.items.get("navSPDocuments");
                if (navDocumentsItem != null) {
                    navDocumentsItem.setFocus();
                    var mainTab = formContext.ui.tabs.get(mainTabName);
                    navDocumentsItem.setFocus();
                    mainTab.setFocus();
                }
                else {
                    setTimeout(function () {
                        dfa_Utility.OpenDocumentsTab(executionContext, mainTab, triedTimes);
                    }, 1000);
                }
            }
        }
        return Utility;
    }());
    CrmJS.Utility = Utility;
})(CrmJS || (CrmJS = {}));
var dfa_Utility = new CrmJS.Utility();


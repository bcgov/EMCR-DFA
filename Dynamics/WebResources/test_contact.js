// Create a unique namespace for this library to prevent name collisions
if (typeof (IsoFairlens) === "undefined") {
   IsoFairlens = {
       namespace: true
   };
}
if (typeof (IsoFairlens.iso_contact) === "undefined") {
   IsoFairlens.iso_contact = {
       namespace: true
   };
}
IsoFairlens.iso_contact = {

   FormOnLoad: function (context) {
       var formContext = context.getFormContext ? context.getFormContext() : context;
       var contactType = formContext.getAttribute("iso_contact_type");
       if (contactType != null && contactType.getValue() == null) {
           contactType.setValue(863050000);
       }

       IsoFairlens.iso_contact.setDefaultCustomerLookupView(context);

       //set customer lookup only for account type
       formContext.getControl("parentcustomerid").setEntityTypes(["account"]);
       //add pre search
       // formContext.getControl("parentcustomerid").addPreSearch(filterAccounts);
       //disable customer number for update record
       var formMode = formContext.ui.getFormType();

       if (formMode >= 2) //update mode
       {
           if (formContext.getControl("iso_cs_number").getAttribute() != null) {
               formContext.getControl("iso_cs_number").setDisabled(true);
               formContext.getControl("iso_csnumberavailable").setDisabled(true);
               formContext.getControl("header_iso_contact_type").setDisabled(true);
           }
           //setting for quick create - Phonecall
           var lookupContact = [{
               "entityType": "contact",
               "name": formContext.getAttribute("firstname").getValue() + " " + formContext.getAttribute("lastname").getValue(),
               "id": formContext.data.entity.getId()
           }]
           window.top.attribute_iso_client = lookupContact;
           if (formContext.getAttribute("parentcustomerid") != null && formContext.getAttribute("parentcustomerid").getValue() != null) {
               window.top.attribute_iso_organization = formContext.getAttribute("parentcustomerid").getValue();
           }
           // IsoFairlens.iso_contact.CreateSettingTask(context);

       }
       // show hide contact sections based on Contact Type
       var tabObj = formContext.ui.tabs.get("SUMMARY_TAB");
       if (tabObj != null) {
           formContext.getControl("header_iso_contact_type").getAttribute().addOnChange(OnChangeContactType); //Add on change event 
           OnChangeContactType();
       }

       function OnChangeContactType() {
           var tabObj = formContext.ui.tabs.get("SUMMARY_TAB");
           if (tabObj != null) {
               var casetabObj = formContext.ui.tabs.get("cases");
               var sectpersonalInfoObj = tabObj.sections.get("personal_information");
               var sectconsiderationObj = tabObj.sections.get("consideration");
               var sectdesignationObj = tabObj.sections.get("designation");
               var sectcontactdetailsObj = tabObj.sections.get("role_details");
               var sectnotesObj = tabObj.sections.get("note");
               var sectaddressObj = tabObj.sections.get("address");
               if (contactType != null && contactType.getValue() == "863050000") {
                   sectcontactdetailsObj.setVisible(false);
                   sectaddressObj.setVisible(false);
                   formContext.getControl("emailaddress1").setVisible(false);
                   formContext.getControl("telephone1").setVisible(false);
                   formContext.getControl("mobilephone").setVisible(false);
                   formContext.getControl("fax").setVisible(false);
                   formContext.getControl("preferredcontactmethodcode").setVisible(false);
                   formContext.getControl("iso_legal_representative").setVisible(true);
                   formContext.getAttribute("iso_cs_number").setRequiredLevel("required");
                   sectconsiderationObj.setVisible(true);
                   sectdesignationObj.setVisible(true);
                   sectpersonalInfoObj.setVisible(true);
                   sectnotesObj.setVisible(true);
                   casetabObj.setVisible(true);
                   formContext.getControl("middlename").setVisible(true);
                   formContext.getControl("iso_preferred_name").setVisible(true);
                   formContext.getControl("birthdate").setVisible(true);
                   // Hide parentcustomerid (Correctional Centre) for Clients
                   formContext.getControl("parentcustomerid").setVisible(false);
                   formContext.getControl("iso_legal_representative").addPreSearch(filterLegalRep);
               }
               if (contactType != null && contactType.getValue() != "863050000") {
                   formContext.getAttribute("iso_cs_number").setRequiredLevel("none");
                   formContext.getAttribute("iso_cs_number").setValue(null);
                   sectconsiderationObj.setVisible(false);
                   sectdesignationObj.setVisible(false);
                   sectpersonalInfoObj.setVisible(false);
                   sectnotesObj.setVisible(false);
                   casetabObj.setVisible(false);
                   formContext.getControl("middlename").setVisible(false);
                   formContext.getControl("iso_preferred_name").setVisible(false);
                   formContext.getControl("birthdate").setVisible(false);
           // Show parentcustomerid (Organization) for other contact types
                   formContext.getControl("parentcustomerid").setVisible(true);
                   formContext.getControl("parentcustomerid").setLabel("Organization");
                   sectcontactdetailsObj.setVisible(true);
                   sectaddressObj.setVisible(true);
                   formContext.getControl("emailaddress1").setVisible(true);
                   formContext.getControl("telephone1").setVisible(true);
                   formContext.getControl("mobilephone").setVisible(true);
                   formContext.getControl("fax").setVisible(true);
                   formContext.getControl("preferredcontactmethodcode").setVisible(true);
                   formContext.getControl("iso_legal_representative").setVisible(false);
               }

           }
           IsoFairlens.iso_contact.LoadBusinessHolidayList(context);

       }
       // filter accounts based on Contact Type selected
       function filterLegalRep() {
           if (contactType != null && contactType.getValue() == "863050000") {
               var legalrepFilter = "<filter type='and'><condition attribute='iso_contact_type' operator='eq' value='863050005'/> " +
                   "</filter>";
               formContext.getControl("iso_legal_representative").addCustomFilter(legalrepFilter, "contact");
           }
       }

       //Quick create
       var qfcontacttabObj = formContext.ui.tabs.get("qccontacttab");
       if (qfcontacttabObj != null && contactType.getValue() == "863050000") {
           var sectqfcontactinforObj = qfcontacttabObj.sections.get("qf_contact_informtaion");
           var sectqfaddressObj = qfcontacttabObj.sections.get("qf_address");
           sectqfcontactinforObj.setVisible(false);
           sectqfaddressObj.setVisible(false);
           formContext.getAttribute("iso_cs_number").setRequiredLevel("required");
           // Hide parentcustomerid (Correctional Centre) for Clients
           formContext.getControl("parentcustomerid").setVisible(false);
       } else if (qfcontacttabObj != null) {
           formContext.getAttribute("iso_cs_number").setRequiredLevel("none");
           formContext.getControl("iso_csnumberavailable").setVisible(false);
           formContext.getControl("iso_cs_number").setVisible(false);
           formContext.getControl("iso_contact_type").setVisible(false);
           // Show parentcustomerid (Organization) for other contact types
           formContext.getControl("parentcustomerid").setVisible(true);
           formContext.getControl("parentcustomerid").setLabel("Organization");
       }
   },

   CreateContact: function (contactType) {
       if (contactType != "") {
           var entityFormOptions = {};
           var formParameters = {};
           formParameters["iso_contact_type"] = contactType;
           entityFormOptions["entityName"] = "contact";
           Xrm.Navigation.openForm(entityFormOptions, formParameters).then(

               function (success) {
                   console.log(success);
               },

               function (error) {
                   console.log(error);
               });
       }
   },

   QuickCreateContact: function (contactType, primaryControl) {
       if (contactType != "") {
           var entityFormOptions = {};
           var formParameters = {};
           formParameters["iso_contact_type"] = contactType;
           entityFormOptions["entityName"] = "contact";
           entityFormOptions["useQuickCreateForm"] = true;

           var lookupAccount = [{
               "entityType": "account",
               "name": primaryControl.getAttribute("name").getValue(),
               "id": primaryControl.data.entity.getId()
           }]
           formParameters["parentcustomerid"] = lookupAccount;
           Xrm.Navigation.openForm(entityFormOptions, formParameters).then(

               function (success) {
                   console.log(success);
               },

               function (error) {
                   console.log(error);
               });
       }
   },

   //Validate CS Number - 8 digit number or N/A
   ValidateCSNumber: function (context) {
       var formContext = context.getFormContext ? context.getFormContext() : context;
       var fieldText = formContext.getAttribute("iso_cs_number").getValue();
       var contactType = formContext.getAttribute("iso_contact_type")

       //check for contact type = "client" if Prisoner then only check for CS Number
       //CS Number should be 8 length and contains only numbers
       if (contactType != null && contactType.getValue() == "863050000") {
           if (fieldText === null) return;


           if (!fieldText.match("^[0-9]*$") || fieldText.length < 8) {
               formContext.getControl("iso_cs_number").setNotification("Please enter length of 8 digit numbers", 102);
               return false;
           } else {
               formContext.getControl("iso_cs_number").clearNotification(102);
               return true;
           }
       }
   },

   //Quick Form - On Contact Type value change
   ShowHideQuickFormFields: function (context) {
       var formContext = context.getFormContext ? context.getFormContext() : context;
       var contactType = formContext.getAttribute("iso_contact_type")
       if (contactType != null) {
           var qfcontacttabObj = formContext.ui.tabs.get("qccontacttab");
           var sectqfcontactinforObj = qfcontacttabObj.sections.get("qf_contact_informtaion");
           var sectqfaddressObj = qfcontacttabObj.sections.get("qf_address");

           sectqfcontactinforObj.setVisible(false);
           sectqfaddressObj.setVisible(false);
           formContext.getControl("iso_csnumberavailable").setVisible(true);
           formContext.getControl("iso_cs_number").setVisible(true);
           formContext.getAttribute("iso_cs_number").setRequiredLevel("required");
           // Hide parentcustomerid (Correctional Centre) for Clients
           formContext.getControl("parentcustomerid").setVisible(false);

           if (contactType.getValue() != "863050000") {
               sectqfcontactinforObj.setVisible(true);
               sectqfaddressObj.setVisible(true);
               formContext.getAttribute("iso_cs_number").setRequiredLevel("none");
               formContext.getAttribute("iso_cs_number").setValue(null);
               formContext.getControl("iso_csnumberavailable").setVisible(false);
               formContext.getControl("iso_cs_number").setVisible(false);
               // Show parentcustomerid (Organization) for other contact types
               formContext.getControl("parentcustomerid").setVisible(true);
               formContext.getControl("parentcustomerid").setLabel("Organization");
           }
       }
   },

   OnChangeCSNumberAvailable: function (context) {
       var formContext = context.getFormContext ? context.getFormContext() : context;
       var contactType = formContext.getAttribute("iso_contact_type");
       var isocsnumavail = formContext.getAttribute("iso_csnumberavailable").getValue();
       if (contactType.getValue() == "863050000") {
           if (isocsnumavail) {
               formContext.getControl("iso_cs_number").setDisabled(false);
               formContext.getAttribute("iso_cs_number").setRequiredLevel("required");
           } else {
               formContext.getControl("iso_cs_number").setDisabled(true);
               formContext.getAttribute("iso_cs_number").setValue(null);
               formContext.getAttribute("iso_cs_number").setRequiredLevel("none");
           }
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

   CreateSettingTask: function (context) {
       var formContext = context.getFormContext ? context.getFormContext() : context;
       var formMode = formContext.ui.getFormType();
       var contacttype = formContext.getAttribute("iso_contact_type").getValue();
       var tasknotification = formContext.getAttribute("iso_is_notification_task_created").getValue();
       var contactid = formContext.data.entity.getId();
       var lookupcontact = contactid.replace("{", "").replace("}", "");
       if (formMode === 2 && tasknotification) {
           IsoFairlens.iso_common.GenerateTask(formContext, lookupcontact, "contact", contacttype, new Date(), false, true);
       }
   },

   // Set default lookup to 'Active Correctional Centres'
   setDefaultCustomerLookupView: function (context) {
       var formContext = context.getFormContext ? context.getFormContext() : context;
       var customerLookup = formContext.getControl("parentcustomerid");
       var defaultViewId = "a81fbe6b-86d4-ee11-b84a-00505683fbf4"; // GUID of the 'Active Correctional Centres' view

       // Check if the form is in a state where the default view can be set (i.e., Create or Update)
       if (formContext.ui.getFormType() === 1 || formContext.ui.getFormType() === 2) {
           customerLookup.setDefaultView(defaultViewId);
       }
   }

}// JavaScript source code
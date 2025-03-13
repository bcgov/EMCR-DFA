if (typeof DFA === "undefined") {
    var DFA = {};
}

if (typeof DFA.Jscripts === "undefined") {
    DFA.Jscripts = {};
}


DFA.Jscripts.Contact = 
{

    //Added by Forrest: for EMBCDFA-233 to changed the labels
    ChangeAddress1CompositeLabels: function (executionContext) 
    {
        var formContext = executionContext.getFormContext();
        var fieldStreet1 = formContext.getControl("address1_composite_compositionLinkControl_address1_line1");
        var fieldStreet2 = formContext.getControl("address1_composite_compositionLinkControl_address1_line2");
        var fieldStreet3 = formContext.getControl("address1_composite_compositionLinkControl_address1_line3");
        var fieldCity    = formContext.getControl("address1_composite_compositionLinkControl_address1_city");
        var fieldStProv  = formContext.getControl("address1_composite_compositionLinkControl_address1_stateorprovince");
        var fieldPostalCode = formContext.getControl("address1_composite_compositionLinkControl_address1_postalcode");
        var fieldCountry = formContext.getControl("address1_composite_compositionLinkControl_address1_country");
        
        if (fieldStreet1 != null) fieldStreet1.setLabel("Street 1");
        if (fieldStreet2 != null) fieldStreet2.setLabel("Street 2");
        if (fieldStreet3 != null) fieldStreet3.setLabel("Street 3");
        if (fieldCity != null) fieldCity.setLabel("City");
        if (fieldStProv != null) fieldStProv.setLabel("State/Province");
        if (fieldPostalCode != null) fieldPostalCode.setLabel("Postal Code");
        if (fieldCountry != null) fieldCountry.setLabel("Country/Region");       
    }
    
}
 
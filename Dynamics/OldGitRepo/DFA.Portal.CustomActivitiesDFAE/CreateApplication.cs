using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;

namespace DFA.CRM.CustomWorkflow2
{
    /// <summary>
    /// Dynamics Workflow can only access information up to the next relationship
    /// This will allow any child entity of Case to access information from the related Event
    /// </summary>
    
        public class CreateApplication : CodeActivity
        {
            [Input("dfa_name")]
            public InArgument<string> Name { get; set; }

            [Input("dfa_accountlegalname")]
            public InArgument<string> AccountLegalName { get; set; }

            [Input("dfa_applicanttype")]
            public InArgument<int> ApplicantType { get; set; }
            [Input("dfa_applicanttype2")]
            public InArgument<int> ApplicantType2 { get; set; }
            [Input("dfa_doyouhaveinsurancecoverage2")]
            public InArgument<int> InsuranceCoverage { get; set; }


            [Input("dfa_primaryapplicationsignednoins")]
            public InArgument<int> PrimaryApplicantionsignednions { get; set; }
            [Input("dfa_primaryapplicantprintnamenoins")]
            public InArgument<string> PrimaryApplicantPrintNamenoins { get; set; }

            [Input("dfa_primaryapplicationsigneddatenoins")]
            public InArgument<DateTime> PimraryApplicationSignedDatenoins { get; set; }
            [Input("dfa_secondaryapplicantsignednoins")]
            public InArgument<int> SecondaryApplicantSignednoins { get; set; }


            [Input("dfa_secondaryapplicantprintnamenoins")]
            public InArgument<string> SecondaryApplicantPrintNamenoins { get; set; }

            [Input("dfa_secondaryapplicantsigneddatenoins")]
            public InArgument<DateTime> SecondaryApplicantSignedDatenoins { get; set; }

            [Input("dfa_appapplicationid")]
            public InArgument<string> applicationId { get; set; }
            // ticket update:
            [Input("dfa_isprimaryanddamagedaddresssame2")]
            public InArgument<int> IsPrimaryAndDamagedAddressSame { get; set; }
            [Input("dfa_damagedpropertystreet1")]
            public InArgument<string> DamagedPropertyStreet1 { get; set; }
            [Input("dfa_damagedpropertystreet2")]
            public InArgument<string> DamagedPropertyStreet2 { get; set; }
            [Input("dfa_damagedpropertycitytext")]
            public InArgument<string> DamagedPropertyCityText { get; set; }
            [Input("dfa_damagedpropertyprovince")]
            public InArgument<string> DamagedPropertyProvince { get; set; }
            [Input("dfa_damagedpropertypostalcode")]
            public InArgument<string> DamagedPropertyPostalCode { get; set; }
            [Input("dfa_isthispropertyyourp2")]
            public InArgument<int> IsPropertyyourRp { get; set; }
            [Input("dfa_indigenousreserve2")]
            public InArgument<int> Indigenousreserve { get; set; }
            [Input("dfa_nameoffirstnationsr")]
            public InArgument<string> NameOfFirstNationSr { get; set; }
            [Input("dfa_manufacturedhom2")]
            public InArgument<int> ManufacturedHome { get; set; }
            [Input("dfa_eligibleforbchomegrantonthisproperty2")]
            public InArgument<int> EligibleForBcHomeGrant { get; set; }
            [Input("dfa_contactfirstname")]
            public InArgument<string> ContactFirstName { get; set; }
            [Input("dfa_contactlastname")]
            public InArgument<string> ContactLastName { get; set; }
            [Input("dfa_contactphone1")]
            public InArgument<string> ContactPhone { get; set; }
            [Input("dfa_contactemail")]
            public InArgument<string> ContactEmail { get; set; }
        
            [Input("dfa_appcontactid")]
            public InArgument<string> AppContactId { get; set; }

            [Input("delete")]
            public InArgument<bool> Delete { get; set; }

            [Input("dfa_causeofdamageflood2")]
            public InArgument<int> CauseOfDamageFlood { get; set; }

            [Input("dfa_receiveguidanceassessingyourinfra")]
            public InArgument<int> ReceiveGuidanceAssessingYourInfo { get; set; }
            //New fields added.  EMCRI-164
            [Input("dfa_applicantlocalgovsubtype")]
            public InArgument<int> ApplicantLocalGovSubType { get; set; }
            [Input("dfa_applicantsubtype")]
            public InArgument<int> ApplicantSubType { get; set; }
            [Input("dfa_dfaapplicantsubtypecomments")]
            public InArgument<string> ApplicantSubTypeComments { get; set; }
        
            [Input("dfa_applicantothercomments")]
            public InArgument<string> ApplicantOtherComments { get; set; }

            [Input("dfa_estimated")]
            public InArgument<int> EstimatedPercentage { get; set; }
            [Input("dfa_causeofdamagestorm2")]
            public InArgument<int> CauseOfDamageStorm { get; set; }
            [Input("dfa_causeofdamagewildfire2")]
            public InArgument<int> CauseOfDamageWildFire { get; set; }
            [Input("dfa_causeofdamagelandslide2")]
            public InArgument<int> CauseOfDamageLandSlide { get; set; }
            [Input("dfa_causeofdamageother2")]
            public InArgument<int> CauseOfOtherDamage { get; set; }
            [Input("dfa_acopyofarentalagreementorlease2")]
            public InArgument<int> dfa_acopyofarentalagreementorlease2 { get; set; }
            [Input("dfa_areyounowresidingintheresidence2")]
            public InArgument<int> dfa_areyounowresidingintheresidence2 { get; set; }
            [Input("dfa_causeofdamageloss")]
            public InArgument<string> dfa_causeofdamageloss { get; set; }
            [Input("dfa_dateofdamage")]
            public InArgument<DateTime> dfa_dateofdamage { get; set; }
            [Input("dfa_dateofdamageto")]
            public InArgument<DateTime> dfa_dateofdamageto { get; set; }
            [Input("dfa_datereturntoresidence")]
            public InArgument<DateTime> dfa_datereturntoresidence { get; set; }
            [Input("dfa_description")]
            public InArgument<string> dfa_description { get; set; }
            [Input("dfa_doyourlossestotalmorethan10002")]
            public InArgument<int> dfa_doyourlossestotalmorethan10002 { get; set; }
            [Input("dfa_havereceiptsforcleanupsorrepairs")]
            public InArgument<int> dfa_havereceiptsforcleanupsorrepairs { get; set; }
            [Input("dfa_wereyouevacuatedduringtheevent2")]
            public InArgument<int> dfa_wereyouevacuatedduringtheevent2 { get; set; }
            [Input("dfa_primaryapplicantprintname")]
            public InArgument<string> dfa_primaryapplicantprintname { get; set; }
            [Input("dfa_primaryapplicantsigned")]
            public InArgument<int> dfa_primaryapplicantsigned { get; set; }
            [Input("dfa_primaryapplicantsigneddate")]
            public InArgument<DateTime> dfa_primaryapplicantsigneddate { get; set; }
            [Input("dfa_secondaryapplicantsigned")]
            public InArgument<int> dfa_secondaryapplicantsigned { get; set; }
            [Input("dfa_secondaryapplicantprintname")]
            public InArgument<string> dfa_secondaryapplicantprintname { get; set; }
            
            [Input("dfa_secondaryapplicantsigneddate")]
            public InArgument<DateTime> dfa_secondaryapplicantsigneddate { get; set; }
            
            [Input("dfa_portalloggedinuser")] 
            public InArgument<string> dfa_portalloggedinuser { get; set; }

            [Input("dfa_createdonportal")]
            public InArgument<bool> Createdonportal { get; set; }

            [Input("dfa_portalsubmitted")]
            public InArgument<bool> Portalsubmitted { get; set; }

            [Input("dfa_applicationcasebpfstages")]
            public InArgument<int> Applicationcasebpfstages { get; set; }
  
            [Input("dfa_applicationcasebpfsubstages")]
            public InArgument<int> Applicationcasebpfsubstages { get; set; }

            [Output("output")]
            public OutArgument<string> Application { get; set; }

            CodeActivityContext Activity;
            ITracingService Tracing;
            IWorkflowContext Workflow;
            IOrganizationServiceFactory ServiceFactory;
            IOrganizationService Service;


            protected override void Execute(CodeActivityContext context)
            {

                Activity = context;
                Tracing = Activity.GetExtension<ITracingService>();
                Workflow = Activity.GetExtension<IWorkflowContext>();
                ServiceFactory = Activity.GetExtension<IOrganizationServiceFactory>();
                Service = ServiceFactory.CreateOrganizationService(Workflow.UserId);
                // Get the input values
                var name = Name.Get(context);
                var applicantType = ApplicantType.Get(context);
                var applicantType2 = ApplicantType2.Get(context);
                var accountLegalName = AccountLegalName.Get(context);
                var insuranceCoverage = InsuranceCoverage.Get(context);
                var primaryApplicantionsignednoins = PrimaryApplicantionsignednions.Get(context);
                var primaryApplicantPrintNamenoins = PrimaryApplicantPrintNamenoins.Get(context);
                var pimraryApplicationSignedDatenoins = PimraryApplicationSignedDatenoins.Get(context);
                var secondaryApplicantSignednoins = SecondaryApplicantSignednoins.Get(context);
                var secondaryApplicantPrintNamenoins = SecondaryApplicantPrintNamenoins.Get(context);
                var secondaryApplicantSignedDatenoins = SecondaryApplicantSignedDatenoins.Get(context);
               // string appcontactId = AppContactId.Get(context);
                string appApplicationId = applicationId.Get(context);
                var isPrimaryAndDamagedAddressSame = IsPrimaryAndDamagedAddressSame.Get(context);
                var damagedPropertyStreet1 = DamagedPropertyStreet1.Get(context);
                var damagedPropertyStreet2 = DamagedPropertyStreet2.Get(context);
                var damagedPropertyCityText = DamagedPropertyCityText.Get(context);
                var damagedPropertyProvince = DamagedPropertyProvince.Get(context);
                var damagedPropertyPostalCode = DamagedPropertyPostalCode.Get(context);
                var isPropertyyourRp = IsPropertyyourRp.Get(context);
                var indigenousreserve = Indigenousreserve.Get(context);
                var nameOfFirstNationSr = NameOfFirstNationSr.Get(context);
                var manufacturedHome = ManufacturedHome.Get(context);
                //   var eligibleForBcHomeGrant = EligibleForBcHomeGrant.Get(context);
                var contactFirstName = ContactFirstName.Get(context);
                var contactLastName = ContactLastName.Get(context);
                var contactPhone = ContactPhone.Get(context);
                var contactEmail = ContactEmail.Get(context);
                var causeOfDamageFlood = CauseOfDamageFlood.Get(context);
                var causeOfDamageStorm = CauseOfDamageStorm.Get(context);
                var receiveguidance = ReceiveGuidanceAssessingYourInfo.Get(context);
                var causeOfDamageWildFire = CauseOfDamageWildFire.Get(context);
                var causeOfDamageLandSlide = CauseOfDamageLandSlide.Get(context);
                var causeOfOtherDamage = CauseOfOtherDamage.Get(context);
                var acopyofarentalagreementorlease = dfa_acopyofarentalagreementorlease2.Get(context);
                var areyounowresidingintheresidence = dfa_areyounowresidingintheresidence2.Get(context);
                var causeofdamageloss = dfa_causeofdamageloss.Get(context);
                var dateofdamage = dfa_dateofdamage.Get(context);
                var dateofdamageto = dfa_dateofdamageto.Get(context);
                var datereturntoresidence = dfa_datereturntoresidence.Get(context);
                var description = dfa_description.Get(context);
                var doyourlossestotalmorethan1000 = dfa_doyourlossestotalmorethan10002.Get(context);
                var havereceiptsforcleanupsorrepairs = dfa_havereceiptsforcleanupsorrepairs.Get(context);
                var wereyouevacuatedduringtheevent = dfa_wereyouevacuatedduringtheevent2.Get(context);
                var primaryapplicantprintname = dfa_primaryapplicantprintname.Get(context);
                var primaryapplicantsigned = dfa_primaryapplicantsigned.Get(context);
                var primaryapplicantsigneddate = dfa_primaryapplicantsigneddate.Get(context);
                var secondaryapplicantsigned = dfa_secondaryapplicantsigned.Get(context);
                var secondaryapplicantprintname = dfa_secondaryapplicantprintname.Get(context);
                var secondaryapplicantsigneddate = dfa_secondaryapplicantsigneddate.Get(context);
                var eligibleForBcHomeGrant = EligibleForBcHomeGrant.Get(context);
                var portalloggedinuser = dfa_portalloggedinuser.Get(context);
             
                //New fields. EMCR-164
                var applicantlocalgovsubtype = ApplicantLocalGovSubType.Get(context);
                var applicantsubtype = ApplicantSubType.Get(context);
                var dfaapplicantsubtypecomments = ApplicantSubTypeComments.Get(context);
                var applicantothercomments = ApplicantOtherComments.Get(context);
                var estimated = EstimatedPercentage.Get(context);

                var dfa_createdonportal = Createdonportal.Get(context);
                var dfa_portalsubmitted = Portalsubmitted.Get(context);
                var dfa_applicationcasebpfstages = Applicationcasebpfstages.Get(context);
                var dfa_applicationcasebpfsubstages = Applicationcasebpfsubstages.Get(context); 


                bool delete = Delete.Get(context);

                if (delete)
                {
                    Tracing.Trace("deleting the guid id : " + appApplicationId);
                    Service.Delete("dfa_appapplication", new Guid(appApplicationId));
                    Application.Set(context, "deleted");
                    Tracing.Trace("deleted the guid id : " + Application);
                    return;
                }
                if (!string.IsNullOrEmpty(appApplicationId))
                {
                    Tracing.Trace("retrieving the application with ID: " + appApplicationId);
                    Entity currentApplication = Service.Retrieve("dfa_appapplication", new Guid(appApplicationId), new ColumnSet("dfa_buildingownerlandlord"));
                    Entity appApplication = new Entity("dfa_appapplication");
                    Entity buildingOwnerLandLord = new Entity("dfa_appbuildingownerlandlord");
                    Tracing.Trace("in the update block:");
                    appApplication.Id = new Guid(appApplicationId);
                    if (applicantType != null)
                    {
                        Tracing.Trace("in the application type block:");
                        if (applicantType == 222710000 || applicantType == 222710001 || applicantType == 222710002 || applicantType == 222710003 || applicantType == 222710004 || applicantType == 222710005 || applicantType == 222710006)
                            appApplication["dfa_applicanttype"] = new OptionSetValue(applicantType);
                    }
                    if (applicantType2 != null)
                    {

                        Tracing.Trace("dfa_applicanttype2:");
                        if (applicantType2 == 222710000 || applicantType2 == 222710006)
                            appApplication["dfa_applicanttype2"] = new OptionSetValue(applicantType2);
                    }
                    //New field. EMCRI-164
                    if (applicantlocalgovsubtype != null)
                    {
                        Tracing.Trace("dfa_applicantlocalgovsubtype:");
                        if (applicantlocalgovsubtype == 222710000 || applicantlocalgovsubtype == 222710001 || applicantlocalgovsubtype == 222710002 ||
                            applicantlocalgovsubtype == 222710003 || applicantlocalgovsubtype == 222710004 || applicantlocalgovsubtype == 222710005 ||
                            applicantlocalgovsubtype == 222710006 || applicantlocalgovsubtype == 222710007 || applicantlocalgovsubtype == 222710008 ||
                            applicantlocalgovsubtype == 222710009 || applicantlocalgovsubtype == 222710010 || applicantlocalgovsubtype == 222710011 ||
                            applicantlocalgovsubtype == 222710012 || applicantlocalgovsubtype == 222710013)
                            appApplication["dfa_applicantlocalgovsubtype"] = new OptionSetValue(applicantlocalgovsubtype);
                    }
                    if (applicantsubtype != null)
                    {
                        Tracing.Trace("dfa_applicantsubtype:");
                        if (applicantsubtype == 222710000)
                            appApplication["dfa_applicantsubtype"] = new OptionSetValue(applicantsubtype);
                    }
                
                    if (!string.IsNullOrEmpty(dfaapplicantsubtypecomments))
                    {
                        Tracing.Trace("dfa_dfaapplicantsubtypecomments:");
                        appApplication["dfa_dfaapplicantsubtypecomments"] = dfaapplicantsubtypecomments;
                    }
                    if (!string.IsNullOrEmpty(applicantothercomments))
                    {
                        Tracing.Trace("dfa_applicantothercomments:");
                        appApplication["dfa_applicantothercomments"] = applicantothercomments;
                    }
                    if (estimated != null)
                    {
                        Tracing.Trace("dfa_estimated:");
                        appApplication["dfa_estimated"] = estimated;
                    }

                    if (insuranceCoverage != null)
                    {
                        Tracing.Trace("dfa_doyouhaveinsurancecoverage2:");
                        if (insuranceCoverage == 222710000 || insuranceCoverage == 222710001 || insuranceCoverage == 222710002)
                            appApplication["dfa_doyouhaveinsurancecoverage2"] = new OptionSetValue(insuranceCoverage);
                    }
                    if (!string.IsNullOrEmpty(accountLegalName))
                    {

                        Tracing.Trace("dfa_accountlegalname:");
                        appApplication["dfa_accountlegalname"] = accountLegalName;
                    }
                    if (!string.IsNullOrEmpty(name))
                    {
                        Tracing.Trace("dfa_governmentbodylegalname:");
                        appApplication["dfa_governmentbodylegalname"] = name;
                    }
                    if (primaryApplicantionsignednoins == 222710000 || primaryApplicantionsignednoins == 222710001)
                    {

                        Tracing.Trace("dfa_primaryapplicantsigneddatenoins:");
                        appApplication["dfa_primaryapplicantsigneddatenoins"] = new OptionSetValue(primaryApplicantionsignednoins);
                    }

                    if (!string.IsNullOrEmpty(primaryApplicantPrintNamenoins))
                    {

                        Tracing.Trace("dfa_primaryapplicantprintnamenoins:");
                        appApplication["dfa_primaryapplicantprintnamenoins"] = primaryApplicantPrintNamenoins;
                    }
                    if (pimraryApplicationSignedDatenoins.Year == DateTime.Now.Year)
                    {

                        Tracing.Trace("dfa_primaryapplicantsigneddatenoins:");
                        appApplication["dfa_primaryapplicantsigneddatenoins"] = pimraryApplicationSignedDatenoins;
                    }
                    if (secondaryApplicantSignednoins == 222710000 || secondaryApplicantSignednoins == 222710001)
                    {
                        Tracing.Trace("dfa_secondaryapplicantsignednoins:");
                        appApplication["dfa_secondaryapplicantsignednoins"] = new OptionSetValue(secondaryApplicantSignednoins);
                    }
                    if (!string.IsNullOrEmpty(secondaryApplicantPrintNamenoins))
                    {
                        Tracing.Trace("dfa_secondaryapplicantprintnamenoins:");
                        appApplication["dfa_secondaryapplicantprintnamenoins"] = secondaryApplicantPrintNamenoins;
                    }
                    if (secondaryApplicantSignedDatenoins.Year == DateTime.Now.Year)
                    {
                        Tracing.Trace("dfa_secondaryapplicantsigneddatenoins:");
                        appApplication["dfa_secondaryapplicantsigneddatenoins"] = secondaryApplicantSignedDatenoins;
                    }
                //if (appcontactId != null)
                //{
                //    Tracing.Trace("dfa_applicant:");
                //    appApplication["dfa_applicant"] = new EntityReference("dfa_appcontact", new Guid(appcontactId));
                //}
               
                  
                if (!string.IsNullOrEmpty(portalloggedinuser))
                {
                    Tracing.Trace("dfa_portalloggedinuser:");
                    appApplication["dfa_bceiduser"] = new EntityReference("dfa_bceiduser", new Guid(portalloggedinuser));
                }
                //ticket update work
                if (isPrimaryAndDamagedAddressSame != null && (isPrimaryAndDamagedAddressSame == 222710000 || isPrimaryAndDamagedAddressSame == 222710001))
                    {
                        Tracing.Trace("dfa_isprimaryanddamagedaddresssame2:");
                        appApplication["dfa_isprimaryanddamagedaddresssame2"] = new OptionSetValue(isPrimaryAndDamagedAddressSame);
                    }
                    if (!string.IsNullOrEmpty(damagedPropertyStreet1))
                    {
                        Tracing.Trace("dfa_damagedpropertystreet1:");
                        appApplication["dfa_damagedpropertystreet1"] = damagedPropertyStreet1;
                    }
                    if (!string.IsNullOrEmpty(damagedPropertyStreet2))
                    {
                        Tracing.Trace("dfa_damagedpropertystreet2:");
                        appApplication["dfa_damagedpropertystreet2"] = damagedPropertyStreet2;
                    }
                    if (!string.IsNullOrEmpty(damagedPropertyCityText))
                    {
                        Tracing.Trace("dfa_damagedpropertycitytext:");
                        appApplication["dfa_damagedpropertycitytext"] = damagedPropertyCityText;
                    }
                    if (!string.IsNullOrEmpty(damagedPropertyProvince))
                    {
                        Tracing.Trace("dfa_damagedpropertyprovince:");
                        appApplication["dfa_damagedpropertyprovince"] = damagedPropertyProvince;
                    }
                    if (!string.IsNullOrEmpty(damagedPropertyPostalCode))
                    {
                        Tracing.Trace("dfa_damagedpropertypostalcode:");
                        appApplication["dfa_damagedpropertypostalcode"] = damagedPropertyPostalCode;
                    }
                    if (isPropertyyourRp != null && (isPropertyyourRp == 222710000 || isPropertyyourRp == 222710001))
                    {
                        Tracing.Trace("dfa_isthispropertyyourp2:");
                        appApplication["dfa_isthispropertyyourp2"] = new OptionSetValue(isPropertyyourRp);
                    }
                    if (indigenousreserve != null && (indigenousreserve == 222710000 || indigenousreserve == 222710001))
                    {
                        Tracing.Trace("dfa_indigenousreserve2:");
                        appApplication["dfa_indigenousreserve2"] = new OptionSetValue(indigenousreserve);
                    }
                    if (!string.IsNullOrEmpty(nameOfFirstNationSr))
                    {
                        Tracing.Trace("dfa_nameoffirstnationsr:");
                        appApplication["dfa_nameoffirstnationsr"] = nameOfFirstNationSr;
                    }
                    if (manufacturedHome != null && (manufacturedHome == 222710000 || manufacturedHome == 222710001))
                    {
                        Tracing.Trace("dfa_manufacturedhom2:");
                        appApplication["dfa_manufacturedhom2"] = new OptionSetValue(manufacturedHome);
                    }
                    // last update work

                    if (causeOfDamageFlood != null && (causeOfDamageFlood == 222710000 || causeOfDamageFlood == 222710001))
                    {
                        Tracing.Trace("dfa_causeofdamageflood2:");
                        appApplication["dfa_causeofdamageflood2"] = new OptionSetValue(causeOfDamageFlood);
                    }
                    if (causeOfDamageStorm != null && (causeOfDamageStorm == 222710000 || causeOfDamageStorm == 222710001))
                    {
                        Tracing.Trace("dfa_causeofdamagestorm2:");
                        appApplication["dfa_causeofdamagestorm2"] = new OptionSetValue(causeOfDamageStorm);
                    }
                    if (receiveguidance != null && (receiveguidance == 222710000 || receiveguidance == 222710001))
                    {
                        Tracing.Trace("dfa_receiveguidanceassessingyourinfra:");
                        appApplication["dfa_receiveguidanceassessingyourinfra"] = new OptionSetValue(receiveguidance);
                    }
                    if (causeOfDamageWildFire != null && (causeOfDamageWildFire == 222710000 || causeOfDamageWildFire == 222710001))
                    {
                        Tracing.Trace("dfa_causeofdamagewildfire2:");
                        appApplication["dfa_causeofdamagewildfire2"] = new OptionSetValue(causeOfDamageWildFire);
                    }
                    if (causeOfDamageLandSlide != null && (causeOfDamageLandSlide == 222710000 || causeOfDamageLandSlide == 222710001))
                    {
                        Tracing.Trace("dfa_causeofdamagelandslide2:");
                        appApplication["dfa_causeofdamagelandslide2"] = new OptionSetValue(causeOfDamageLandSlide);
                    }
                    if (causeOfOtherDamage != null && (causeOfDamageLandSlide == 222710000 || causeOfDamageLandSlide == 222710001))
                    {
                        Tracing.Trace("dfa_causeofdamageother2:");
                        appApplication["dfa_causeofdamageother2"] = new OptionSetValue(causeOfOtherDamage);
                    }
                    if (acopyofarentalagreementorlease != null && (acopyofarentalagreementorlease == 222710000 || acopyofarentalagreementorlease == 222710001))
                    {
                        Tracing.Trace("dfa_acopyofarentalagreementorlease2:");
                        appApplication["dfa_acopyofarentalagreementorlease2"] = new OptionSetValue(acopyofarentalagreementorlease);
                    }
                    if (areyounowresidingintheresidence != null && (areyounowresidingintheresidence == 222710000 || areyounowresidingintheresidence == 222710001))
                    {
                        Tracing.Trace("dfa_areyounowresidingintheresidence2:");
                        appApplication["dfa_areyounowresidingintheresidence2"] = new OptionSetValue(areyounowresidingintheresidence);
                    }
                    if (!string.IsNullOrEmpty(causeofdamageloss))
                    {
                        Tracing.Trace("dfa_causeofdamageloss:");
                        appApplication["dfa_causeofdamageloss"] = causeofdamageloss;
                    }
                    if (dateofdamage != new DateTime())
                    {
                        Tracing.Trace("dfa_dateofdamage:");
                        appApplication["dfa_dateofdamage"] = dateofdamage;
                    }
                    if (dateofdamageto != new DateTime())
                    {
                        Tracing.Trace("dfa_dateofdamageto:");
                        appApplication["dfa_dateofdamageto"] = dateofdamageto;
                    }
                    if (datereturntoresidence != new DateTime())
                    {
                        Tracing.Trace("dfa_datereturntotheresidence:");
                        appApplication["dfa_datereturntotheresidence"] = datereturntoresidence;
                    }
                    if (!string.IsNullOrEmpty(description))
                    {
                        Tracing.Trace("dfa_description:");
                        appApplication["dfa_description"] = description;
                    }
                    if (doyourlossestotalmorethan1000 != null && (doyourlossestotalmorethan1000 == 222710000 || doyourlossestotalmorethan1000 == 222710001))
                    {
                        Tracing.Trace("dfa_doyourlossestotalmorethan10002:");
                        appApplication["dfa_doyourlossestotalmorethan10002"] = new OptionSetValue(doyourlossestotalmorethan1000);
                    }
                    if (havereceiptsforcleanupsorrepairs != null && (havereceiptsforcleanupsorrepairs == 222710000 || havereceiptsforcleanupsorrepairs == 222710001))
                    {
                        Tracing.Trace("dfa_haveinvoicesreceiptsforcleanuporrepairs2:");
                        appApplication["dfa_haveinvoicesreceiptsforcleanuporrepairs2"] = new OptionSetValue(havereceiptsforcleanupsorrepairs);
                    }
                    if (wereyouevacuatedduringtheevent != null && (wereyouevacuatedduringtheevent == 222710000 || wereyouevacuatedduringtheevent == 222710001))
                    {
                        Tracing.Trace("dfa_wereyouevacuatedduringtheevent2:");
                        appApplication["dfa_wereyouevacuatedduringtheevent2"] = new OptionSetValue(wereyouevacuatedduringtheevent);
                    }
                    if (!string.IsNullOrEmpty(primaryapplicantprintname))
                    {
                        Tracing.Trace("dfa_primaryapplicantprintname:");
                        appApplication["dfa_primaryapplicantprintname"] = primaryapplicantprintname;
                    }
                    if (!string.IsNullOrEmpty(name))
                    {
                        Tracing.Trace("dfa_governmentbodylegalname:");
                        appApplication["dfa_governmentbodylegalname"] = name;
                    }
                    if (primaryapplicantsigned == 222710000 || primaryapplicantsigned == 222710001)
                    {
                        Tracing.Trace("dfa_primaryapplicantsigned:");
                        appApplication["dfa_primaryapplicantsigned"] = new OptionSetValue(primaryapplicantsigned);
                    }
                    if (primaryapplicantsigneddate != new DateTime())
                    {
                        Tracing.Trace("dfa_primaryapplicantsigneddate:");
                        appApplication["dfa_primaryapplicantsigneddate"] = primaryapplicantsigneddate;
                    }
                    if (secondaryapplicantsigned == 222710000 || secondaryapplicantsigned == 222710001)
                    {
                        Tracing.Trace("dfa_secondaryapplicantsigned:");
                        appApplication["dfa_secondaryapplicantsigned"] = new OptionSetValue(secondaryapplicantsigned);
                    }
                    if (secondaryapplicantprintname != null)
                    {
                        Tracing.Trace("dfa_secondaryapplicantprintname:");
                        appApplication["dfa_secondaryapplicantprintname"] = secondaryapplicantprintname;
                    }
                    if (secondaryapplicantsigneddate != new DateTime())
                    {
                        Tracing.Trace("dfa_secondaryapplicantsigneddate:");
                        appApplication["dfa_secondaryapplicantsigneddate"] = secondaryapplicantsigneddate;
                    }

                    if (eligibleForBcHomeGrant == 222710000 || eligibleForBcHomeGrant == 222710001)
                    {
                        Tracing.Trace("dfa_eligibleforbchomegrantonthisproperty2:");
                        appApplication["dfa_eligibleforbchomegrantonthisproperty2"] = new OptionSetValue(eligibleForBcHomeGrant);
                    }
                    if (!string.IsNullOrEmpty(contactFirstName))
                    {
                        Tracing.Trace("dfa_contactfirstname:");
                        buildingOwnerLandLord["dfa_contactfirstname"] = contactFirstName;
                    }

                    if (!string.IsNullOrEmpty(contactLastName))
                    {
                        Tracing.Trace("dfa_contactlastname:");
                        buildingOwnerLandLord["dfa_contactlastname"] = contactLastName;
                    }
                    if (!string.IsNullOrEmpty(contactPhone))
                    {
                        Tracing.Trace("dfa_contactphone1:");
                        buildingOwnerLandLord["dfa_contactphone1"] = contactPhone;
                    }
                    if (!string.IsNullOrEmpty(contactEmail))
                    {
                        Tracing.Trace("dfa_contactemail:");
                        buildingOwnerLandLord["dfa_contactemail"] = contactEmail;
                    }

                    // last update
               
                    appApplication["dfa_createdonportal"] = true;

                    appApplication["dfa_portalsubmitted"] = dfa_portalsubmitted;

                    if (dfa_applicationcasebpfstages != null && (dfa_applicationcasebpfstages == 222710000 || dfa_applicationcasebpfstages == 222710001 ||
                        dfa_applicationcasebpfstages == 222710002 || dfa_applicationcasebpfstages == 222710003 || dfa_applicationcasebpfstages == 222710004 || 
                        dfa_applicationcasebpfstages == 222710005 || dfa_applicationcasebpfstages == 222710006))
                    {
                        Tracing.Trace("dfa_applicationcasebpfstages:");
                        appApplication["dfa_applicationcasebpfstages"] = new OptionSetValue(dfa_applicationcasebpfstages);
                    }
                    if (dfa_applicationcasebpfsubstages != null && (dfa_applicationcasebpfsubstages == 222710000))
                    {
                        Tracing.Trace("dfa_applicationcasebpfsubstages:");
                        appApplication["dfa_applicationcasebpfsubstages"] = new OptionSetValue(dfa_applicationcasebpfsubstages);
                    }

                    if (currentApplication.GetAttributeValue<EntityReference>("dfa_buildingownerlandlord") != null)
                        {
                            Tracing.Trace("landloard");
                            // update buildingOwnerLandLord
                            buildingOwnerLandLord.Id = currentApplication.GetAttributeValue<EntityReference>("dfa_buildingownerlandlord").Id;
                            Service.Update(buildingOwnerLandLord);
                            Tracing.Trace("Updated the building Owner Landlord with id: " + buildingOwnerLandLord.Id);
                        }
                        if (currentApplication.GetAttributeValue<EntityReference>("dfa_buildingownerlandlord") == null)
                        {
                            Tracing.Trace("dfa_buildingownerlandlord:");
                            var buidlingLandLordId = Service.Create(buildingOwnerLandLord);
                            Tracing.Trace("Created the building Owner Landlord with id: " + buidlingLandLordId);
                            appApplication["dfa_buildingownerlandlord"] = new EntityReference("dfa_appbuildingownerlandlord", buidlingLandLordId);
                        }
                        Service.Update(appApplication);
                        Tracing.Trace("appApplication is updated with guid:");
                        Application.Set(context, "Updated");


                    }
                else
                {
                    // Create
                    Entity appApplication = new Entity("dfa_appapplication");
                    appApplication["dfa_governmentbodylegalname"] = name;
                    if (applicantType == 222710000 || applicantType == 222710001 || applicantType == 222710002 || applicantType == 222710003 || applicantType == 222710004 || applicantType == 222710005 || applicantType == 222710006)
                        appApplication["dfa_applicanttype"] = new OptionSetValue(applicantType);
                    if (insuranceCoverage == 222710000 || insuranceCoverage == 222710001 || insuranceCoverage == 222710002)
                        appApplication["dfa_doyouhaveinsurancecoverage2"] = new OptionSetValue(insuranceCoverage);
                    appApplication["dfa_accountlegalname"] = accountLegalName;
                    if (primaryApplicantionsignednoins == 222710000 || primaryApplicantionsignednoins == 222710001)
                        appApplication["dfa_primaryapplicantsignednoins"] = new OptionSetValue(primaryApplicantionsignednoins);

                    appApplication["dfa_primaryapplicantprintnamenoins"] = primaryApplicantPrintNamenoins;
                    if (pimraryApplicationSignedDatenoins != new DateTime())
                        appApplication["dfa_primaryapplicantsigneddatenoins"] = pimraryApplicationSignedDatenoins;
                    if (secondaryApplicantSignednoins == 222710000 || secondaryApplicantSignednoins == 222710001)
                        appApplication["dfa_secondaryapplicantsignednoins"] = new OptionSetValue(secondaryApplicantSignednoins);
                    appApplication["dfa_secondaryapplicantprintnamenoins"] = secondaryApplicantPrintNamenoins;
                    if (secondaryApplicantSignedDatenoins != new DateTime())
                        appApplication["dfa_secondaryapplicantsigneddatenoins"] = secondaryApplicantSignedDatenoins;
                    if (doyourlossestotalmorethan1000 != null && (doyourlossestotalmorethan1000 == 222710000 || doyourlossestotalmorethan1000 == 222710001))
                    {
                        appApplication["doyourlossestotalmorethan1000"] = new OptionSetValue(doyourlossestotalmorethan1000);
                    }

                    //Expansion project - new fields update
                    if (primaryApplicantionsignednoins == 222710000 || primaryApplicantionsignednoins == 222710001)
                        appApplication["dfa_primaryapplicantsignednoins"] = new OptionSetValue(primaryApplicantionsignednoins);


                if (causeOfDamageFlood == 222710000 || causeOfDamageFlood == 222710001)
                {
                    appApplication["dfa_causeofdamageflood2"] = new OptionSetValue(causeOfDamageFlood);
                }
                if (causeOfDamageStorm == 222710000 || causeOfDamageStorm == 222710001)
                {
                    appApplication["dfa_causeofdamagestorm2"] = new OptionSetValue(causeOfDamageStorm);
                }
                if (receiveguidance == 222710000 || receiveguidance == 222710001)
                {
                    appApplication["dfa_receiveguidanceassessingyourinfra"] = new OptionSetValue(receiveguidance);
                }

                //New Fields.  EMCRI-164
                if (applicantlocalgovsubtype == 222710000 || applicantlocalgovsubtype == 222710001 || applicantlocalgovsubtype == 222710002 ||
                        applicantlocalgovsubtype == 222710003 || applicantlocalgovsubtype == 222710004 || applicantlocalgovsubtype == 222710005 ||
                        applicantlocalgovsubtype == 222710006 || applicantlocalgovsubtype == 222710007 || applicantlocalgovsubtype == 222710008 ||
                        applicantlocalgovsubtype == 222710009 || applicantlocalgovsubtype == 222710010 || applicantlocalgovsubtype == 222710011 ||
                        applicantlocalgovsubtype == 222710012 || applicantlocalgovsubtype == 222710013)
                {
                    appApplication["dfa_applicantlocalgovsubtype"] = new OptionSetValue(applicantlocalgovsubtype);
                }

                if (applicantsubtype == 222710000 || applicantsubtype == 222710001 || applicantsubtype == 222710002 ||
                    applicantsubtype == 222710003 || applicantsubtype == 222710004)
                {
                    appApplication["dfa_applicantsubtype"] = new OptionSetValue(applicantsubtype);
                }

                if (!string.IsNullOrEmpty(dfaapplicantsubtypecomments))
                {
                    appApplication["dfa_dfaapplicantsubtypecomments"] = dfaapplicantsubtypecomments;
                }
                if (!string.IsNullOrEmpty(applicantothercomments))
                {
                    appApplication["dfa_applicantothercomments"] = applicantothercomments;
                }
                if (estimated != null)
                {
                    appApplication["dfa_estimated"] = estimated;
                }

                if (causeOfDamageWildFire == 222710000 || causeOfDamageWildFire == 222710001)
                {
                    appApplication["dfa_causeofdamagewildfire2"] = new OptionSetValue(causeOfDamageWildFire);
                }
                if (causeOfDamageLandSlide == 222710000 || causeOfDamageLandSlide == 222710001)
                {
                    appApplication["dfa_causeofdamagelandslide2"] = new OptionSetValue(causeOfDamageLandSlide);
                }
                if (causeOfDamageLandSlide == 222710000 || causeOfDamageLandSlide == 222710001)
                {
                    appApplication["dfa_causeofdamageother2"] = new OptionSetValue(causeOfOtherDamage);
                }
                if (!string.IsNullOrEmpty(name))
                {
                    Tracing.Trace("dfa_governmentbodylegalname:");
                    appApplication["dfa_governmentbodylegalname"] = name;
                }
                if (!string.IsNullOrEmpty(causeofdamageloss))
                {
                    appApplication["dfa_causeofdamageloss"] = causeofdamageloss;
                }
                if (dateofdamage != new DateTime())
                {
                    appApplication["dfa_dateofdamage"] = dateofdamage;
                }
                if (dateofdamageto != new DateTime())
                {
                    appApplication["dfa_dateofdamageto"] = dateofdamageto;
                }
                if (!string.IsNullOrEmpty(portalloggedinuser))
                {
                    Tracing.Trace("dfa_portalloggedinuser:");
                    appApplication["dfa_bceiduser"] = new EntityReference("dfa_bceiduser", new Guid(portalloggedinuser));
                }
                //A Boolean.
                appApplication["dfa_createdonportal"] = true;
                appApplication["dfa_portalsubmitted"] = dfa_portalsubmitted;

                if (dfa_applicationcasebpfstages != null && (dfa_applicationcasebpfstages == 222710000 || dfa_applicationcasebpfstages == 222710001 ||
                    dfa_applicationcasebpfstages == 222710002 || dfa_applicationcasebpfstages == 222710003 || dfa_applicationcasebpfstages == 222710004 ||
                    dfa_applicationcasebpfstages == 222710005 || dfa_applicationcasebpfstages == 222710006))
                {
                    Tracing.Trace("dfa_applicationcasebpfstages:");
                    appApplication["dfa_applicationcasebpfstages"] = new OptionSetValue(dfa_applicationcasebpfstages);
                }
                if (dfa_applicationcasebpfsubstages != null && (dfa_applicationcasebpfsubstages == 222710000))
                {
                    Tracing.Trace("dfa_applicationcasebpfsubstages:");
                    appApplication["dfa_applicationcasebpfsubstages"] = new OptionSetValue(dfa_applicationcasebpfsubstages);
                }
                var appApplicationGuid = Service.Create(appApplication);
                    Tracing.Trace("appApplicationGuid created is : " + appApplicationGuid.ToString());
                    Application.Set(context, appApplicationGuid.ToString());
                }

            }

        }

}

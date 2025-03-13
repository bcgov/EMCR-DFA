using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DFA.CRM.Plugins
{
    /// <summary>
    /// This is to populate Public Sector Weekly Report Number
    /// 
    /// </summary>
    public class PopulatePublicSectorStatusReport : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            //Extract the tracing service for use in debugging sandboxed plug-ins.
            var Tracing =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            // Obtain the execution context from the service provider.
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            Tracing.Trace("context");
            // Obtain the organization service reference.
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            Tracing.Trace("servicefactory");
            var Service = serviceFactory.CreateOrganizationService(context.UserId);
            Tracing.Trace("contextuser" + context.UserId.ToString());
            Tracing.Trace("service" + Service.ToString());
            Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Message Name: {context.MessageName}");
            try
            {
                if (context.MessageName != "Create")
                {
                    // Do Nothing
                    return;
                }
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Obtain Target Entity");
                var entTarget = (Entity)context.InputParameters["Target"];
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Obtain Event Lookup");
                var eventER = entTarget.GetAttributeValue<EntityReference>("dfa_eventid");
                if (eventER == null)
                {
                    // Do Nothing
                    return;
                }
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Event Lookup: {eventER.Name} - {eventER.Id}");
                var eventRecord = Service.Retrieve("dfa_event", eventER.Id, new ColumnSet(true));
                var eventName = eventRecord.GetAttributeValue<string>("dfa_id");
                var caseQuery = new QueryExpression("incident") { ColumnSet = new ColumnSet(true) };
                caseQuery.Criteria.AddCondition("dfa_applicanttype", ConditionOperator.Equal, 222710005); // Public Sector
                caseQuery.Criteria.AddCondition("dfa_eventid", ConditionOperator.Equal, eventER.Id);
                var caseResult = Service.RetrieveMultiple(caseQuery);
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Query for Case that is Public Sector of this Event: {caseResult.Entities.Count}");
                var communitiesWithApprovedProject = 0;
                var numberOfApprovedProjectsInEvent = 0;
                var numberOfCliamPaidInEvent = 0;
                var communitiesWithProject = 0;
                var caseStillOpen = 0;
                var caseClosedIneligible = 0;
                var caseClosedWithdrawn = 0;
                var caseClosedCompleted = 0;
                var caseClosedWithoutPayment = 0;
                var numberOfProjectsApprovedInEvent = 0;
                var numberOfProjectsClosedCompletedInEvent = 0;
                var numberOfProjectsClosedIneligibleInEvent = 0;
                var numberOfProjectsPendingAdjudicationInEvent = 0;
                var numberOfProjectsPendingApprovalInEvent = 0;
                var numberOfProjectsWithdrawnInEvent = 0;
                var totalApprovedCRPValuesInEvent = 0m;
                var totalClaimsPaidValueInEvent = 0m;
                var totalCRPValueInEvent = 0m;
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Loop Through all the cases to review case status and build child record data");
                foreach (var caseRecord in caseResult.Entities)
                {
                    // Create the sub entity
                    var childRecord = new Entity("dfa_publicsectorreportcommunitiesdata");
                    childRecord["dfa_case"] = caseRecord.ToEntityReference();
                    childRecord["dfa_publicsectorreportid"] = entTarget.ToEntityReference();
                    var caseState = caseRecord.GetAttributeValue<OptionSetValue>("statecode").Value;
                    var caseStatus = caseRecord.GetAttributeValue<OptionSetValue>("statuscode").Value;
                    switch (caseStatus)
                    {
                        case 6: // Cancelled
                        case 2000: // Merged
                        case 3: // Not Used 2
                        case 4: // Not Used 3
                            // Not Used
                            break;
                        case 1: // Open
                            caseStillOpen++;
                            break;
                        case 2: // Appeal
                            caseStillOpen++;
                            break;
                        case 222710000: // Ineligible and Closed
                            caseClosedIneligible++;
                            break;
                        case 222710001: // Withdrawn and Closed
                            caseClosedWithdrawn++;
                            break;
                        case 1000: // Closed Without Payment
                            caseClosedWithoutPayment++;
                            break;
                        case 5: // Closed
                            caseClosedCompleted++;
                            break;

                    }
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Case Status: {caseStatus}");
                    childRecord["dfa_iscaseclosed"] = (caseState != 0); // Not active

                    // Effected Community Information
                    var effectedCommunityER = caseRecord.GetAttributeValue<EntityReference>("dfa_effectedcommunityregionid");
                    childRecord["dfa_community"] = effectedCommunityER;
                    if (effectedCommunityER != null)
                    {
                        var effectedCommunity = Service.Retrieve("dfa_effectedregioncommunity", effectedCommunityER.Id, new ColumnSet(true));
                        childRecord["dfa_communityisindigenous"] = effectedCommunity.GetAttributeValue<bool>("dfa_isindigenous");
                        childRecord["dfa_name"] = $"{effectedCommunityER.Name} - {eventName} -  {DateTime.Now.ToString("MMM d, yyyy")}";
                        childRecord["dfa_embcregion"] = effectedCommunity.GetAttributeValue<EntityReference>("dfa_regionid");
                    }
                    else
                    {
                        childRecord["dfa_name"] = $"{caseRecord.ToEntityReference().Name} - {eventName} -  {DateTime.Now.ToString("MMM d, yyyy")}";
                    }
                    var numberOfPendingAdjudicationProjects = 0;
                    var numberOfApprovedProjects = 0;
                    var numberOfCompletedProjects = 0;
                    var numberOfIneligibleProjects = 0;
                    var numberOfWithdrawnProjects = 0;
                    var numberOfPendingApprovalProjects = 0;

                    childRecord["dfa_costrecoveryplanreceiveddate"] = caseRecord.GetAttributeValue<DateTime>("dfa_datereceived");
                    var numberOfCostRecoveryPlanReceived = 0; // Does this mean number of projects?
                    var approvedCost = 0m;
                    var trueForecast = 0m; // EMBCDFA-322
                    var amountPaid = 0m;
                    var firstFiscal = 0;
                    var secondFiscal = 0;
                    var thirdFiscal = 0;
                    var fourthFiscal = 0;
                    var fifthFiscal = 0;
                    var sixthFiscal = 0;
                    var seventhFiscal = 0;
                    var eighthFiscal = 0;
                    var currentFiscal = DateTime.Today.Year;
                    if (DateTime.Today.Month > 3)
                    {
                        currentFiscal++;
                    }
                    // All Projects
                    var firstPaidProjectQuery = new QueryExpression("dfa_projectclaim") { ColumnSet = new ColumnSet(true) };
                    firstPaidProjectQuery.Criteria.AddCondition("dfa_caseid", ConditionOperator.Equal, caseRecord.Id);
                    firstPaidProjectQuery.Criteria.AddCondition("dfa_recoveryplanid", ConditionOperator.NotNull);
                    firstPaidProjectQuery.Criteria.AddCondition("dfa_claimpaiddate", ConditionOperator.NotNull);
                    firstPaidProjectQuery.AddOrder("dfa_claimpaiddate", OrderType.Ascending);
                    firstPaidProjectQuery.TopCount = 1; // Only want the first one
                    var firstPaidClaim = Service.RetrieveMultiple(firstPaidProjectQuery);
                    DateTime? firstClaimPaidDate = null;
                    EntityReference firstPaidClaimProjectER = null;
                    if (firstPaidClaim.Entities.Count > 0)
                    {
                        firstPaidClaimProjectER = firstPaidClaim.Entities[0].GetAttributeValue<EntityReference>("dfa_recoveryplanid");
                        firstClaimPaidDate = firstPaidClaim.Entities[0].GetAttributeValue<DateTime>("dfa_claimpaiddate");
                    }

                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Query Project of this case that are approved or closed complete");
                    // Approved Projects
                    var approvedProjectQuery = new QueryExpression("dfa_project") { ColumnSet = new ColumnSet(true) };
                    approvedProjectQuery.Criteria.AddCondition("dfa_caseid", ConditionOperator.Equal, caseRecord.Id);
                    // approvedProjectQuery.Criteria.AddCondition("dfa_projectapproveddate", ConditionOperator.NotNull);
                    approvedProjectQuery.Criteria.AddCondition("statuscode", ConditionOperator.NotEqual, 222710004); // Not Withdrawn
                    approvedProjectQuery.Criteria.AddCondition("statuscode", ConditionOperator.NotEqual, 1); // Not Approval Pending
                    approvedProjectQuery.Criteria.AddCondition("statuscode", ConditionOperator.NotEqual, 222710005); // Not Ineligible
                    approvedProjectQuery.Criteria.AddCondition("statuscode", ConditionOperator.NotEqual, 222710002); // Not Closed Project Withdrawn
                    approvedProjectQuery.Criteria.AddCondition("statuscode", ConditionOperator.NotEqual, 222710003); // Not Closed Project Ineligible
                    approvedProjectQuery.AddOrder("dfa_projectapproveddate", OrderType.Ascending);
                    var approvedProjectResults = Service.RetrieveMultiple(approvedProjectQuery);
                    DateTime? costRecoveryPlanApprovedDateNullable = null;
                    if (approvedProjectResults.Entities.Count > 0)
                    {
                        var hasApprovedCostDeduct1000 = false;
                        // EMBCDFA-324
                        var financialForecast = 0m;
                        // Loop Through all projectes to gather Project Related Statistics
                        foreach (var approvedProject in approvedProjectResults.Entities)
                        {
                            if (costRecoveryPlanApprovedDateNullable == null)
                            {
                                costRecoveryPlanApprovedDateNullable = approvedProjectResults.Entities[0].GetAttributeValue<Nullable<DateTime>>("dfa_projectapproveddate");
                            }

                            var productApprovedPercentage = 80m;
                            var projectPercentageNullable = approvedProject.GetAttributeValue<Nullable<decimal>>("dfa_approvedpercentages");
                            if (projectPercentageNullable != null)
                            {
                                productApprovedPercentage = projectPercentageNullable.Value;
                            }
                            var projectApprovedCostMoney = approvedProject.GetAttributeValue<Money>("dfa_approvedcost");
                            var trueForecaseMoney = approvedProject.GetAttributeValue<Money>("dfa_trueforecast");
                            if (projectApprovedCostMoney != null)
                            {
                                var projectApprovedCost = projectApprovedCostMoney.Value;
                                approvedCost += projectApprovedCost;
                                if (projectApprovedCost > 1000m && !hasApprovedCostDeduct1000)
                                {
                                    hasApprovedCostDeduct1000 = true;
                                    projectApprovedCost = projectApprovedCost - 1000m; // First Deductable
                                }
                                financialForecast += (projectApprovedCost * productApprovedPercentage / 100m);
                            }
                            // EMBCDFA-322
                            /*
                            // "When ‘project’ in dynamics = closed and completed/withdrawn/ineligible,
                            // a new column needs to display 0. New column name will be called “True Forecast”."
                            // These are either Approved, Pending Adjudication or Closed Project Completed
                            */
                            if (trueForecaseMoney != null)
                            {
                                trueForecast += trueForecaseMoney.Value;
                            }
                        }

                        // EMBCDFA-324
                        childRecord["dfa_financialforecast"] = new Money(financialForecast);

                        // EMBCDFA-322
                        // "When ‘project’ in dynamics = closed and completed/withdrawn/ineligible,
                        // a new column needs to display 0. New column name will be called “True Forecast”."
                        childRecord["dfa_trueforecast"] = new Money(trueForecast);

                        // EMBCDFA-322, 320, 324.  Now the business wants to calculate it just like PSR2.
                        // Hence we are using Project Status Report records data instead.


                        // 18 Month Deadline is the 18 Months after the first project Approval Date
                        if (costRecoveryPlanApprovedDateNullable == null)
                        {
                            // So, none of the project has data????? 
                            // Use the First Claim Paid value instead.
                            costRecoveryPlanApprovedDateNullable = firstClaimPaidDate;
                        }

                        // If there is Approved Plan Date, then set these things
                        if (costRecoveryPlanApprovedDateNullable != null)
                        {
                            var costRecoveryPlanApprovedDate = costRecoveryPlanApprovedDateNullable.Value;
                            var deadline18Month = costRecoveryPlanApprovedDate.AddMonths(18);

                            childRecord["dfa_18monthdeadline"] = deadline18Month;
                            childRecord["dfa_crpapprovaldate"] = costRecoveryPlanApprovedDate;

                            // All Transaction should occur from first approved date to 18 months after approved date
                            // So technically, that would stretch over maximum of 3 fiscal years
                            // Obtain first fiscal.
                            firstFiscal = costRecoveryPlanApprovedDate.Year;
                            if (costRecoveryPlanApprovedDate.Month <= 3)
                            {
                                firstFiscal--;
                            }
                            // EMBCDFA-387
                            // EMBC Fiscal Year Is (FY18 = April 1, 2017- March 31, 2018)
                            firstFiscal++;
                            secondFiscal = firstFiscal + 1;
                            thirdFiscal = secondFiscal + 1;
                            fourthFiscal = thirdFiscal + 1;
                            fifthFiscal = fourthFiscal + 1;
                            sixthFiscal = fifthFiscal + 1;
                            seventhFiscal = sixthFiscal + 1;
                            eighthFiscal = seventhFiscal + 1;
                            childRecord["dfa_firstfiscal"] = firstFiscal;
                            
                            childRecord["dfa_secondfiscal"] = secondFiscal;
                            childRecord["dfa_thirdfiscal"] = thirdFiscal;
                            childRecord["dfa_fourthfiscal"] = fourthFiscal;
                            childRecord["dfa_fifthfiscal"] = fifthFiscal;
                            childRecord["dfa_sixthfiscal"] = sixthFiscal;
                            childRecord["dfa_seventhfiscal"] = seventhFiscal;
                            childRecord["dfa_eighthfiscal"] = eighthFiscal;

                            childRecord["dfa_firstfy"] = FormatFiscalYearString(firstFiscal);
                            childRecord["dfa_secondfy"] = FormatFiscalYearString(secondFiscal);
                            childRecord["dfa_thirdfy"] = FormatFiscalYearString(thirdFiscal);
                            childRecord["dfa_fourthfy"] = FormatFiscalYearString(fourthFiscal);
                            childRecord["dfa_fifthfy"] = FormatFiscalYearString(fifthFiscal);
                            childRecord["dfa_sixthfy"] = FormatFiscalYearString(sixthFiscal);
                            childRecord["dfa_seventhfy"] = FormatFiscalYearString(seventhFiscal);
                            childRecord["dfa_eighthfy"] = FormatFiscalYearString(eighthFiscal);
                        }
                        // End Approved Project Loop
                    }
                    if (approvedProjectResults.Entities.Count > 0)
                    {
                        communitiesWithApprovedProject++;
                    }
                    childRecord["dfa_crpapprovedamount100"] = new Money(approvedCost);

                    
                    totalApprovedCRPValuesInEvent += approvedCost;
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Query of all Projects of the case for other data");
                    // Project Count stats
                    var projectQuery = new QueryExpression("dfa_project") { ColumnSet = new ColumnSet(true) };
                    projectQuery.Criteria.AddCondition("dfa_caseid", ConditionOperator.Equal, caseRecord.Id);
                    var allProjects = Service.RetrieveMultiple(projectQuery);
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: All Project under Case - {allProjects.Entities.Count}");
                    // These are so very strange.
                    // Some records have Acutal Cost
                    // Some have Estimated Cost
                    // Approved Cost is not available until project is approved.
                    // When there is Approved Cost, sometimes, the other 2 (Estimated and Actual) are not available
                    var initialCRPValueInCase = 0m;
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Where is the NULL - Start of FOR Loop of All Projects");
                    foreach (var project in allProjects.Entities)
                    {
                        var estimatedCost = project.GetAttributeValue<Money>("dfa_estimatedcost");
                        var actualCost = project.GetAttributeValue<Money>("dfa_actualcostwithtax"); // It's called that but it doesn't have tax
                        var projectApprovedCost = project.GetAttributeValue<Money>("dfa_approvedCost");
                        var initialCRPValue = 0m;

                        if (projectApprovedCost != null)
                        {
                            Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Project Approved Cost is NOT NULL - Where is the NULL - {project.Id}");
                            initialCRPValue = projectApprovedCost.Value;
                        }
                        else if (estimatedCost != null && actualCost != null)
                        {
                            Tracing.Trace($"{CustomCodeHelper.LineNumber()}: NULL ApprovedCost but there is Estimated Cost and Acutal Cost - {project.Id}");
                            // If both contains data, take the greater value one.
                            if (estimatedCost.Value >= actualCost.Value)
                            {
                                initialCRPValue = estimatedCost.Value;
                            }
                            else
                            {
                                initialCRPValue = actualCost.Value;
                            }
                        }
                        else if (estimatedCost != null)
                        {
                            Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Only Estimated Cost is NOT NULL - Where is the NULL - {project.Id}");
                            initialCRPValue = estimatedCost.Value;
                        }
                        else if (actualCost != null)
                        {
                            Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Only Actual Cost is NOT NULL - Where is the NULL - {project.Id}");
                            initialCRPValue = actualCost.Value;
                        }
                        initialCRPValueInCase += initialCRPValue;
                        totalCRPValueInEvent += initialCRPValue;

                        var status = project.GetAttributeValue<OptionSetValue>("statuscode").Value;
                        Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Project Status: {status} Where is the NULL - {project.Id}");
                        switch (status)
                        {
                            case 222710001: // Approved
                                numberOfProjectsApprovedInEvent++;
                                numberOfApprovedProjects++;
                                break;
                            case 222710000: // Pending Adjudication - Approved Projects with Claims validations to be paid.
                                numberOfPendingAdjudicationProjects++;
                                numberOfProjectsPendingAdjudicationInEvent++;
                                break;
                            case 2: // Closed Project Completed
                                numberOfCompletedProjects++;
                                numberOfProjectsClosedCompletedInEvent++;
                                break;
                            case 222710005: // Ineligible
                            case 222710003: // Closed Project Ineligible
                                numberOfIneligibleProjects++;
                                numberOfProjectsClosedIneligibleInEvent++;
                                break;
                            case 1: // Approval Pending
                                numberOfPendingApprovalProjects++;
                                numberOfProjectsPendingApprovalInEvent++;
                                break;
                            case 222710004: // Withdrawn
                            case 222710002: // Closed Project Withdrawn
                                numberOfWithdrawnProjects++;
                                numberOfProjectsWithdrawnInEvent++;
                                break;
                        }
                        // End All Project Loop per Case
                    }

                    childRecord["dfa_crpinitialrequestamount"] = new Money(initialCRPValueInCase);
                    if (allProjects.Entities.Count > 0)
                    {
                        communitiesWithProject++;
                    }

                    // Project Count
                    numberOfCostRecoveryPlanReceived = allProjects.Entities.Count;
                    childRecord["dfa_numberofprojects"] = allProjects.Entities.Count;
                    childRecord["dfa_numberofapprovedprojects"] = numberOfApprovedProjects;
                    childRecord["dfa_numberofprojectscompleted"] = numberOfCompletedProjects;
                    childRecord["dfa_numberofprojectsineligible"] = numberOfIneligibleProjects;
                    childRecord["dfa_numberofprojectspendingapproval"] = numberOfPendingApprovalProjects;
                    childRecord["dfa_numberofprojectswithdrawn"] = numberOfWithdrawnProjects;
                    childRecord["dfa_numberofprojectspendingadjudication"] = numberOfPendingAdjudicationProjects;
                    numberOfApprovedProjectsInEvent += numberOfApprovedProjects;

                    // Claim Data.
                    // PAID Claims
                    Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Query for PAID Claim data");
                    var claimQuery = new QueryExpression("dfa_projectclaim") { ColumnSet = new ColumnSet(true) };
                    claimQuery.Criteria.AddCondition("dfa_caseid", ConditionOperator.Equal, caseRecord.Id);
                    claimQuery.Criteria.AddCondition("dfa_recoveryplanid", ConditionOperator.NotNull);
                    claimQuery.AddOrder("dfa_recoveryplanid", OrderType.Ascending);
                    var claims = Service.RetrieveMultiple(claimQuery);


                    var firstFiscalPaidAmount = 0m;
                    var secondFiscalPaidAmount = 0m;
                    var thirdFiscalPaidAmount = 0m;
                    var fourthFiscalPaidAmount = 0m;
                    var fifthFiscalPaidAmount = 0m;
                    var sixthFiscalPaidAmount = 0m;
                    var seventhFiscalPaidAmount = 0m;
                    var eighthFiscalPaidAmount = 0m;
                    var projectPercentageDefault = 80m;
                    var projectPercentage = projectPercentageDefault;
                    EntityReference projectER = null;
                    // Put it into a list for future processing
                    foreach (var paidClaim in claims.Entities)
                    {
                        var paidClaimProjectER = paidClaim.GetAttributeValue<EntityReference>("dfa_recoveryplanid");
                        if (projectER == null || projectER.Id != paidClaimProjectER.Id)
                        {
                            var paidClaimProject = Service.Retrieve("dfa_project", paidClaimProjectER.Id, new ColumnSet("dfa_approvedpercentages"));
                            var paidClaimProjectApprovedPercentage = paidClaimProject.GetAttributeValue<Nullable<decimal>>("dfa_approvedpercentages");
                            if (paidClaimProjectApprovedPercentage == null)
                            {
                                projectPercentage = projectPercentageDefault;
                            }
                            else
                            {
                                projectPercentage = paidClaimProjectApprovedPercentage.Value;
                            }
                            projectER = paidClaimProjectER;
                        }
                        var claimPaidDate = paidClaim.GetAttributeValue<Nullable<DateTime>>("dfa_claimpaiddate");
                        var claimAmount = paidClaim.GetAttributeValue<Money>("dfa_claimamount");
                        if (claimPaidDate != null && claimAmount != null)
                        {
                            var claimPaid = new ClaimPaid
                            {
                                ClaimAmount = claimAmount.Value,
                                ProjectPercentage = projectPercentage,
                                ClaimPaidDate = claimPaidDate.Value,
                                IsFirstClaim = paidClaim.GetAttributeValue<bool>("dfa_isfirstclaim")
                            };

                            if (claimPaid.FiscalYear == firstFiscal)
                            {
                                firstFiscalPaidAmount += claimPaid.AmountPaid;
                            }
                            else if (claimPaid.FiscalYear == secondFiscal)
                            {
                                secondFiscalPaidAmount += claimPaid.AmountPaid;
                            }
                            else if (claimPaid.FiscalYear == thirdFiscal)
                            {
                                thirdFiscalPaidAmount += claimPaid.AmountPaid;
                            }
                            else if (claimPaid.FiscalYear == fourthFiscal)
                            {
                                fourthFiscalPaidAmount += claimPaid.AmountPaid;
                            }
                            else if (claimPaid.FiscalYear == fifthFiscal)
                            {
                                fifthFiscalPaidAmount += claimPaid.AmountPaid;
                            }
                            else if (claimPaid.FiscalYear == sixthFiscal)
                            {
                                sixthFiscalPaidAmount += claimPaid.AmountPaid;
                            }
                            else if (claimPaid.FiscalYear == seventhFiscal)
                            {
                                seventhFiscalPaidAmount += claimPaid.AmountPaid;
                            }
                            else if (claimPaid.FiscalYear == eighthFiscal)
                            {
                                eighthFiscalPaidAmount += claimPaid.AmountPaid;
                            }
                            else
                            {
                                var debug = claimPaid.ClaimPaidDate;
                            }
                            amountPaid += claimPaid.AmountPaid;
                            numberOfCliamPaidInEvent++;
                            Tracing.Trace($"{CustomCodeHelper.LineNumber()}: claim amount: {claimPaid.ClaimAmount}; project percentage: {claimPaid.ProjectPercentage}; Is First Claim; {claimPaid.IsFirstClaim}; Paid Amount: {claimPaid.AmountPaid}");
                        }

                        // End Claim Loop per Case
                    }
                    childRecord["dfa_haveclaimsbeenreceived"] = claims.Entities.Count > 0;
                    childRecord["dfa_amountpaidin1stfiscal"] = new Money(firstFiscalPaidAmount);
                    childRecord["dfa_amountpaidin2ndfiscal"] = new Money(secondFiscalPaidAmount);
                    childRecord["dfa_amountpaidin3rdfiscal"] = new Money(thirdFiscalPaidAmount);
                    childRecord["dfa_amountpaidin4thfiscal"] = new Money(fourthFiscalPaidAmount);
                    childRecord["dfa_amountpaidin5thfiscal"] = new Money(fifthFiscalPaidAmount);
                    childRecord["dfa_amountpaidin6thfiscal"] = new Money(sixthFiscalPaidAmount);
                    childRecord["dfa_amountpaidin7thfiscal"] = new Money(seventhFiscalPaidAmount);
                    childRecord["dfa_amountpaidin8thfiscal"] = new Money(eighthFiscalPaidAmount);
                    childRecord["dfa_totalamountpaid"] = new Money(amountPaid);
                    totalClaimsPaidValueInEvent += amountPaid;

                    // Calculating Outstanding
                    // This Math will only work if Approved Percentage is consistent throughout all projects under a case.
                    var totalApprovedCostAdjusted = approvedCost - 1000m; // $1000 deductable
                    if (totalApprovedCostAdjusted < 0)
                    {
                        totalApprovedCostAdjusted = 0m;
                    }
                    totalApprovedCostAdjusted = totalApprovedCostAdjusted * projectPercentage / 100m;
                    var amountOutstanding = totalApprovedCostAdjusted - amountPaid;
                    childRecord["dfa_amountoutstanding"] = new Money(amountOutstanding);
                    Service.Create(childRecord);

                    // End Case
                }

                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: Other header data");
                entTarget["dfa_claimsreceivedtotal"] = new Money(0);
                entTarget["dfa_eventid"] = eventER;
                entTarget["dfa_noofcommunitieswithcrphasapprovedprojects"] = communitiesWithApprovedProject;
                entTarget["dfa_numberofapprovedcrp"] = numberOfApprovedProjectsInEvent;
                entTarget["dfa_numberofclaimspaid"] = numberOfCliamPaidInEvent;
                entTarget["dfa_numberofcommunitiesapplied"] = caseResult.Entities.Count;
                entTarget["dfa_numberofcommunitieshavecrp"] = communitiesWithProject;
                entTarget["dfa_numberofcommunitiesinopenstatus"] = caseStillOpen;
                entTarget["dfa_numberofcommunitieswithdrawn"] = caseClosedWithdrawn;
                entTarget["dfa_totalcrpvalue"] = totalCRPValueInEvent;
                entTarget["dfa_totalapprovedcrpvalues"] = totalApprovedCRPValuesInEvent;
                entTarget["dfa_totalclaimspaidvalue"] = totalClaimsPaidValueInEvent;
                entTarget["dfa_numberofprojectsapproved"] = numberOfApprovedProjectsInEvent;
                entTarget["dfa_numberofprojectspendingapproval"] = numberOfProjectsPendingApprovalInEvent;
                entTarget["dfa_numberofprojectspendingadjudication"] = numberOfProjectsPendingAdjudicationInEvent;
                entTarget["dfa_numberofprojectsclosedcompleted"] = numberOfProjectsClosedCompletedInEvent;
                entTarget["dfa_numberofprojectsclosedineligible"] = numberOfProjectsClosedIneligibleInEvent;
                entTarget["dfa_numberofprojectswithdrawn"] = numberOfProjectsWithdrawnInEvent;

                entTarget["dfa_name"] = eventName + " - " + DateTime.Now.ToString("MMM d, yyyy");
                Service.Update(entTarget);
            }
            catch (Exception ex)
            {
                var error = "{\"error\":\"" + ex.Message + "|" + ex.Source + "|" + ex.StackTrace + "\"}";
                Tracing.Trace($"{CustomCodeHelper.LineNumber()}: {error}");
            }

        }

        public string FormatFiscalYearString(int fiscalyear)
        {
            if (fiscalyear.ToString().Length != 4)
            {
                return string.Empty;
            }
            // A Fiscal Year is 2022
            var previousYear = fiscalyear - 1;
            return "FY" + previousYear.ToString().Substring(2, 2) + "/" + fiscalyear.ToString().Substring(2, 2);
        }
    }

    public class ClaimPaid
    {
        public DateTime? ClaimPaidDate { get; set; }
        public decimal ClaimAmount { get; set; }
        public bool IsFirstClaim { get; set; }
        public decimal ProjectPercentage { get; set; } // Please ensure to set this with default 80m
        public decimal AmountPaid
        {
            get
            {
                var paidAmount = this.ClaimAmount;
                if (IsFirstClaim)
                {
                    paidAmount = ClaimAmount - 1000m;
                }
                paidAmount = paidAmount * this.ProjectPercentage / 100m;
                return paidAmount;
            }
        }
        public int FiscalYear
        {
            get
            {
                // Should never happen
                if (ClaimPaidDate == null)
                {
                    return -1;
                }
                var fiscalYear = ClaimPaidDate.Value.Year;
                var month = ClaimPaidDate.Value.Month;
                if (month > 3)
                {
                    fiscalYear++;
                }
                return fiscalYear;
            }

        }
    }
}
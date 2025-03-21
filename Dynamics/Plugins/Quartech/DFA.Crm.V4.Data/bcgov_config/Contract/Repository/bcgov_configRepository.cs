using System.Collections.Generic;
using System.Linq;
using DFA.Crm.V4.Data.bcgov_config.Contract;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DFA.Crm.V4.Data.bcgov_config.Repository
{
    public class bcgov_configRepository : Ibcgov_configRepository
    {
        private readonly IOrganizationService organizationService;

        public bcgov_configRepository(IOrganizationService organizationService)
        {
            this.organizationService = organizationService;
        }

        public Dictionary<string, string> GetAllGroupConfigs(string groupName)
        {
            QueryExpression queryExpression = new QueryExpression("bcgov_config");
            queryExpression.ColumnSet = new ColumnSet("bcgov_key", "bcgov_value", "bcgov_securevalue");

            FilterExpression filterExpression = new FilterExpression(LogicalOperator.And);
            filterExpression.Conditions.Add(new ConditionExpression("bcgov_group", ConditionOperator.Equal, groupName));
            filterExpression.Conditions.Add(new ConditionExpression("statecode", ConditionOperator.Equal, 0));

            queryExpression.Criteria = filterExpression;

            return this.organizationService.RetrieveMultiple(queryExpression).Entities
                .ToDictionary(x => x.GetAttributeValue<string>("bcgov_key"), x => x.Contains("bcgov_securevalue")?
                x.GetAttributeValue<string>("bcgov_securevalue") 
                : x.GetAttributeValue<string>("bcgov_value"));
        }

        public string GetValue(string keyName)
        {
            QueryExpression queryExpression = new QueryExpression("bcgov_config");
            queryExpression.ColumnSet = new ColumnSet("bcgov_key", "bcgov_value");

            FilterExpression filterExpression = new FilterExpression(LogicalOperator.And);
            filterExpression.Conditions.Add(new ConditionExpression("bcgov_key", ConditionOperator.Equal, keyName));
            filterExpression.Conditions.Add(new ConditionExpression("statecode", ConditionOperator.Equal, 0));

            queryExpression.Criteria = new FilterExpression(LogicalOperator.And);

            return this.organizationService.RetrieveMultiple(queryExpression).Entities
                .ToList().Select(x => x.Contains("bcgov_securevalue") ?
                x.GetAttributeValue<string>("bcgov_securevalue")
                : x.GetAttributeValue<string>("bcgov_value")).FirstOrDefault();
        }
    }
}

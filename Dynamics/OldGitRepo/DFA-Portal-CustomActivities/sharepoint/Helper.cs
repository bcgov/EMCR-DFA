using DFA.Custom.Actions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Description;

namespace DFA.CRM.CustomWorkflow
{
    class Helper
    {

        public static EntityReference RetrieveEntitySharePointLocation(IOrganizationService service, string entityName)
        {
            EntityReference result = null;

            QueryExpression exp = new QueryExpression("sharepointdocumentlocation");
            exp.NoLock = true;
            exp.Criteria.AddCondition("regardingobjectid", ConditionOperator.Null);
            exp.Criteria.AddCondition("relativeurl", ConditionOperator.Equal, entityName.ToLowerInvariant());
            exp.Criteria.AddCondition("locationtype", ConditionOperator.Equal, 0); //General
            exp.Criteria.AddCondition("servicetype", ConditionOperator.Equal, 0); //SharePoint

            var coll = service.RetrieveMultiple(exp);

            if (coll != null && coll.Entities != null && coll.Entities.Count > 0)
                result = coll.Entities[0].ToEntityReference();
            else
                throw new InvalidPluginExecutionException(string.Format("Unable to find '{0}' SharePoint Document Location..", entityName));

            return result;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="group"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static List<Entity> GetSystemConfigurations(IOrganizationService service, string group, string key)
        {
            List<Entity> result = new List<Entity>();
            ColumnSet cols = new ColumnSet(
                ConfigEntity.Schema.GROUP,
                ConfigEntity.Schema.KEY,
                ConfigEntity.Schema.VALUE,
                ConfigEntity.Schema.SECURE_VALUE,
                ConfigEntity.Schema.STATE_CODE);

            QueryExpression exp = new QueryExpression(ConfigEntity.ENTITY_NAME)
            {
                NoLock = true
            };

            exp.ColumnSet = cols;

            exp.Criteria.AddCondition(ConfigEntity.Schema.STATE_CODE, ConditionOperator.Equal, 0); //Active
            if (!string.IsNullOrEmpty(group))
                exp.Criteria.AddCondition(ConfigEntity.Schema.GROUP, ConditionOperator.Equal, group);
            if (!string.IsNullOrEmpty(key))
                exp.Criteria.AddCondition(ConfigEntity.Schema.KEY, ConditionOperator.Equal, key);

            var coll = service.RetrieveMultiple(exp);
            if (coll != null && coll.Entities != null && coll.Entities.Count > 0)
                result = coll.Entities.ToList();

            if (result.Count < 1)
                throw new InvalidPluginExecutionException(string.Format(Strings.CONFIGURATION_NOT_FOUND, group, key));

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configurations"></param>
        /// <param name="key"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public static string GetConfigKeyValue(List<Entity> configurations, string key, string group)
        {
            if (string.IsNullOrEmpty(key))
                throw new InvalidPluginExecutionException("Config Key is required..");

            foreach (var configEntity in configurations)
            {
                if (configEntity[ConfigEntity.Schema.GROUP].ToString().Equals(group, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (configEntity[ConfigEntity.Schema.KEY].ToString().Equals(key, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return configEntity[ConfigEntity.Schema.VALUE].ToString();
                    }
                }

            }
            throw new InvalidPluginExecutionException(string.Format("Unable to find configuration with Key '{0}', Group '{1}'..", key, group));
        }


        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="configurations"></param>
        ///// <param name="key"></param>
        ///// <param name="group"></param>
        ///// <returns></returns>
        public static string GetSecureConfigKeyValue(List<Entity> configurations, string key, string group)
        {
            if (string.IsNullOrEmpty(key))
                throw new InvalidPluginExecutionException("Config Key is required..");

            foreach (var configEntity in configurations)
            {
                if (configEntity[ConfigEntity.Schema.GROUP].ToString().Equals(group, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (configEntity[ConfigEntity.Schema.KEY].ToString().Equals(key, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return configEntity[ConfigEntity.Schema.SECURE_VALUE].ToString();
                    }
                }

            }
            throw new InvalidPluginExecutionException(string.Format("Unable to find configuration with Key '{0}', Group '{1}'..", key, group));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static Entity GetSystemUserId(IOrganizationService service, string userName)
        {
            List<Entity> result = new List<Entity>();

            QueryExpression exp = new QueryExpression("systemuser");
            exp.NoLock = true;
            exp.ColumnSet.AllColumns = true;
            exp.Criteria.AddCondition("domainname", ConditionOperator.Equal, userName);


            var coll = service.RetrieveMultiple(exp);
            if (coll != null && coll.Entities != null && coll.Entities.Count > 0)
                result = coll.Entities.ToList();

            if (result.Count < 1)
                throw new InvalidPluginExecutionException($"System User {userName} cannot be found");

            return result[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverUrl"></param>
        /// <param name="orgName"></param>
        /// <param name="deploymentType"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static IOrganizationService InitializeOrganizationService(string serverUrl, string orgName, string deploymentType, string userName, string password)
        {
            Console.WriteLine(string.Format("Initializing organization service to '{0}'", serverUrl));

            var credentials = new ClientCredentials();
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                credentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
            else
            {
                credentials.UserName.UserName = userName;
                credentials.UserName.Password = password;
            }

            Uri crmServerUrl;
            if (deploymentType.Equals("CrmOnline", StringComparison.InvariantCultureIgnoreCase))
                crmServerUrl = new Uri(string.Format("{0}/XRMServices/2011/Organization.svc", serverUrl));
            else
                crmServerUrl = new Uri(string.Format("{0}/{1}/XRMServices/2011/Organization.svc", serverUrl, orgName));

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            using (OrganizationServiceProxy serviceProxy = new OrganizationServiceProxy(crmServerUrl, null, credentials, null))
            {
                serviceProxy.EnableProxyTypes();
                serviceProxy.Timeout = new TimeSpan(1, 0, 0);
                return (IOrganizationService)serviceProxy;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverUrl"></param>
        /// <param name="orgName"></param>
        /// <param name="deploymentType"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="callerId"></param>
        /// <returns></returns>
        public static IOrganizationService InitializeProxyOrganizationService(string serverUrl, string orgName, string deploymentType, string userName, string password, Guid callerId)
        {
            Console.WriteLine(string.Format("Initializing organization service to '{0}'", serverUrl));

            var credentials = new ClientCredentials();
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                credentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
            else
            {
                credentials.UserName.UserName = userName;
                credentials.UserName.Password = password;
            }

            Uri crmServerUrl;
            if (deploymentType.Equals("CrmOnline", StringComparison.InvariantCultureIgnoreCase))
                crmServerUrl = new Uri(string.Format("{0}/XRMServices/2011/Organization.svc", serverUrl));
            else
                crmServerUrl = new Uri(string.Format("{0}/{1}/XRMServices/2011/Organization.svc", serverUrl, orgName));

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            using (OrganizationServiceProxy serviceProxy = new OrganizationServiceProxy(crmServerUrl, null, credentials, null))
            {
                serviceProxy.CallerId = callerId;
                serviceProxy.Timeout = new TimeSpan(4, 0, 0);
                serviceProxy.EnableProxyTypes();
                return (IOrganizationService)serviceProxy;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverUrl"></param>
        /// <param name="orgName"></param>
        /// <param name="deploymentType"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static DiscoveryServiceProxy InitializeDiscoveryService(string serverUrl, string orgName, string deploymentType, string userName, string password)
        {
            Console.WriteLine(string.Format("Initializing discovery service to '{0}'", serverUrl));

            var credentials = new ClientCredentials();
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                credentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
            else
            {
                credentials.UserName.UserName = userName;
                credentials.UserName.Password = password;
            }

            Uri crmServerUrl;
            if (deploymentType.Equals("CrmOnline", StringComparison.InvariantCultureIgnoreCase))
                crmServerUrl = new Uri(string.Format("{0}/XRMServices/2011/Discovery.svc", serverUrl));
            else
                crmServerUrl = new Uri(string.Format("{0}/{1}/XRMServices/2011/Discovery.svc", serverUrl, orgName));

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            using (DiscoveryServiceProxy serviceProxy = new DiscoveryServiceProxy(crmServerUrl, null, credentials, null))
            {
                serviceProxy.Timeout = new TimeSpan(4, 0, 0);
                return serviceProxy;
            }
        }

    }
}

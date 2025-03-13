using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Json;
using Microsoft.Xrm.Sdk.Query;

namespace DFA.CRM.CustomWorkflow
{
    public static class CustomCodeHelper
    {
        public static int LineNumber([System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0)
        {
            return lineNumber;
        }

        public static string ConstructCEEntityRecordURL(string baseURL, string entityLogicalName, Guid recordId)
        {
            var guidInString = recordId.ToString("D"); // ########-####-####-####-############
            var url = $"{baseURL}/main.aspx?etn={entityLogicalName}&pagetype=entityrecord&id=%7B{guidInString}%7D";
            return url;
        }

        public static string RemoveWhitespace(string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray());
        }

        public static EntityCollection GetSystemConfigurationsByGroup(IOrganizationService Service, ITracingService Tracing, string group)
        {
            var query = new QueryExpression("dfa_systemconfig") { ColumnSet = new ColumnSet("dfa_value") };
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0); // Active
            query.Criteria.AddCondition("dfa_group", ConditionOperator.Equal, group);
            query.AddOrder("modifiedon", OrderType.Descending); // Last Modified
            return Service.RetrieveMultiple(query);
        }

        public static string GetSystemConfigurationsValueByGroupAndKey(IOrganizationService Service, ITracingService Tracing, string group, string key)
        {
            var query = new QueryExpression("dfa_systemconfig") { ColumnSet = new ColumnSet("dfa_value") };
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0); // Active
            query.Criteria.AddCondition("dfa_group", ConditionOperator.Equal, group);
            if (!string.IsNullOrEmpty(key))
            {
                query.Criteria.AddCondition("dfa_key", ConditionOperator.Equal, key);
            }
            query.AddOrder("modifiedon", OrderType.Descending); // Last Modified
            query.TopCount = 1;
            var results = Service.RetrieveMultiple(query);
            string result = null;
            if (results.Entities.Count > 0)
            {
                result = results.Entities[0].GetAttributeValue<string>("dfa_value");
            }return result;
        }

        public static T Clone<T>(this T entity) where T: Entity
        {
            var clone = new Entity(entity.LogicalName);
            foreach(KeyValuePair<string, object> attr in entity.Attributes)
            {
                var attrKey = attr.Key.ToLower();
                // Not cloning GUID of record - EntityLogicalName+id or activityid
                if (attrKey == entity.LogicalName.ToLower() + "id" ||
                    attrKey == "activityid")
                {
                    continue;
                }
                // Not cloning metadata of record
                switch (attrKey)
                {
                    case "createdby":
                    case "createdonbehalfby":
                    case "createdon":
                    case "importsequencenumber":
                    case "modifiedby":
                    case "modifiedonbehalfby":
                    case "modifiedon":
                    case "ownerid":
                    case "ownerbusinessunit":
                    case "owingteam":
                    case "owinguser":
                    case "overriddencreatedon":
                    case "timezoneuleversionumber":
                    case "utcconversiontimezonecode":
                    case "versionnumber":
                    case "exchangerate":
                    case "transactioncurrencyid":
                        continue;
                }
                // Clone the Attribute value
                clone[attr.Key] = attr.Value;
            }
            return clone.ToEntity<T>();
        }


        #region Attribute Helper
        public static string GetAttributeValueInString(IOrganizationService Service, ITracingService Tracing, string entityLogicalName, string attributeName, object attributeValue, out string attributeLabel)
        {
            var attributeValueString = string.Empty;
            if (attributeValue == null)
            {
                attributeLabel = string.Empty;
                return null;
            }

            if (attributeName == (entityLogicalName + "id"))
            {
                attributeValueString = ((Guid)attributeValue).ToString("D");
                attributeLabel = entityLogicalName;
            }
            else
            {
                var attributeRequest = new RetrieveAttributeRequest
                {
                    EntityLogicalName = entityLogicalName,
                    LogicalName = attributeName,
                    RetrieveAsIfPublished = false
                };

                // Execute the request
                var attributeResponse = (RetrieveAttributeResponse)Service.Execute(attributeRequest);
                var attributeType = attributeResponse.AttributeMetadata.AttributeType;
                attributeLabel = attributeResponse.AttributeMetadata.DisplayName.UserLocalizedLabel.Label;
                Tracing.Trace($"Custom Code Helper/GetAttributeValueInString {CustomCodeHelper.LineNumber()} Attribute Type:{attributeType} Attribute Name: {attributeName} Attribute Label: {attributeLabel}");
                if (attributeValue == null)
                {
                    return string.Empty;
                }
                switch (attributeType)
                {
                    case AttributeTypeCode.BigInt:
                    case AttributeTypeCode.Integer:
                    case AttributeTypeCode.Boolean:
                    case AttributeTypeCode.Decimal:
                    case AttributeTypeCode.Double:
                    case AttributeTypeCode.Memo:
                    case AttributeTypeCode.String:

                        attributeValueString = attributeValue.ToString();
                        break;
                    case AttributeTypeCode.Customer:
                    case AttributeTypeCode.Lookup:
                    case AttributeTypeCode.Owner:
                        var attributeValueER = (EntityReference)attributeValue;
                        if (attributeValueER != null)
                        {
                            attributeValueString = attributeValueER.Name;
                        }
                        break;
                    case AttributeTypeCode.DateTime:
                        attributeValueString = ((DateTime)attributeValue).ToString("M/d/yyyy hh:mm tt");
                        break;
                    case AttributeTypeCode.Money:
                        attributeValueString = ((Money)attributeValue).Value.ToString();
                        break;
                    case AttributeTypeCode.Picklist:
                    case AttributeTypeCode.State:
                    case AttributeTypeCode.Status:
                        var attributeValueOS = (OptionSetValue)attributeValue;
                        if (attributeValueOS != null)
                        {
                            var attMetadata = (EnumAttributeMetadata)attributeResponse.AttributeMetadata;
                            attributeValueString = attMetadata.OptionSet.Options.
                                Where(x => x.Value == attributeValueOS.Value).FirstOrDefault()?.Label.UserLocalizedLabel.Label;
                            if (string.IsNullOrEmpty(attributeValueString))
                            {
                                attributeValueString = attributeValueOS.Value.ToString();
                            }
                        }
                        break;
                }
            }
            return attributeValueString;
        }

#endregion Attribute Helper

        public static DateTime ConvertToPacificTime(DateTime utc)
        {
            //var isDayLightSaving = utc.IsDaylightSavingTime();
            TimeZoneInfo pacificZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            var pacificTime = TimeZoneInfo.ConvertTimeFromUtc(utc, pacificZone);
            return pacificTime;
        }

        #region Serializer Wrapper
        public static String Serialize<T>(T srcObject)
        {
            if (srcObject == null)
            {
                return string.Empty;
            }
            using (var serializeStream = new System.IO.MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                serializer.WriteObject(serializeStream, srcObject);
                var jsonString = Encoding.Default.GetString(serializeStream.ToArray());
                return jsonString;
            }
        }

        public static T Deserialize<T>(string jsonObject)
        {
            if (string.IsNullOrEmpty(jsonObject))
            {
                return default;
            }
            using (var deserializeStream = new System.IO.MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                var writer = new System.IO.StreamWriter(deserializeStream);
                writer.Write(jsonObject);
                writer.Flush();
                deserializeStream.Position = 0;
                T deserializedObject = (T)serializer.ReadObject(deserializeStream);
                return deserializedObject;
            }
        }
        #endregion

        #region security role helper
        /// <summary>
        /// Security Role of the same role name has different Role Id for each business unit
        /// This method returns all the Role Id which fits the role name specified
        /// </summary>
        /// <param name="context">Workflow Context for data retrieval and Trace</param>
        /// <param name="roleName">Specified Role Name for search</param>
        /// <returns>List of Security Role Id that match the specified security role name</returns>
        public static List<Guid> GetRoleIdsByRoleName(IOrganizationService Service, ITracingService Tracing, string roleName)
        {
            var roleIds = new List<Guid>();
            var retrievedRoles =
                Service.RetrieveMultiple(
                    new FetchExpression(GenerateFetchXmlForRolesByRoleName(roleName)));
            if (retrievedRoles != null && retrievedRoles.Entities != null && retrievedRoles.Entities.Count > 0)
            {
                foreach (var role in retrievedRoles.Entities)
                {
                    var roleId = role.GetAttributeValue<Guid>("roleid");
                    roleIds.Add(roleId);
                }
            }

            return roleIds;
        }

        /// <summary>
        /// Check If User has security role specified by a list of Security Role Ids
        /// </summary>
        /// <param name="context">Workflow context for data retrival</param>
        /// <param name="userId">User Id to test security role against</param>
        /// <param name="roleIds">Security Role Ids for matching of User's assigned security role</param>
        /// <param name="debugString">For Debug / Tracing purpose</param>
        /// <returns>Boolean indicating whether User is assigned of security role(s) specified</returns>
        public static bool IsUserInRole(IOrganizationService Service, ITracingService Tracing, Guid userId, List<Guid> roleIds, out string debugString)
        {
            var debug = new StringBuilder();
            var isInRole = false;
            Tracing.Trace("Retrieve all roles which the user has been assigned.");
            var retrievedUserRoles =
                Service.RetrieveMultiple(new FetchExpression(GenerateFetchXmlForRolesByUserId(userId)));

            Tracing.Trace("Iterate through all user assigned roles to see if there is a match");
            if (retrievedUserRoles != null && retrievedUserRoles.Entities != null &&
                retrievedUserRoles.Entities.Count > 0)
            {
                Tracing.Trace("User has the following roles");
                foreach (var userRoleEntity in retrievedUserRoles.Entities)
                {
                    var userRoleId = userRoleEntity.GetAttributeValue<Guid>("roleid");
                    Tracing.Trace("User Has Role of " + userRoleId);
                    if (roleIds.Contains(userRoleId))
                    {
                        Tracing.Trace("Role match one of those in RoleIds List");
                        isInRole = true;
                        break;
                    }
                }
            }
            debugString = debug.ToString();
            return isInRole;
        }

        /// <summary>
        /// Generate Fetch XML For Roles of specific Role Name
        /// </summary>
        /// <param name="roleName">Specified Role Name</param>
        /// <returns>Fetch XML For Roles of specific Role Name</returns>
        private static string GenerateFetchXmlForRolesByRoleName(string roleName)
        {
            var fetchXml = new StringBuilder();
            fetchXml.Append("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>");
            fetchXml.Append("<entity name='role'>");
            fetchXml.Append("<attribute name='name' />");
            fetchXml.Append("<attribute name='roleid' />");
            fetchXml.Append("<filter type='and'>");
            fetchXml.Append("<condition attribute='name' operator='eq' value='" + roleName + "' />");
            fetchXml.Append("</filter>");
            fetchXml.Append("</entity>");
            fetchXml.Append("</fetch>");
            return fetchXml.ToString();
        }

        /// <summary>
        /// Generate Fetch XML For Roles that are assigned to the User 
        /// </summary>
        /// <param name="userId">User Id to be filtered with</param>
        /// <returns>Fetch XML For Roles that are assigned to the User</returns>
        private static string GenerateFetchXmlForRolesByUserId(Guid userId)
        {
            var fetchXml = new StringBuilder();
            fetchXml.Append("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>");
            fetchXml.Append("<entity name='role'>");
            fetchXml.Append("<attribute name='name' />");
            fetchXml.Append("<attribute name='roleid' />");
            fetchXml.Append(
                "<link-entity name='systemuserroles' from='roleid' to='roleid' visible='false' intersect='true'>");
            fetchXml.Append("<link-entity name='systemuser' from='systemuserid' to='systemuserid'>");
            fetchXml.Append("<filter type='and'>");
            fetchXml.Append("<condition attribute='systemuserid' operator='eq' value='" + userId + "' />");
            fetchXml.Append("</filter>");
            fetchXml.Append("</link-entity>");
            fetchXml.Append("</link-entity>");
            fetchXml.Append("</entity>");
            fetchXml.Append("</fetch>");
            return fetchXml.ToString();
        }
        #endregion security role helper

    }
}
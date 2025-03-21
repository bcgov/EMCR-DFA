using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using Microsoft.Xrm.Sdk;

namespace DFA.Crm.V4.Common.Extensions
{
    public static class EntityExtensions
    {
        public static T GetAttributeValueOrDefault<T>(this Entity entity, string attribute, T defaultValue = default(T))
        {
            if (entity.Contains(attribute))
            {
                return entity.GetAttributeValue<T>(attribute);
            }
            return defaultValue;
        }


        public static OptionSetValue MapToOptionsetValueOrNull(this int? value)
        {
            return value.HasValue && value.Value !=0 ? new OptionSetValue(value.Value) : null;
        }

        public static Money MapToMoneyOrNull(this decimal? value)
        {
            return value.HasValue && value.Value>0 ? new Money(value.Value) : null;
        }

        public static DateTime? MapToDateTimeOrNull(this DateTime? value)
        {
            return value.HasValue && value.Value > DateTime.MinValue ? value : null;
        }

        public static EntityReference MapToEntityReferenceOrNull(this string id, string entityName)
        {
            return !string.IsNullOrEmpty(id) ? new EntityReference(entityName, new Guid(id)) : null;
        }

        public static OptionSetValue ToOptionsetValue(this int value)
        {
            return new OptionSetValue(value);
        }
    }
}

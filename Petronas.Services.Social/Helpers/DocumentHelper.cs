using System;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using Petronas.Services.Social.Constants;

namespace Petronas.Services.Social.Helpers
{
    public static class DocumentHelper
    {
        public static void SetEnvironment(object obj, string environment)
        {
            Type t = obj.GetType();
            foreach (var propInfo in t.GetProperties())
            {
                if (propInfo.Name == CommonProperties.Environment)
                propInfo.SetValue(obj, environment, null);
            }
        }

        public static void SetCreationProperties(Document document, string createBy)
        {
            document.SetPropertyValue(LowerFirstChar(CommonProperties.CreatedOn), DateTime.UtcNow);
            document.SetPropertyValue(LowerFirstChar(CommonProperties.CreatedBy), createBy);
        }

        public static void SetModificationProperties(Document document, string updateBy, object item)
        {
            var excludedProperties = new string[]
            {
                CommonProperties.Id,
                CommonProperties.IsDeleted,
                CommonProperties.CreatedOn,
                CommonProperties.CreatedBy,
                CommonProperties.ModifiedOn,
                CommonProperties.ModifiedBy,
                CommonProperties.DeletedOn,
                CommonProperties.DeletedBy
            };
            var properties = item.GetType().GetProperties().Where(x => !excludedProperties.Contains(x.Name));

            foreach (var property in properties)
            {
                if(property.GetCustomAttribute<JsonPropertyAttribute>()==null)
                {
                        document.SetPropertyValue(property.Name, property.GetValue(item));
                }
                else{
                    document.SetPropertyValue(property.GetCustomAttribute<JsonPropertyAttribute>().PropertyName, property.GetValue(item));
                }
                
                
            }

            document.SetPropertyValue(LowerFirstChar(CommonProperties.ModifiedOn), DateTime.UtcNow);
            document.SetPropertyValue(LowerFirstChar(CommonProperties.ModifiedBy), updateBy);
        }

        public static void SetDeletionProperties(Document document, string deleteBy)
        {
            document.SetPropertyValue(LowerFirstChar(CommonProperties.IsDeleted), true);
            document.SetPropertyValue(LowerFirstChar(CommonProperties.DeletedOn), DateTime.UtcNow);
            document.SetPropertyValue(LowerFirstChar(CommonProperties.DeletedBy), deleteBy);
        }

        public static string GetPartitionKeyByEnvironment(string environment, string partitionKey)
        {
            return environment + "-" + partitionKey;
        }

        private static string LowerFirstChar(string str)
        {
            return char.ToLower(str[0]) + str.Substring(1);
        }
    }
}

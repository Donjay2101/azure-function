using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Petronas.Services.Social.Constants;

namespace Petronas.Services.Social.Helpers
{
    public static class GeneralHelper
    {
        public static T GetPagedListContract<T>(HttpRequest req) where T : class, new()
        {
            var dict = req.Query.ToDictionary(k => k.Key, k => k.Value);
            var contractJsonString = JsonConvert.SerializeObject(dict).Replace("[", string.Empty).Replace("]", string.Empty);
            var contract = JsonConvert.DeserializeObject<T>(contractJsonString);
            var continuationToken = req.Headers[RequestHeaders.ContinuationToken];
            PropertyInfo propertyInfo = contract.GetType().GetProperty(RequestHeaders.ContinuationToken);

            if (propertyInfo != null)
            {
                propertyInfo.SetValue(
                    contract,
                    !StringValues.IsNullOrEmpty(continuationToken) && continuationToken.Count > 0
                        ? DecodeString(continuationToken.FirstOrDefault())
                        : null,
                    null);
            }

            return contract;
        }

        public static string GenerateNewId()
        {
            return Guid.NewGuid().ToString().Replace("-", string.Empty).ToUpper();
        }

        public static string EncodeString(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            var encodedBytes = System.Text.Encoding.UTF8.GetBytes(text);
            return System.Convert.ToBase64String(encodedBytes);
        }

        public static string DecodeString(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            var encodedBytes = System.Convert.FromBase64String(text);
            return System.Text.Encoding.UTF8.GetString(encodedBytes);
        }

        public static bool IsHeaderExist(string header, HttpRequest request)
        {
            var value = request.Headers[header];
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }
            return true;
            //request

        }

        public static string GetHeaderValue(string header, HttpRequest request)
        {
            return request.Headers[header];
        }
    }
}
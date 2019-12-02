using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Documents;
using Petronas.Services.Social.ViewModels;

namespace Petronas.Services.Social.Helpers
{
    public static class HttpHelper
    {
        public static ResponseObjectModel ReturnObject(object data)
        {
            return new ResponseObjectModel(HttpStatusCode.OK, data);
        }

        public static ResponseObjectModel ThrowError(HttpStatusCode? statusCode, KeyValuePair<string, string> error)
        {
            var errorContent = new ErrorResponseModel
            {
                ErrorCode = error.Key,
                Message = error.Value
            };

            return new ResponseObjectModel(statusCode, errorContent);
        }

        public static ResponseObjectModel ThrowError(HttpStatusCode statusCode, string errorMessage)
        {
            var errorContent = new ErrorResponseModel
            {
                ErrorCode = statusCode.ToString(),
                Message = errorMessage
            };

            return new ResponseObjectModel(statusCode, errorContent);
        }

        public static ResponseObjectModel ThrowError(DocumentClientException ex)
        {
            int code = (int)(ex.StatusCode ?? HttpStatusCode.InternalServerError);
            var errorMessage = ex.Message.Split('\n')[0].Replace("Message:", "");
            var errorContent = new KeyValuePair<string, string>(code.ToString(), errorMessage);
            return new ResponseObjectModel(ex.StatusCode, errorContent);
        }
    }
}
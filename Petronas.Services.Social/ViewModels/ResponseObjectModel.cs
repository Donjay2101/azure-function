using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Petronas.Services.Social.ViewModels
{
    public class ResponseObjectModel : ObjectResult
    {
        public ResponseObjectModel(HttpStatusCode? statusCode, object data) : base(data)
        {
            StatusCode = statusCode != null ? (int)statusCode : (int)HttpStatusCode.InternalServerError;
            Value = data;
        }
    }
}

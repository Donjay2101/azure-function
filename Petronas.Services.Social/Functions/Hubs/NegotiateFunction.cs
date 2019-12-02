using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Petronas.Services.Social.Constants;
using Petronas.Services.Social.Services;

namespace Petronas.Services.Social.Functions.Hubs
{
    public static class NegotiateFunction
    {
        [FunctionName("NegotiateFunction")]
        public static IActionResult Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                RequestMethods.Get,
                Route = "{hubName}/negotiate")]HttpRequest req,
            string hubName,
            ILogger log)
        {
            var hubNames = typeof(HubNames).GetFields().Select(x => x.GetValue(null)).Cast<string>();

            // Validate hub name
            if (!hubNames.Contains(hubName))
                return new BadRequestObjectResult(ErrorMessages.HubNameNotValid);

            var signalRService = new SignalRService(Environment.GetEnvironmentVariable(AppSettings.AzureSignalRConnectionString));
            return signalRService != null
                ? (ActionResult)new OkObjectResult(new
                    {
                        serviceUrl = signalRService.GetClientHubUrl(hubName),
                        accessToken = signalRService.GenerateAccessToken(hubName)
                    })
                : new NotFoundObjectResult(ErrorMessages.HubLoadFailed);
        }
    }
}

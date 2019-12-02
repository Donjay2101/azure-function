using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Petronas.Services.Social.Filters
{
    public class ErrorHandlerAttribute : FunctionExceptionFilterAttribute
    {
        public override Task OnExceptionAsync(
            FunctionExceptionContext exceptionContext,
            CancellationToken cancellationToken)
        {
            // Log the exception into the function exception context
            if (exceptionContext.Exception is DocumentClientException)
            {
                var ex = (DocumentClientException)exceptionContext.Exception;
                exceptionContext.Logger.LogError(ex.Message);
            }

            return Task.CompletedTask;
        }
    }
}
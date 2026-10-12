using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Adex.Mvc.Hosting
{
    /// <summary>
    /// Answers JSON requests with a problem response; for pages it only sets the status code and
    /// lets the exception handler middleware render the error page.
    /// </summary>
    public sealed class MvcExceptionHandler(IProblemDetailsService problemDetailsService)
        : IExceptionHandler
    {
        private const int ClientClosedRequest = 499;

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken
        )
        {
            if (
                exception is OperationCanceledException
                && httpContext.RequestAborted.IsCancellationRequested
            )
            {
                httpContext.Response.StatusCode = ClientClosedRequest;
                return true;
            }

            var (status, message) = exception.Map();
            httpContext.Response.StatusCode = status;
            if (
                !httpContext.Request.Headers.Accept.ToString().Contains(
                    "application/json",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return false;
            }

            return await problemDetailsService.TryWriteAsync(
                new ProblemDetailsContext
                {
                    HttpContext = httpContext,
                    Exception = exception,
                    ProblemDetails = new ProblemDetails { Status = status, Detail = message },
                }
            );
        }
    }
}

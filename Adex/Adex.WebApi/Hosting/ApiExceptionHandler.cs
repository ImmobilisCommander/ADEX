using Adex.WebApi.Csv;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Adex.WebApi.Hosting
{
    /// <summary>Turns any exception escaping the pipeline into an RFC 9457 problem response.</summary>
    public sealed class ApiExceptionHandler(IProblemDetailsService problemDetailsService)
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

            var (status, title, detail) = exception switch
            {
                CsvFileUnavailableException => (
                    StatusCodes.Status404NotFound,
                    "Fichier CSV indisponible",
                    exception.Message
                ),
                CsvFilterNotAllowedException => (
                    StatusCodes.Status400BadRequest,
                    "Filtre invalide",
                    exception.Message
                ),
                _ => (
                    StatusCodes.Status500InternalServerError,
                    "Erreur interne",
                    "Une erreur inattendue s’est produite."
                ),
            };

            httpContext.Response.StatusCode = status;
            return await problemDetailsService.TryWriteAsync(
                new ProblemDetailsContext
                {
                    HttpContext = httpContext,
                    Exception = exception,
                    ProblemDetails = new ProblemDetails
                    {
                        Status = status,
                        Title = title,
                        Detail = detail,
                    },
                }
            );
        }
    }
}

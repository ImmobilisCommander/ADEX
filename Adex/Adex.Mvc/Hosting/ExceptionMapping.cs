using Microsoft.AspNetCore.Http;

using System;
using System.Net;
using System.Net.Http;

namespace Adex.Mvc.Hosting
{
    public static class ExceptionMapping
    {
        public static (int StatusCode, string Message) Map(Exception exception)
        {
            if (exception is not HttpRequestException http)
            {
                return (
                    StatusCodes.Status500InternalServerError,
                    "Une erreur inattendue s’est produite."
                );
            }

            return http.StatusCode == HttpStatusCode.ServiceUnavailable
                ? (
                    StatusCodes.Status503ServiceUnavailable,
                    "Un import est en cours, les données sont temporairement indisponibles."
                )
                : (
                    StatusCodes.Status502BadGateway,
                    "Les données ne sont pas accessibles. Vérifiez que l’API est disponible."
                );
        }
    }
}

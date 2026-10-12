using System;
using System.Net;
using System.Net.Http;

namespace Adex.WebApi.Client
{
    public class AdexApiException(
        string message,
        HttpStatusCode? statusCode = null,
        Exception innerException = null
    ) : HttpRequestException(message, innerException, statusCode) { }
}

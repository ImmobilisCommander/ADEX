using System;
using System.IO;

namespace Adex.WebApi.Csv
{
    public sealed class CsvFileUnavailableException(string path, Exception innerException)
        : Exception(
            $"Le fichier « {Path.GetFileName(path)} » est introuvable ou illisible.",
            innerException
        );
}

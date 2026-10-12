using System;

namespace Adex.Application.Csv
{
    public sealed class CsvFilterNotAllowedException(string column)
        : Exception($"La colonne « {column} » ne peut pas être filtrée.");
}

/// <summary>
/// Factory class for creating exporter instances based on the specified format.
/// </summary>
public static class ExporterFactory
{
    /// <summary>
    /// Returns an exporter instance for the given format. Optionally applies a cell value converter for CSV export.
    /// </summary>
    /// <typeparam name="T">The type of data to export.</typeparam>
    /// <param name="format">The export format (csv, json, pdf).</param>
    /// <param name="cellValueConverter">Optional cell value converter for CSV export.</param>
    /// <returns>An exporter instance for the specified format.</returns>
    public static IExporter<T> GetExporter<T>(string format, Func<string, string, string>? cellValueConverter = null)
    {
        switch (format.ToLower())
        {
            case "csv":
                var csv = new CsvExporter<T>();
                if (cellValueConverter != null)
                    csv.CellValueConverter = cellValueConverter;
                return csv;
            case "json":
                return new JsonExporter<T>();
            case "pdf":
                return new PdfExporter<T>();
            default:
                throw new NotSupportedException($"Format {format} is not supported.");
        }
    }
}
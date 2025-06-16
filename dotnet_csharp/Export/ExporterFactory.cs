public static class ExporterFactory
{
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
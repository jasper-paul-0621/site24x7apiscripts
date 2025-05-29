public static class ExporterFactory
{
    public static IExporter<T> GetExporter<T>(string format)
    {
        return format.ToLower() switch
        {
            "csv" => new CsvExporter<T>(),
            "json" => new JsonExporter<T>(),
            // Add PDF and others as needed
            _ => throw new NotSupportedException($"Format {format} is not supported.")
        };
    }
}
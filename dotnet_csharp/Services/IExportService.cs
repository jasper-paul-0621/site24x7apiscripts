namespace Site24x7CustomReports.Services
{
    public interface IExportService
    {
        Task ExportAsync(string monitorsJson, Func<string, string, string> cellValueConverter);
    }
}

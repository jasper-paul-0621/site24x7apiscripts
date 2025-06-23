using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Site24x7Integration;

namespace Site24x7CustomReports.Services
{
    public class ExportService : IExportService
    {
        private readonly string _exportFormat;

        public ExportService(string exportFormat = "csv")
        {
            // Set the default export format if needed
            // This could be used to initialize the exporter factory or set default values
            _exportFormat = exportFormat.ToLower();

            // Initialize any required services or dependencies here
        }

        public async Task ExportAsync(string monitorsJson, Func<string, string, string> cellValueConverter)
        {
            if (string.IsNullOrEmpty(monitorsJson))
                return;
            var doc = JsonDocument.Parse(monitorsJson);
            var root = doc.RootElement;

            var monitors = new List<JsonElement>();
            foreach (var item in root.EnumerateArray())
                monitors.Add(item);
            var exporter = ExporterFactory.GetExporter<JsonElement>(_exportFormat, cellValueConverter);
            string filePath = _exportFormat switch
            {
                "csv" => "monitors.csv",
                "json" => "monitors.json",
                "pdf" => "monitors.pdf",
                _ => throw new NotSupportedException($"Unknown export format: {_exportFormat}")
            };
            await Task.Run(() => exporter.Export(monitors, filePath));
        }
    }
}

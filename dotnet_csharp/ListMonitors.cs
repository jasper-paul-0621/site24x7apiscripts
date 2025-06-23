using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using Site24x7CustomReports.Services;

namespace Site24x7Integration
{
    /// <summary>
    /// Provides methods to list Site24x7 monitors and export them in various formats.
    /// </summary>
    public class ListMonitors
    {
        private readonly IApiService _apiService;
        private readonly IExportService _exportService;


        public ListMonitors(IExportService exportService)
        {
            _apiService = new ApiService();
            _exportService = exportService;
        }

        /// <summary>
        /// Orchestrates authentication, API call, and export using injected services.
        /// </summary>
        public async Task RunAsync(Func<string, string, string> cellValueConverter)
        {
            var monitorsJson = await _apiService.GetMonitorsJsonAsync();
            if (string.IsNullOrEmpty(monitorsJson))
            {
                Console.WriteLine("No monitor data returned from API.");
                return;
            }
            await _exportService.ExportAsync(monitorsJson, cellValueConverter);
            Console.WriteLine("Export completed.");
        }
    }
}
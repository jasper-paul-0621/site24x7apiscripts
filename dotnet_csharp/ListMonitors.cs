using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using CSharpFNF;

namespace Site24x7Integration
{
    public class ListMonitors
    {
        public static async Task RunAsync()
        {
            var clientId = TokenConstants.CLIENT_ID;
            var clientSecret = TokenConstants.CLIENT_SECRET;
            var refreshToken = TokenConstants.REFRESH_TOKEN;
            var accountUrl = "https://accounts.zoho.com";
            var exportFormat = "csv"; // Default export format

            var auth = new AuthZoho(clientId, clientSecret, refreshToken, accountUrl);
            var api = new Site24x7ApiClient(auth);
            var monitors = await api.GetMonitorListAsync();

            switch (exportFormat)
            {
                case "csv":
                    ExportUtils.ExportToCsv(monitors, "monitors.csv");
                    Console.WriteLine("Exported to monitors.csv");
                    break;
                case "json":
                    ExportUtils.ExportToJson(monitors, "monitors.json");
                    Console.WriteLine("Exported to monitors.json");
                    break;
                case "pdf":
                    ExportUtils.ExportToPdf(monitors, "monitors.pdf");
                    Console.WriteLine("Exported to monitors.pdf");
                    break;
                default:
                    Console.WriteLine($"Unknown export format: {exportFormat}");
                    break;
            }
        }
    }
}
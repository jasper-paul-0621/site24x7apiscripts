using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using Site24x7CustomReports;

namespace Site24x7Integration
{
    /// <summary>
    /// Provides methods to list Site24x7 monitors and export them in various formats.
    /// </summary>
    public class ListMonitors
    {
        /// <summary>
        /// Reads authentication credentials, retrieves the monitor list from Site24x7, and exports the data.
        /// </summary>
        /// <remarks>
        /// - Reads credentials from 'site24x7_auth.txt' using <see cref="AuthZoho.TryReadAuthFile"/>.
        /// - Retrieves monitor data using <see cref="Site24x7ApiClient.GetMonitorListAsync"/>.
        /// - Exports data to CSV, JSON, or PDF using <see cref="ExportUtils"/>.
        /// </remarks>
        public static async Task RunAsync()
        {
            var authFilePath = "site24x7_auth.txt";
            if (!AuthZoho.TryReadAuthFile(authFilePath, out var clientId, out var clientSecret, out var refreshToken))
            {
                Console.WriteLine($"Missing CLIENT_ID, CLIENT_SECRET, or REFRESH_TOKEN in auth file '{authFilePath}'.");
                return;
            }
            var accountUrl = "https://accounts.zoho.com";
            var exportFormat = "csv"; // Default export format

            var auth = new AuthZoho(clientId, clientSecret, refreshToken, accountUrl);
            var api = new Site24x7ApiClient(auth);
            var monitors = await api.GetMonitorListAsync();
            Console.WriteLine($"Found {monitors.Count} monitors.");
            if (monitors.Count == 0)
            {
                Console.WriteLine("No monitors found.");
                return;
            }   

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
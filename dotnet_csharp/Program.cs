using Site24x7CustomReports;
using Site24x7CustomReports.Services;
using Site24x7Integration;
using System.Text.Json;

/// <summary>
/// Entry point for the Site24x7CustomReports application. Handles user interaction and starts the monitor export process.
/// </summary>
internal class Program
{
    /// <summary>
    /// Main entry point. Prompts the user for export format, sets up cell value conversion, and runs the monitor export.
    /// </summary>
    /// <param name="args">Command-line arguments. Optionally, the first argument can specify the export format (csv, json, pdf).</param>
    private static async Task Main(string[] args)
    {
        // Determine export format from command-line or prompt the user
        string exportFormat = args.Length > 0 ? args[0].ToLower() : AskExportFormat();

        // Set up a cell value converter for custom value mapping (e.g., state field)
        var cellValueConverter = GetCellValueConverter();

        // Instantiate services
        IExportService exportService = new ExportService(exportFormat);

        // Create and run the monitor export process using services
        var monitors = new ListMonitors(exportService);
        await monitors.RunAsync(cellValueConverter);

        Console.WriteLine("Export completed. Press Enter to exit.");
        Console.ReadLine();
        Environment.Exit(0);
    }

    /// <summary>
    /// Prompts the user to enter the export format.
    /// </summary>
    /// <returns>The chosen export format (csv, json, or pdf).</returns>
    private static string AskExportFormat()
    {
        Console.WriteLine("Enter export format (csv, json, pdf): ");
        var input = Console.ReadLine()?.Trim().ToLower();
        if (input == "csv" || input == "json" || input == "pdf")
            return input;
        Console.WriteLine("Invalid format. Defaulting to csv.");
        return "csv";
    }

    /// <summary>
    /// Returns a function that converts cell values for export, e.g., mapping state codes to text.
    /// </summary>
    /// <returns>A function that takes a property name and value, and returns the converted value.</returns>
    private static Func<string, string, string> GetCellValueConverter()
    {
        return (property, value) =>
        {
            if (property == "state")
            {
                return value switch
                {
                    "0" => "Active",
                    "3" => "Deleted",
                    "5" => "Suspended",
                    _ => value
                };
            }
            // Add more property conversions as needed
            return value;
        };
    }
}
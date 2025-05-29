using Site24x7Integration;
using System.Threading;

public class CsvExporter<T> : IExporter<T>
{
    public void Export(List<T> data, string filePath)
    {
        // Optimized CSV export logic (as previously fixed)
        ExportUtils.ExportToCsv(data, "monitors.csv");
        Console.WriteLine("Exported to monitors.csv");
    }
}
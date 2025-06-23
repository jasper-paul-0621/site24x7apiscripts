using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Site24x7Integration;

public class JsonExporter<T> : IExporter<T>
{
    public void Export(List<T> data, string filePath)
    {
        if (typeof(T).FullName == "Site24x7Integration.Monitor")
        {
            var json = System.Text.Json.JsonSerializer.Serialize(
                data as List<Site24x7Integration.Monitor>,
                Site24x7Integration.MonitorJsonContext.Default.ListMonitor);
            File.WriteAllText(filePath, json);
        }
        else
        {
            var json = System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }
    }
}
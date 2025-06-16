using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Site24x7Integration;
using System.Threading;
using System.Text.Json;

public class CsvExporter<T> : IExporter<T>
{
    public Func<string, string, string> CellValueConverter { get; set; } = (property, value) => value;

    public void Export(List<T> data, string filePath)
    {
        if (data == null || data.Count == 0)
            return;

        string[] propertyNames;
        if (typeof(T) == typeof(System.Text.Json.JsonElement))
        {
            var firstObj = data[0];
            if (firstObj is null)
                throw new ArgumentException("Data contains null element.");
            var firstElement = firstObj is System.Text.Json.JsonElement je ? je : default;
            if (firstElement.ValueKind != System.Text.Json.JsonValueKind.Object)
                throw new ArgumentException("First element is not a valid JSON object.");
            propertyNames = firstElement.EnumerateObject().Select(p => p.Name).ToArray();
        }
        else
        {
            propertyNames = typeof(T).GetProperties()
                .Where(p => p.GetIndexParameters().Length == 0)
                .Select(p => p.Name)
                .ToArray();
        }

        using var writer = new StreamWriter(filePath);
        writer.WriteLine(string.Join(",", propertyNames));

        foreach (var item in data)
        {
            List<string> values = new();
            JsonElement jsonElement = typeof(T) == typeof(JsonElement)
                ? (item is JsonElement je ? je : default)
                : ToJsonElement(item);

            foreach (var propName in propertyNames)
            {
                string val = string.Empty;
                if (jsonElement.TryGetProperty(propName, out var propValue))
                {
                    val = propValue.ToString();
                }
                // Apply custom conversion
                val = CellValueConverter(propName, val);
                if (val.Contains('"') || val.Contains(','))
                    val = $"\"{val.Replace("\"", "\"\"")}\"";
                values.Add(val);
            }
            writer.WriteLine(string.Join(",", values));
        }
    }

    private System.Text.Json.JsonElement ToJsonElement<TObj>(TObj obj)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(obj);
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        return doc.RootElement.Clone();
    }
}
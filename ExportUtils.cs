using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using OfficeOpenXml;  // Install-Package EPPlus
using iTextSharp.text;  // Install-Package iTextSharp
using iTextSharp.text.pdf;

namespace Site24x7Integration
{
    public static class ExportUtils
    {
        public static void ExportToJson<T>(List<T> data, string filePath)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        public static void ExportToPdf<T>(List<T> data, string filePath)
        {
            var properties = typeof(T).GetProperties()
                .Where(p => p.GetIndexParameters().Length == 0)
                .ToArray();

            var doc = new Document();
            PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
            doc.Open();

            // Create table with columns equal to number of properties
            PdfPTable table = new PdfPTable(properties.Length);

            // Add header row
            foreach (var prop in properties)
            {
                table.AddCell(new Phrase(prop.Name));
            }

            // Add data rows
            foreach (var item in data)
            {
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(item)?.ToString() ?? string.Empty;
                    table.AddCell(new Phrase(value));
                }
            }

            doc.Add(table);
            doc.Close();
        }

        public static void ExportToCsv<T>(List<T> data, string filePath)
        {
            using var writer = new StreamWriter(filePath);
            if (data == null || data.Count == 0)
                return;

            // If T is JsonElement, get property names from the first element
            string[] propertyNames;
            if (typeof(T) == typeof(JsonElement))
            {
                var firstObj = data[0];
                if (firstObj is null)
                    throw new ArgumentException("Data contains null element.");
                var firstElement = firstObj is JsonElement je ? je : default;
                if (firstElement.ValueKind != JsonValueKind.Object)
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

            // Write header
            writer.WriteLine(string.Join(",", propertyNames));

            // Write data rows
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
                    // Escape double quotes and commas
                    if (val.Contains('"') || val.Contains(','))
                        val = $"\"{val.Replace("\"", "\"\"")}\"";
                    values.Add(val);
                }
                writer.WriteLine(string.Join(",", values));
            }
        }

        public static JsonElement ToJsonElement<T>(T obj)
        {
            var json = JsonSerializer.Serialize(obj);
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.Clone(); // Clone to avoid disposal issues
        }
    }
}
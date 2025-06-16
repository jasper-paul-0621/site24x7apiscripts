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
            throw new NotImplementedException("Use JsonExporter instead.");
        }

        public static void ExportToPdf<T>(List<T> data, string filePath)
        {
            throw new NotImplementedException("Use PdfExporter instead.");
        }

        public static void ExportToCsv<T>(List<T> data, string filePath)
        {
            throw new NotImplementedException("Use CsvExporter instead.");
        }

        public static JsonElement ToJsonElement<T>(T obj)
        {
            var json = JsonSerializer.Serialize(obj);
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.Clone(); // Clone to avoid disposal issues
        }
    }
}
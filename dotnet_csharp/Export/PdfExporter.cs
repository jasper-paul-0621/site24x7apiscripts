using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;

public class PdfExporter<T> : IExporter<T>
{
    public void Export(List<T> data, string filePath)
    {
        var properties = typeof(T).GetProperties()
            .Where(p => p.GetIndexParameters().Length == 0)
            .ToArray();

        var doc = new Document();
        PdfWriter.GetInstance(doc, new FileStream(filePath, FileMode.Create));
        doc.Open();

        PdfPTable table = new PdfPTable(properties.Length);

        foreach (var prop in properties)
        {
            table.AddCell(new Phrase(prop.Name));
        }

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
}

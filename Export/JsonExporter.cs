using System.Text.Json;

public class JsonExporter<T> : IExporter<T>
{
    public void Export(List<T> data, string filePath)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
    }
}
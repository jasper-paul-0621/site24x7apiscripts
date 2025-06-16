public interface IExporter<T>
{
    void Export(List<T> data, string filePath);
    
}
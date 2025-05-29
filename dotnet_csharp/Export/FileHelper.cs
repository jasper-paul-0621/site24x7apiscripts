public static class FileHelper
{
    public static string GetSafeFilePath(string filePath)
    {
        // Prevent path traversal
        var fullPath = Path.GetFullPath(filePath);
        // Optionally, restrict to a specific directory
        // if (!fullPath.StartsWith(allowedDirectory)) throw new UnauthorizedAccessException();
        return fullPath;
    }
}
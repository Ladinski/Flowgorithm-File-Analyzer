using System.IO.Compression;

namespace FlowgorithmAnalyzer.Core.Services;

public interface IFileHandlingService
{
    Task<List<(string fileName, byte[] content)>> ExtractZipContentsAsync(byte[] zipContent);
    string DetectFileType(string fileName);
    byte[]? ReadFileAsBytes(string filePath);
}

public class FileHandlingService : IFileHandlingService
{
    public async Task<List<(string fileName, byte[] content)>> ExtractZipContentsAsync(byte[] zipContent)
    {
        var files = new List<(string, byte[])>();
        
        try
        {
            using var ms = new MemoryStream(zipContent);
            using var archive = new ZipArchive(ms, ZipArchiveMode.Read);

            foreach (var entry in archive.Entries.Where(e => IsFlowgorithmFile(e.Name)))
            {
                using var entryStream = entry.Open();
                using var memoryStream = new MemoryStream();
                await entryStream.CopyToAsync(memoryStream);
                files.Add((entry.Name, memoryStream.ToArray()));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting ZIP: {ex.Message}");
        }

        return files;
    }

    public string DetectFileType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".fprg" or ".frpg" or ".fgl" => "flowgorithm",
            ".zip" => "zip",
            _ => "unknown"
        };
    }

    private static bool IsFlowgorithmFile(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        return extension.Equals(".fprg", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".frpg", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".fgl", StringComparison.OrdinalIgnoreCase);
    }

    public byte[]? ReadFileAsBytes(string filePath)
    {
        try
        {
            return File.ReadAllBytes(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading file: {ex.Message}");
            return null;
        }
    }
}

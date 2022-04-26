using Honeydew.Logging;

namespace Honeydew.Extractors.Load;

public class ActualFilePathProvider
{
    private readonly ILogger _logger;

    public ActualFilePathProvider(ILogger logger)
    {
        _logger = logger;
    }

    public string GetActualFilePath(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return "";
        }

        try
        {
            if (File.Exists(filePath))
            {
                return GetProperFilePathCapitalization(filePath);
            }

            if (Directory.Exists(filePath))
            {
                return GetProperDirectoryCapitalization(new DirectoryInfo(filePath));
            }

            return filePath;
        }
        catch
        {
            return filePath;
        }
    }

    private string GetProperDirectoryCapitalization(DirectoryInfo dirInfo)
    {
        var parentDirInfo = dirInfo.Parent;
        if (null == parentDirInfo)
        {
            return dirInfo.Root.FullName;
        }

        return Path.Combine(GetProperDirectoryCapitalization(parentDirInfo),
            parentDirInfo.GetDirectories(dirInfo.Name)[0].Name);
    }

    private string GetProperFilePathCapitalization(string filename)
    {
        var fileInfo = new FileInfo(filename);
        var dirInfo = fileInfo.Directory;

        if (dirInfo is null)
        {
            _logger.Log($"Could not get directory info for file: {filename}");
            return filename;
        }

        return Path.Combine(GetProperDirectoryCapitalization(dirInfo), dirInfo.GetFiles(fileInfo.Name)[0].Name);
    }
}

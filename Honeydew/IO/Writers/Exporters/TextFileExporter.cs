using Honeydew.Logging;

namespace Honeydew.IO.Writers.Exporters;

public class TextFileExporter
{
    private readonly ILogger _logger;

    public TextFileExporter(ILogger logger)
    {
        _logger = logger;
    }

    public void Export(string filePath, string content)
    {
        var directoryName = Path.GetDirectoryName(filePath);
        if (string.IsNullOrEmpty(directoryName))
        {
            _logger.Log($"Could not create directory for Text File Export: {filePath}", LogLevels.Error);
            return;
        }

        Directory.CreateDirectory(directoryName);

        File.WriteAllText(filePath, content);
    }
}

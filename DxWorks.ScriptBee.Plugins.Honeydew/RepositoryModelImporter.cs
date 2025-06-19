using Honeydew.Models;
using DxWorks.ScriptBee.Plugins.Honeydew.Converters;
using Newtonsoft.Json;

namespace DxWorks.ScriptBee.Plugins.Honeydew;

public class RepositoryModelImporter
{
    private readonly ProjectModelConverter _projectModelConverter;

    public RepositoryModelImporter(ProjectModelConverter projectModelConverter)
    {
        _projectModelConverter = projectModelConverter;
    }

    public async Task<RepositoryModel?> Import(Stream inputStream, CancellationToken cancellationToken)
    {
        // Ensure the inputStream supports reading and has data
        if (inputStream == null || !inputStream.CanRead)
            throw new ArgumentException("Invalid input stream.", nameof(inputStream));

        // Use the stream reader and JSON text reader to handle large files efficiently
        using var streamReader = new StreamReader(inputStream);
        await using var jsonTextReader = new JsonTextReader(streamReader);

        var serializer = new JsonSerializer
        {
            // Optional: Configure JSON serializer settings for large file performance
            CheckAdditionalContent = true,
            MaxDepth = null // Handle nested structures without predefined limits
        };

        serializer.Converters.Add(_projectModelConverter);

        // Perform deserialization in an asynchronous-friendly task
        var result = await Task.Run(() =>
        {
            try
            {
                return serializer.Deserialize<RepositoryModel>(jsonTextReader);
            }
            catch (JsonReaderException ex)
            {
                // Handle JSON format issues gracefully
                throw new InvalidOperationException("Failed to parse JSON data.", ex);
            }
        }, cancellationToken);

        return result;
    }
}

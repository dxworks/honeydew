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
        using var streamReader = new StreamReader(inputStream);
        var serializer = new JsonSerializer();

        serializer.Converters.Add(_projectModelConverter);

        using var jsonTextReader = new JsonTextReader(streamReader);
        var result = await Task.Run(() => serializer.Deserialize<RepositoryModel>(jsonTextReader), cancellationToken);

        return result;
    }
}

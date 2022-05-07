using Honeydew.Extractors.Converters;
using Honeydew.Models;
using Newtonsoft.Json;

namespace Honeydew.Extractors.Importers;

public class RepositoryModelImporter
{
    private readonly ProjectModelConverter _projectModelConverter;

    public RepositoryModelImporter(ProjectModelConverter projectModelConverter)
    {
        _projectModelConverter = projectModelConverter;
    }

    public async Task<RepositoryModel?> Import(string filePath, CancellationToken cancellationToken)
    {
        using var streamReader = File.OpenText(filePath);
        var serializer = new JsonSerializer();

        serializer.Converters.Add(_projectModelConverter);

        using var jsonTextReader = new JsonTextReader(streamReader);
        var result = await Task.Run(() => serializer.Deserialize<RepositoryModel>(jsonTextReader), cancellationToken);

        return result;
    }
}

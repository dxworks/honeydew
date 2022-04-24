using Newtonsoft.Json;

namespace Honeydew.Models.Importers;

public class JsonModelImporter<TModel>
{
    private readonly IConverterList _converterList;

    public JsonModelImporter(IConverterList converterList)
    {
        _converterList = converterList;
    }

    public async Task<TModel?> Import(string filePath, CancellationToken cancellationToken)
    {
        using var streamReader = File.OpenText(filePath);
        var serializer = new JsonSerializer();
        foreach (var converter in _converterList.GetConverters())
        {
            serializer.Converters.Add(converter);
        }

        using var jsonTextReader = new JsonTextReader(streamReader);
        var result = await Task.Run(() => serializer.Deserialize<TModel>(jsonTextReader), cancellationToken);

        return result;
    }
}

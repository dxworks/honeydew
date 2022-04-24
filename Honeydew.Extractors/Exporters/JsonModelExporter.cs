using Newtonsoft.Json;

namespace Honeydew.Extractors.Exporters;

public class JsonModelExporter
{
    public void Export(string filePath, object model)
    {
        var jsonSerializer = JsonSerializer.Create();
        using var writer = new StreamWriter(filePath);
        using var jsonWriter = new JsonTextWriter(writer);

        jsonSerializer.Serialize(jsonWriter, model);
    }
}

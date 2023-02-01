using Honeydew.ModelAdapters.V2._1._0.CSharp;
using Newtonsoft.Json;

namespace Honeydew.ModelAdapters.V2._1._0.Importers;

public class RawCSharpFileRepositoryLoader_V210
{
    public RepositoryModel_V210? Load(string path)
    {
        Console.WriteLine($"Opening File at {path}");

        try
        {
            var repositoryModel = Import(path);

            if (repositoryModel is null)
            {
                return null;
            }

            Console.WriteLine("Model Loaded");

            return repositoryModel;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static RepositoryModel_V210? Import(string filePath)
    {
        using var streamReader = File.OpenText(filePath);

        var serializer = new JsonSerializer();
        var jsonConverters = new List<JsonConverter>
        {
            new ClassTypeConverter()
        };
        foreach (var converter in jsonConverters)
        {
            serializer.Converters.Add(converter);
        }

        using var jsonTextReader = new JsonTextReader(streamReader);

        return serializer.Deserialize<RepositoryModel_V210>(jsonTextReader);
    }
}

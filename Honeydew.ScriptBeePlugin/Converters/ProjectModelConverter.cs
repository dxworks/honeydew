using Honeydew.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Honeydew.ScriptBeePlugin.Converters;

public class ProjectModelConverter : JsonConverter
{
    private readonly IDictionary<string, IConverterList> _converterListDictionary;
    private readonly IConverterList _defaultConverterList;

    public ProjectModelConverter(IDictionary<string, IConverterList> converterListDictionary,
        IConverterList defaultConverterList)
    {
        _converterListDictionary = converterListDictionary;
        _defaultConverterList = defaultConverterList;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        var obj = JObject.Load(reader);

        var languageTypeToken = obj.GetValue("Language");

        var projectLanguage = languageTypeToken?.ToString() ?? "";
        var converterList = string.IsNullOrEmpty(projectLanguage)
            ? _defaultConverterList
            : _converterListDictionary[projectLanguage];

        var projectSerializer = new JsonSerializer();

        foreach (var converter in converterList.GetConverters())
        {
            projectSerializer.Converters.Add(converter);
        }

        var projectJson = JsonConvert.SerializeObject(obj);

        using var jsonTextReader = new JsonTextReader(new StringReader(projectJson));
        var projectModel = projectSerializer.Deserialize<ProjectModel>(jsonTextReader)!;

        return projectModel;
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(ProjectModel);
    }
}

using Newtonsoft.Json;

namespace Honeydew.ModelRepresentations;

public class CyclomaticComplexityPerFileRepresentation
{
    [JsonProperty("file")] public FileWrapper File { get; set; } = new();

    public void AddConcern(Concern concern)
    {
        File.Concerns.Add(concern);
    }
}

public class FileWrapper
{
    [JsonProperty("concerns")] public List<Concern> Concerns { get; set; } = new();
}

public record Concern(
    [JsonProperty("entity")] string Entity,
    [JsonProperty("tag")] string Tag,
    [JsonProperty("strength")] string Strength);

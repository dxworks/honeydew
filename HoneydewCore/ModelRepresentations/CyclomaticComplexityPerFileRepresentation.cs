using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HoneydewCore.ModelRepresentations
{
    public class CyclomaticComplexityPerFileRepresentation
    {
        [JsonPropertyName("file")] public FileWrapper File { get; set; } = new();

        public void AddConcern(Concern concern)
        {
            File.Concerns.Add(concern);
        }
    }

    public class FileWrapper
    {
        [JsonPropertyName("concerns")] public List<Concern> Concerns { get; set; } = new();
    }

    public class Concern
    {
        [JsonPropertyName("entity")] public string Entity { get; set; }
        [JsonPropertyName("tag")] public string Tag { get; set; }
        [JsonPropertyName("strength")] public string Strength { get; set; }
    }
}

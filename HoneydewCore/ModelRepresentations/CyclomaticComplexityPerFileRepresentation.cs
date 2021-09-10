using System.Collections.Generic;
using Newtonsoft.Json;

namespace HoneydewCore.ModelRepresentations
{
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

    public class Concern
    {
        [JsonProperty("entity")] public string Entity { get; set; }
        [JsonProperty("tag")] public string Tag { get; set; }
        [JsonProperty("strength")] public string Strength { get; set; }
    }
}

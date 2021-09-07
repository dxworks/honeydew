using System.IO;
using HoneydewCore.ModelRepresentations;
using HoneydewModels.Exporters;
using Newtonsoft.Json;

namespace HoneydewCore.IO.Writers.Exporters
{
    public class
        JsonCyclomaticComplexityPerFileRepresentationExporter : IModelExporter<
            CyclomaticComplexityPerFileRepresentation>
    {
        public void Export(string filePath, CyclomaticComplexityPerFileRepresentation model)
        {
            var jsonSerializer = JsonSerializer.Create();
            jsonSerializer.Serialize(new StreamWriter(filePath), model);
        }
    }
}

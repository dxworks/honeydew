using System.Text.Json;
using HoneydewCore.ModelRepresentations;
using HoneydewModels.Exporters;

namespace HoneydewCore.IO.Writers.Exporters
{
    public class
        JsonCyclomaticComplexityPerFileRepresentationExporter : IModelExporter<
            CyclomaticComplexityPerFileRepresentation>
    {
        public string Export(CyclomaticComplexityPerFileRepresentation model)
        {
            return JsonSerializer.Serialize(model);
        }
    }
}

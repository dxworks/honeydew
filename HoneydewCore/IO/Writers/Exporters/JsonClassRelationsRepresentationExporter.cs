using System.Text.Json;
using HoneydewCore.ModelRepresentations;
using HoneydewModels.Exporters;

namespace HoneydewCore.IO.Writers.Exporters
{
    public class JsonClassRelationsRepresentationExporter : IModelExporter<ClassRelationsRepresentation>
    {
        public string Export(ClassRelationsRepresentation model)
        {
            return JsonSerializer.Serialize(model);
        }
    }
}

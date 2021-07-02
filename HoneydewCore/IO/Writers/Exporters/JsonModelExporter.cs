using System.Text.Json;
using HoneydewCore.Models;
using HoneydewCore.Models.Representations;

namespace HoneydewCore.IO.Writers.Exporters
{
    public class JsonModelExporter : ISolutionModelExporter, IClassRelationsRepresentationExporter
    {
        public string Export(SolutionModel model)
        {
            return JsonSerializer.Serialize(model);
        }

        public string Export(ClassRelationsRepresentation classRelationsRepresentation)
        {
            return JsonSerializer.Serialize(classRelationsRepresentation);
        }
    }
}
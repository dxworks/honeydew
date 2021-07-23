using System.Text.Json;
using HoneydewCore.Models.Representations;
using HoneydewModels;
using HoneydewModels.Representations;

namespace HoneydewCore.IO.Writers.Exporters
{
    public class JsonModelExporter : ISolutionModelExporter, IClassRelationsRepresentationExporter,
        IRepositoryModelExporter
    {
        public string Export(SolutionModel model)
        {
            return JsonSerializer.Serialize(model);
        }

        public string Export(ClassRelationsRepresentation classRelationsRepresentation)
        {
            return JsonSerializer.Serialize(classRelationsRepresentation);
        }

        public string Export(RepositoryModel model)
        {
            return JsonSerializer.Serialize(model);
        }
    }
}

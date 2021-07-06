using System.Text.Json;
using HoneydewCore.IO.Writers.JSON;
using HoneydewCore.Models;
using HoneydewCore.Models.Representations;
using HoneydewCore.Models.Representations.ReferenceModel;

namespace HoneydewCore.IO.Writers.Exporters
{
    public class JsonModelExporter : ISolutionModelExporter, IClassRelationsRepresentationExporter,
        IReferenceSolutionModelExporter, IRepositoryModelExporter
    {
        public string Export(SolutionModel model)
        {
            return JsonSerializer.Serialize(model);
        }

        public string Export(ClassRelationsRepresentation classRelationsRepresentation)
        {
            return JsonSerializer.Serialize(classRelationsRepresentation);
        }

        public string Export(ReferenceSolutionModel model)
        {
            return JsonReferenceSolutionModelSerializer.Serialize(model);
        }

        public string Export(RepositoryModel model)
        {
            return JsonSerializer.Serialize(model);
        }
    }
}
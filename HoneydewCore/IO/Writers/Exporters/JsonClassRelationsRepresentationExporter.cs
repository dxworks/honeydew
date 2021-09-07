using HoneydewCore.ModelRepresentations;
using HoneydewModels.Exporters;

namespace HoneydewCore.IO.Writers.Exporters
{
    public class JsonClassRelationsRepresentationExporter : IModelExporter<ClassRelationsRepresentation>
    {
        public void Export(string filePath, ClassRelationsRepresentation model)
        {
            new JsonStreamWriter().Write(filePath, model);
        }
    }
}

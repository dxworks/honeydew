using HoneydewCore.ModelRepresentations;
using HoneydewModels.Exporters;

namespace HoneydewCore.IO.Writers.Exporters
{
    public class
        JsonCyclomaticComplexityPerFileRepresentationExporter : IModelExporter<
            CyclomaticComplexityPerFileRepresentation>
    {
        public void Export(string filePath, CyclomaticComplexityPerFileRepresentation model)
        {
            new JsonStreamWriter().Write(filePath, model);
        }
    }
}

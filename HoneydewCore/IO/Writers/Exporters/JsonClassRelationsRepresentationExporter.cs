using System.IO;
using HoneydewCore.ModelRepresentations;
using HoneydewModels.Exporters;
using Newtonsoft.Json;

namespace HoneydewCore.IO.Writers.Exporters
{
    public class JsonClassRelationsRepresentationExporter : IModelExporter<ClassRelationsRepresentation>
    {
        public void Export(string filePath, ClassRelationsRepresentation model)
        {
            var jsonSerializer = JsonSerializer.Create();
            jsonSerializer.Serialize(new StreamWriter(filePath), model);
        }
    }
}

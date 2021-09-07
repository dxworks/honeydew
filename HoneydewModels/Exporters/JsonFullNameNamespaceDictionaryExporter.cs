using System.Collections.Generic;
using System.IO;
using HoneydewModels.CSharp;
using Newtonsoft.Json;

namespace HoneydewModels.Exporters
{
    public class JsonFullNameNamespaceDictionaryExporter : IModelExporter<IDictionary<string, NamespaceTree>>
    {
        public void Export(string filePath, IDictionary<string, NamespaceTree> model)
        {
            var jsonSerializer = JsonSerializer.Create();
            jsonSerializer.Serialize(new StreamWriter(filePath), model);
        }
    }
}

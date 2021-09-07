using System.Collections.Generic;
using HoneydewModels.CSharp;

namespace HoneydewModels.Exporters
{
    public class JsonFullNameNamespaceDictionaryExporter : IModelExporter<IDictionary<string, NamespaceTree>>
    {
        public void Export(string filePath, IDictionary<string, NamespaceTree> model)
        {
            new JsonStreamWriter().Write(filePath, model);
        }
    }
}

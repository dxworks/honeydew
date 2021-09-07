using HoneydewModels.CSharp;

namespace HoneydewModels.Exporters
{
    public class JsonRepositoryModelExporter : IModelExporter<RepositoryModel>
    {
        public void Export(string filePath, RepositoryModel model)
        {
            new JsonStreamWriter().Write(filePath, model);
        }
    }
}

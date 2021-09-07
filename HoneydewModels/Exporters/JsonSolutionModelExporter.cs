using HoneydewModels.CSharp;

namespace HoneydewModels.Exporters
{
    public class JsonSolutionModelExporter : IModelExporter<SolutionModel>
    {
        public void Export(string filePath, SolutionModel model)
        {
            new JsonStreamWriter().Write(filePath, model);
        }
    }
}

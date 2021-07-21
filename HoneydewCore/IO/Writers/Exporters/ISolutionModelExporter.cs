using HoneydewModels;

namespace HoneydewCore.IO.Writers.Exporters
{
    public interface ISolutionModelExporter : IExporter
    {
        string Export(SolutionModel model);
    }
}

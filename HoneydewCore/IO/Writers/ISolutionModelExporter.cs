using HoneydewCore.Models;

namespace HoneydewCore.IO.Writers
{
    public interface ISolutionModelExporter : IExporter
    {
        string Export(SolutionModel model);
    }
}
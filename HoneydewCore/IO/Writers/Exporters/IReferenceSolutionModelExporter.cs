using HoneydewCore.Models.Representations.ReferenceModel;

namespace HoneydewCore.IO.Writers.Exporters
{
    public interface IReferenceSolutionModelExporter : IExporter
    {
        string Export(ReferenceSolutionModel model);
    }
}
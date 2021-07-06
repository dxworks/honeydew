using HoneydewCore.Models;

namespace HoneydewCore.IO.Writers.Exporters
{
    public interface IRepositoryModelExporter : IExporter
    {
        string Export(RepositoryModel model);
    }
}
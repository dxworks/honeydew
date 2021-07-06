using System.Collections.Generic;
using HoneydewCore.IO.Writers.Exporters;

namespace HoneydewCore.Models
{
    public class RepositoryModel : IExportable
    {
        public IList<SolutionModel> Solutions { get; set; } = new List<SolutionModel>();

        public string Export(IExporter exporter)
        {
            if (exporter is IRepositoryModelExporter modelExporter)
            {
                return modelExporter.Export(this);
            }

            return string.Empty;
        }
    }
}
using System.Text.Json;
using HoneydewCore.Models;

namespace HoneydewCore.IO.Writers
{
    public class RawModelExporter : ISolutionModelExporter
    {
        public string Export(SolutionModel model)
        {
            return JsonSerializer.Serialize(model);
        }
    }
}
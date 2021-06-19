using HoneydewCore.Models.Representations;

namespace HoneydewCore.IO.Writers.Exporters
{
    public interface IFileRelationsRepresentationExporter : IExporter
    {
        string Export(FileRelationsRepresentation fileRelationsRepresentation);
    }
}
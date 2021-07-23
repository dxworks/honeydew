using HoneydewCore.Models.Representations;

namespace HoneydewCore.IO.Writers.Exporters
{
    public interface IClassRelationsRepresentationExporter : IExporter
    {
        string Export(ClassRelationsRepresentation classRelationsRepresentation);
    }
}

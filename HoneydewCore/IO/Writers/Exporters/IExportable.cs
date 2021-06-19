namespace HoneydewCore.IO.Writers.Exporters
{
    public interface IExportable
    {
        string Export(IExporter exporter);
    }
}
namespace HoneydewCore.IO.Writers
{
    public interface IExportable
    {
        string Export(IExporter exporter);
    }
}
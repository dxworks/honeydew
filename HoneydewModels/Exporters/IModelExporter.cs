namespace HoneydewModels.Exporters
{
    public interface IModelExporter<in TModel>
    {
        void Export(string filePath, TModel model);
    }
}

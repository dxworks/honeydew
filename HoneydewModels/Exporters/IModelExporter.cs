namespace HoneydewModels.Exporters
{
    public interface IModelExporter<in TModel>
    {
        string Export(TModel model);
    }
}

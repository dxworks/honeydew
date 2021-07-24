namespace HoneydewModels.Exporters
{
    public interface IModelLoader<out TModel>
    {
        TModel Load(string fileContent);
    }
}

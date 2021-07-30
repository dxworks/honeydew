namespace HoneydewModels.Importers
{
    public interface IModelImporter<out TModel>
    {
        TModel Import(string fileContent);
    }
}

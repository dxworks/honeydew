using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.PostExtraction.Model;

public class ClassTypeSetterModelVisitor : ModelSetterVisitor<FileModel, EntityModel>
{
    public ClassTypeSetterModelVisitor(IEnumerable<IModelVisitor<EntityModel>> visitors) : base(visitors)
    {
    }

    public override void Visit(FileModel modelType)
    {
        foreach (var entityModel in modelType.Entities)
        {
            foreach (var typeVisitor in GetContainedVisitors())
            {
                if (typeVisitor is IModelVisitor<EntityModel> modelVisitor)
                {
                    modelVisitor.Visit(entityModel);
                }
            }
        }
    }
}

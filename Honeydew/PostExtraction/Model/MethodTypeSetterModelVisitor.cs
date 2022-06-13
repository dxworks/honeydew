using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.PostExtraction.Model;

public class MethodTypeSetterModelVisitor :
    ModelSetterVisitor<EntityModel, MethodModel>
{
    public MethodTypeSetterModelVisitor(IEnumerable<IModelVisitor<MethodModel>> visitors) : base(visitors)
    {
    }

    public override void Visit(EntityModel modelType)
    {
        IList<MethodModel> methodModels = new List<MethodModel>();
        switch (modelType)
        {
            case ClassModel classModel:
                methodModels = classModel.Methods;
                break;
            case InterfaceModel interfaceModel:
                methodModels = interfaceModel.Methods;
                break;
        }

        foreach (var methodType in methodModels)
        {
            foreach (var typeVisitor in GetContainedVisitors())
            {
                if (typeVisitor is IModelVisitor<MethodModel> modelVisitor)
                {
                    modelVisitor.Visit(methodType);
                }
            }
        }
    }
}

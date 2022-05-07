using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;

namespace Honeydew.ScriptBeePlugin.Loaders;

public class VisualBasicRepositoryModelConversionStrategy : IRepositoryModelConversionStrategy
{
    public bool IsDelegateModel(IClassType classType)
    {
        return classType is VisualBasicDelegateModel;
    }

    public bool IsEnumModel(IClassType classType)
    {
        return classType is VisualBasicEnumModel;
    }

    public string GetEnumType(IClassType classType)
    {
        return classType is VisualBasicEnumModel visualBasicEnumModel ? visualBasicEnumModel.Type : "";
    }

    public bool IsInterfaceModel(IClassType classType)
    {
        return classType is VisualBasicClassModel { ClassType: "interface" };
    }

    public bool IsClassModel(IClassType classType)
    {
        return classType is VisualBasicClassModel classModel && classModel.ClassType != "interface";
    }

    public bool IsPrimitive(string type)
    {
        return VisualBasicConstants.IsPrimitive(type);
    }

    public bool IsEvent(IFieldType fieldType)
    {
        return false;
    }

    public bool IsEvent(IPropertyType fieldType)
    {
        return false;
    }

    public IParameterType CreateParameterType(string type)
    {
        var indexOfNullable = type.IndexOf('?');
        return new VisualBasicParameterModel
        {
            Type = new VisualBasicEntityTypeModel
            {
                Name = indexOfNullable >= 0 ? type[..indexOfNullable] : type
            },
            IsNullable = indexOfNullable >= 0
        };
    }

    public IEntityType CreateEntityTypeModel(string type)
    {
        return VisualBasicFullTypeNameBuilder.CreateEntityTypeModel(type);
    }
}

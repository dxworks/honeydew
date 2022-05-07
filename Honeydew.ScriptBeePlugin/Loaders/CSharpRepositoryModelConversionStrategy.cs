using Honeydew.Models.CSharp;
using Honeydew.Models.Types;

namespace Honeydew.ScriptBeePlugin.Loaders;

public class CSharpRepositoryModelConversionStrategy : IRepositoryModelConversionStrategy
{
    public bool IsDelegateModel(IClassType classType)
    {
        return classType is CSharpDelegateModel;
    }

    public bool IsEnumModel(IClassType classType)
    {
        return classType is CSharpEnumModel;
    }

    public string GetEnumType(IClassType classType)
    {
        return classType is CSharpEnumModel cSharpEnumModel ? cSharpEnumModel.Type : "";
    }

    public bool IsInterfaceModel(IClassType classType)
    {
        return classType is CSharpClassModel { ClassType: "interface" };
    }

    public bool IsClassModel(IClassType classType)
    {
        return classType is CSharpClassModel classModel && classModel.ClassType != "interface";
    }

    public bool IsPrimitive(string type)
    {
        return CSharpConstants.IsPrimitive(type);
    }

    public bool IsEvent(IFieldType fieldType)
    {
        return fieldType is CSharpFieldModel { IsEvent: true };
    }

    public bool IsEvent(IPropertyType fieldType)
    {
        return fieldType is CSharpPropertyModel { IsEvent: true };
    }

    public IParameterType CreateParameterType(string type)
    {
        var indexOfNullable = type.IndexOf('?');
        return new CSharpParameterModel
        {
            Type = new CSharpEntityTypeModel
            {
                Name = indexOfNullable >= 0 ? type[..indexOfNullable] : type
            },
            IsNullable = indexOfNullable >= 0
        };
    }

    public IEntityType CreateEntityTypeModel(string type)
    {
        return CSharpFullTypeNameBuilder.CreateEntityTypeModel(type);
    }
}

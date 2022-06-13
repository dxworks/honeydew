using Honeydew.Models.Types;

namespace Honeydew.ScriptBeePlugin.Loaders;

public interface IRepositoryModelConversionStrategy
{
    bool IsDelegateModel(IClassType classType);

    bool IsClassModel(IClassType classType);

    bool IsInterfaceModel(IClassType classType);

    bool IsEnumModel(IClassType classType);

    string GetEnumType(IClassType classType);

    bool IsPrimitive(string type);

    bool IsEvent(IFieldType fieldType);

    bool IsEvent(IPropertyType fieldType);

    IParameterType CreateParameterType(string type);

    IEntityType CreateEntityTypeModel(string type);

    int GetGenericParameterCount(IClassType classType);

    int MethodCount(IClassType classType);
    
    int ConstructorCount(IClassType classType);
}

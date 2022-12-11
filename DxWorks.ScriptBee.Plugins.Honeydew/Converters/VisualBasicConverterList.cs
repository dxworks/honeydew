using Honeydew.Models;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Newtonsoft.Json;

namespace DxWorks.ScriptBee.Plugins.Honeydew.Converters;

public class VisualBasicConverterList : IConverterList
{
    public IEnumerable<JsonConverter> GetConverters()
    {
        return new List<JsonConverter>
        {
            new ModelJsonConverter<IAccessorMethodType, VisualBasicAccessorMethodModel>(),
            new ModelJsonConverter<IAttributeType, VisualBasicAttributeModel>(),
            new ModelJsonConverter<IBaseType, VisualBasicBaseTypeModel>(),
            new ClassTypeConverter(new VisualBasicClassTypeConverter()),
            new ModelJsonConverter<ICompilationUnitType, VisualBasicCompilationUnitModel>(),
            new ModelJsonConverter<IConstructorType, VisualBasicConstructorModel>(),
            new ModelJsonConverter<IDestructorType, VisualBasicDestructorModel>(),
            new ModelJsonConverter<IEntityType, VisualBasicEntityTypeModel>(),
            new ModelJsonConverter<IEnumType, VisualBasicEnumModel>(),
            new ModelJsonConverter<IEnumLabelType, VisualBasicEnumLabelType>(),
            new ModelJsonConverter<IFieldType, VisualBasicFieldModel>(),
            new ModelJsonConverter<IGenericParameterType, VisualBasicGenericParameterModel>(),
            new ModelJsonConverter<ILocalVariableType, VisualBasicLocalVariableModel>(),
            new ModelJsonConverter<IMethodCallType, VisualBasicMethodCallModel>(),
            new ModelJsonConverter<IMethodType, VisualBasicMethodModel>(),
            new ModelJsonConverter<IMethodTypeWithLocalFunctions, VisualBasicMethodModel>(),
            new ModelJsonConverter<IParameterType, VisualBasicParameterModel>(),
            new ModelJsonConverter<IPropertyType, VisualBasicPropertyModel>(),
            new ModelJsonConverter<IReturnValueType, VisualBasicReturnValueModel>(),
            new ModelJsonConverter<IImportType, VisualBasicImportModel>(),
        };
    }
}

using Honeydew.Extractors.Converters;
using Honeydew.Models;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Newtonsoft.Json;

namespace Honeydew.Extractors.CSharp.Converters;

public class CSharpConverterList : IConverterList
{
    public IEnumerable<JsonConverter> GetConverters()
    {
        return new List<JsonConverter>
        {
            new ModelJsonConverter<IAccessorMethodType, CSharpAccessorMethodModel>(),
            new ModelJsonConverter<IAttributeType, CSharpAttributeModel>(),
            new ModelJsonConverter<IBaseType, CSharpBaseTypeModel>(),
            new ClassTypeConverter(new CSharpClassTypeConverter()),
            new ModelJsonConverter<ICompilationUnitType, CSharpCompilationUnitModel>(),
            new ModelJsonConverter<IConstructorType, CSharpConstructorModel>(),
            new ModelJsonConverter<IDestructorType, CSharpDestructorModel>(),
            new ModelJsonConverter<IEntityType, CSharpEntityTypeModel>(),
            new ModelJsonConverter<IEnumType, CSharpEnumModel>(),
            new ModelJsonConverter<IEnumLabelType, CSharpEnumLabelType>(),
            new ModelJsonConverter<IFieldType, CSharpFieldModel>(),
            new ModelJsonConverter<IGenericParameterType, CSharpGenericParameterModel>(),
            new ModelJsonConverter<ILocalVariableType, CSharpLocalVariableModel>(),
            new ModelJsonConverter<IMethodCallType, CSharpMethodCallModel>(),
            new ModelJsonConverter<IMethodType, CSharpMethodModel>(),
            new ModelJsonConverter<IMethodTypeWithLocalFunctions, CSharpMethodModel>(),
            new ModelJsonConverter<IParameterType, CSharpParameterModel>(),
            new ModelJsonConverter<IPropertyType, CSharpPropertyModel>(),
            new ModelJsonConverter<IReturnValueType, CSharpReturnValueModel>(),
            new ModelJsonConverter<IImportType, CSharpUsingModel>(),
        };
    }
}

using System.Collections.Generic;
using HoneydewModels.Converters;
using HoneydewModels.Converters.JSON;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Newtonsoft.Json;

namespace HoneydewModels
{
    public interface IConverterList
    {
        IEnumerable<JsonConverter> GetConverters();
    }

    public class ConverterList : IConverterList
    {
        public IEnumerable<JsonConverter> GetConverters()
        {
            return new List<JsonConverter>
            {
                new ModelJsonConverter<IRepositoryModel, RepositoryModel>(),
                new ModelJsonConverter<IEntityType, EntityTypeModel>(),
                new ModelJsonConverter<IParameterType, ParameterModel>(),
                new ModelJsonConverter<IBaseType, BaseTypeModel>(),
                new ModelJsonConverter<IConstructorType, ConstructorModel>(),
                new ModelJsonConverter<IMethodType, MethodModel>(),
                new ModelJsonConverter<IFieldType, FieldModel>(),
                new ModelJsonConverter<IPropertyType, PropertyModel>(),
                new ModelJsonConverter<IReturnValueType, ReturnValueModel>(),
                new ModelJsonConverter<IImportType, UsingModel>(),
                new ModelJsonConverter<IMethodSignatureType, MethodCallModel>(),
                new ModelJsonConverter<IMethodTypeWithLocalFunctions, MethodModel>(),
                new ModelJsonConverter<IAttributeType, AttributeModel>(),
                new ModelJsonConverter<ILocalVariableType, LocalVariableModel>(),
                new ModelJsonConverter<IGenericParameterType, GenericParameterModel>(),
                new ModelJsonConverter<ICompilationUnitType, CompilationUnitModel>(),

                new ClassTypeConverter(new CSharpClassTypeConverter())
            };
        }
    }
}

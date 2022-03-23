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
                new ModelJsonConverter<IAccessorType, AccessorModel>(),
                new ModelJsonConverter<IAttributeType, AttributeModel>(),
                new ModelJsonConverter<IBaseType, BaseTypeModel>(),
                new ClassTypeConverter(new CSharpClassTypeConverter()),
                new ModelJsonConverter<ICompilationUnitType, CompilationUnitModel>(),
                new ModelJsonConverter<IConstructorType, ConstructorModel>(),
                new ModelJsonConverter<IDestructorType, DestructorModel>(),
                new ModelJsonConverter<IEntityType, EntityTypeModel>(),
                new ModelJsonConverter<IEnumType, EnumModel>(),
                new ModelJsonConverter<IEnumLabelType, EnumLabelType>(),
                new ModelJsonConverter<IFieldType, FieldModel>(),
                new ModelJsonConverter<IGenericParameterType, GenericParameterModel>(),
                new ModelJsonConverter<ILocalVariableType, LocalVariableModel>(),
                new ModelJsonConverter<IMethodCallType, MethodCallModel>(),
                new ModelJsonConverter<IMethodType, MethodModel>(),
                new ModelJsonConverter<IMethodTypeWithLocalFunctions, MethodModel>(),
                new ModelJsonConverter<IParameterType, ParameterModel>(),
                new ModelJsonConverter<IPropertyType, PropertyModel>(),
                new ModelJsonConverter<IRepositoryModel, RepositoryModel>(),
                new ModelJsonConverter<IReturnValueType, ReturnValueModel>(),
                new ModelJsonConverter<IImportType, UsingModel>(),
            };
        }
    }
}

using System.Text.Json;
using HoneydewModels.CSharp;
using HoneydewModels.Types;

namespace HoneydewModels.Importers
{
    public class JsonSolutionModelImporter : IModelImporter<SolutionModel>
    {
        public SolutionModel Import(string fileContent)
        {
            return JsonSerializer.Deserialize<SolutionModel>(fileContent, new JsonSerializerOptions
            {
                Converters =
                {
                    new ModelJsonConverter<IEntityType, EntityTypeModel>(),
                    new ModelJsonConverter<IParameterType, ParameterModel>(),
                    new ModelJsonConverter<IBaseType, BaseTypeModel>(),
                    new ModelJsonConverter<IConstructorType, ConstructorModel>(),
                    new ModelJsonConverter<IMethodType, MethodModel>(),
                    new ModelJsonConverter<IClassType, ClassModel>(),
                    new ModelJsonConverter<IFieldType, FieldModel>(),
                    new ModelJsonConverter<IPropertyType, PropertyModel>(),
                    new ModelJsonConverter<IReturnValueType, ReturnValueModel>(),
                }
            });
        }
    }
}

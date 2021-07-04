using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using HoneydewCore.Models;
using HoneydewCore.Models.Representations.ReferenceModel;

namespace HoneydewCore.IO.Writers.JSON
{
    public class JsonReferenceSolutionModelSerializer
    {
        private const string ProjectIdentifier = "project";
        private const string NamespaceIdentifier = "namespace";
        private const string ClassIdentifier = "class";
        private const string FieldIdentifier = "field";
        private const string MethodIdentifier = "method";
        private const string OtherClassIdentifier = "other-class";

        private readonly Dictionary<ReferenceEntity, int> _serializedEntities = new();
        private int _currentId;

        private JsonReferenceSolutionModelSerializer()
        {
        }

        public static string Serialize(ReferenceSolutionModel model)
        {
            return new JsonReferenceSolutionModelSerializer().SerializeSolution(model);
        }

        private string SerializeSolution(ReferenceSolutionModel model)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append(@"{""entities"":");
            stringBuilder.Append(SerializeAllEntities(model));

            stringBuilder.Append(@",""model"":{""Projects"":[");
            for (var index = 0; index < model.Projects.Count; index++)
            {
                stringBuilder.Append(SerializeProject(model.Projects[index]));
                if (index != model.Projects.Count - 1)
                {
                    stringBuilder.Append(',');
                }
            }

            stringBuilder.Append("]}}");

            return stringBuilder.ToString();
        }

        private string SerializeAllEntities(ReferenceSolutionModel model)
        {
            var stringBuilder = new StringBuilder();

            var entities = new List<EntitySerializedInfo>();

            foreach (var projectModel in model.Projects)
            {
                var projectId = _currentId;
                entities.Add(AddEntitySerializedInfo(projectModel, ProjectIdentifier, null));

                foreach (var namespaceModel in projectModel.Namespaces)
                {
                    var namespaceId = _currentId;
                    entities.Add(AddEntitySerializedInfo(namespaceModel, NamespaceIdentifier, projectId));

                    foreach (var classModel in namespaceModel.ClassModels)
                    {
                        var classId = _currentId;
                        entities.Add(AddEntitySerializedInfo(classModel, ClassIdentifier, namespaceId));

                        entities.AddRange(classModel.Fields.Select(fieldModel =>
                            AddEntitySerializedInfo(fieldModel, FieldIdentifier, classId)));

                        entities.AddRange(classModel.Methods.Select(methodModel =>
                            AddEntitySerializedInfo(methodModel, MethodIdentifier, classId)));
                    }
                }
            }

            var createdReferences = model.GetAllCreatedReferences();
            entities.AddRange(createdReferences.Select(classModel =>
                AddEntitySerializedInfo(classModel, OtherClassIdentifier, null)));

            EntitySerializedInfo AddEntitySerializedInfo(ReferenceEntity referenceEntity, string type, int? container)
            {
                var serializedInfo = new EntitySerializedInfo
                {
                    Id = _currentId,
                    Name = referenceEntity.Name,
                    Type = type,
                    Container = container,
                };
                _serializedEntities.Add(referenceEntity, _currentId);

                _currentId++;

                return serializedInfo;
            }


            stringBuilder.Append(JsonSerializer.Serialize(entities));

            return stringBuilder.ToString();
        }


        private string SerializeProject(ReferenceProjectModel model)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append($@"{{""Name"":""{model.Name}"",""Namespaces"":[");

            for (var index = 0; index < model.Namespaces.Count; index++)
            {
                stringBuilder.Append(SerializeNamespace(model.Namespaces[index]));
                if (index != model.Namespaces.Count - 1)
                {
                    stringBuilder.Append(',');
                }
            }

            stringBuilder.Append(@"]}");

            return stringBuilder.ToString();
        }

        private string SerializeNamespace(ReferenceNamespaceModel model)
        {
            var stringBuilder = new StringBuilder();

            var projectId = _serializedEntities[model.ProjectReference];

            stringBuilder.Append(
                $@"{{""Name"":""{model.Name}"",""ReferenceProjectModel"":{projectId},""ClassModels"":[");

            for (var index = 0; index < model.ClassModels.Count; index++)
            {
                stringBuilder.Append(SerializeClass(model.ClassModels[index]));
                if (index != model.ClassModels.Count - 1)
                {
                    stringBuilder.Append(',');
                }
            }

            stringBuilder.Append(@"]}");

            return stringBuilder.ToString();
        }

        private string SerializeClass(ReferenceClassModel model)
        {
            var stringBuilder = new StringBuilder();

            var namespaceId = _serializedEntities[model.NamespaceReference];
            var baseClassId = model.BaseClass == null ? "null" : _serializedEntities[model.BaseClass].ToString();

            stringBuilder.Append(
                $@"{{""Name"":""{model.Name}"",""ClassType"":""{model.ClassType}"",""FilePath"":""{model.FilePath}""");
            stringBuilder.Append($@",""AccessModifier"":""{model.AccessModifier}"",""Modifier"":""{model.Modifier}""");
            stringBuilder.Append($@",""NamespaceReference"":{namespaceId},""BaseClass"":{baseClassId}");
            stringBuilder.Append(@",""BaseInterfaces"":[");
            for (var index = 0; index < model.BaseInterfaces.Count; index++)
            {
                var baseInterfaceId = _serializedEntities[model.BaseInterfaces[index]];
                stringBuilder.Append(baseInterfaceId);
                if (index != model.BaseInterfaces.Count - 1)
                {
                    stringBuilder.Append(',');
                }
            }

            stringBuilder.Append(@"],""Fields"":[");
            for (var index = 0; index < model.Fields.Count; index++)
            {
                stringBuilder.Append(SerializeField(model.Fields[index]));
                if (index != model.Fields.Count - 1)
                {
                    stringBuilder.Append(',');
                }
            }

            stringBuilder.Append(@"],""Methods"":[");
            for (var index = 0; index < model.Methods.Count; index++)
            {
                stringBuilder.Append(SerializeMethod(model.Methods[index]));
                if (index != model.Methods.Count - 1)
                {
                    stringBuilder.Append(',');
                }
            }

            stringBuilder.Append(@"],""Metrics"":[");
            for (var index = 0; index < model.Metrics.Count; index++)
            {
                stringBuilder.Append(SerializeMetric(model.Metrics[index]));
                if (index != model.Metrics.Count - 1)
                {
                    stringBuilder.Append(',');
                }
            }


            stringBuilder.Append("]}");

            return stringBuilder.ToString();
        }

        private string SerializeField(ReferenceFieldModel model)
        {
            var stringBuilder = new StringBuilder();

            var containingClassId = _serializedEntities[model.ContainingClass];
            var typeId = _serializedEntities[model.Type];
            var isEvent = model.IsEvent ? "true" : "false";
            
            stringBuilder.Append($@"{{""Name"":""{model.Name}"",""ContainingClass"":{containingClassId}");
            stringBuilder.Append(
                $@",""Type"":{typeId},""Modifier"":""{model.Modifier}"",""AccessModifier"":""{model.AccessModifier}""");
            stringBuilder.Append($@",""IsEvent"":{isEvent}}}");

            return stringBuilder.ToString();
        }

        private string SerializeMethod(ReferenceMethodModel model)
        {
            var stringBuilder = new StringBuilder();

            var containingClassId = _serializedEntities[model.ContainingClass];
            var returnTypeId = _serializedEntities[model.ReturnTypeReferenceClassModel];

            stringBuilder.Append($@"{{""Name"":""{model.Name}"",""ContainingClass"":{containingClassId}");
            stringBuilder.Append($@",""Modifier"":""{model.Modifier}"",""AccessModifier"":""{model.AccessModifier}""");
            stringBuilder.Append($@",""ReturnTypeReferenceClassModel"":{returnTypeId}");

            stringBuilder.Append(@",""ParameterTypes"":[");
            for (var index = 0; index < model.ParameterTypes.Count; index++)
            {
                var parameterId = _serializedEntities[model.ParameterTypes[index]];
                stringBuilder.Append(parameterId);
                if (index != model.ParameterTypes.Count - 1)
                {
                    stringBuilder.Append(',');
                }
            }

            stringBuilder.Append(@"],""CalledMethods"":[");
            for (var index = 0; index < model.CalledMethods.Count; index++)
            {
                var calledMethodId = _serializedEntities[model.CalledMethods[index]];
                stringBuilder.Append(calledMethodId);
                if (index != model.CalledMethods.Count - 1)
                {
                    stringBuilder.Append(',');
                }
            }

            stringBuilder.Append("]}");

            return stringBuilder.ToString();
        }

        private static string SerializeMetric(ClassMetric model)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append(
                $@"{{""ExtractorName"":""{model.ExtractorName}"",""Value"":""{model.Value}"",""ValueType"":""{model.ValueType}""}}");


            return stringBuilder.ToString();
        }
    }
}
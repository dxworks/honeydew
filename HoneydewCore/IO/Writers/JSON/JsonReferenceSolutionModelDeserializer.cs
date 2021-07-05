using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using HoneydewCore.Models;
using HoneydewCore.Models.Representations.ReferenceModel;

namespace HoneydewCore.IO.Writers.JSON
{
    public class JsonReferenceSolutionModelDeserializer
    {
        private readonly Dictionary<int, ReferenceEntity> _deserializedEntities = new();
        private readonly List<ReferenceMethodModel> _referenceMethodModels = new();

        private JsonReferenceSolutionModelDeserializer()
        {
        }

        public static ReferenceSolutionModel Deserialize(string content)
        {
            return new JsonReferenceSolutionModelDeserializer().DeserializeSolution(content);
        }

        private ReferenceSolutionModel DeserializeSolution(string content)
        {
            ReferenceSolutionModel referenceSolutionModel = new();

            Dictionary<string, object> values;
            try
            {
                values = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
            }
            catch (Exception)
            {
                return referenceSolutionModel;
            }

            if (values == null)
            {
                return referenceSolutionModel;
            }

            if (!values.TryGetValue("entities", out var entities))
            {
                return referenceSolutionModel;
            }

            if (!values.TryGetValue("model", out var model))
            {
                return referenceSolutionModel;
            }

            var entitiesJsonElement = (JsonElement) entities;
            var modelJsonElement = (JsonElement) model;

            try
            {
                DeserializeEntities(entitiesJsonElement, referenceSolutionModel);

                DeserializeSolutionModel(modelJsonElement, referenceSolutionModel);

                DescrambleMethodNames();

                return referenceSolutionModel;
            }
            catch (Exception)
            {
                return referenceSolutionModel;
            }
        }

        private void DescrambleMethodNames()
        {
            foreach (var methodModel in _referenceMethodModels)
            {
                var underscoreIndex = methodModel.Name.IndexOf('_');
                if (underscoreIndex >= 0)
                {
                    methodModel.Name = methodModel.Name.Remove(underscoreIndex);
                }
            }
        }

        private void DeserializeSolutionModel(JsonElement solutionModelJsonElement,
            ReferenceSolutionModel referenceSolutionModel)
        {
            var projectsProperty = solutionModelJsonElement.GetProperty("Projects");

            foreach (var projectElement in projectsProperty.EnumerateArray())
            {
                var projectName = projectElement.GetProperty("Name").GetString();
                var referenceProjectModel = referenceSolutionModel.Projects.First(model => model.Name == projectName);

                var namespacesProperty = projectElement.GetProperty("Namespaces");

                foreach (var namespaceElement in namespacesProperty.EnumerateArray())
                {
                    DeserializeNamespaceModel(referenceProjectModel, namespaceElement);
                }
            }
        }

        private void DeserializeNamespaceModel(ReferenceProjectModel referenceProjectModel,
            JsonElement namespacesElement)
        {
            var namespaceName = namespacesElement.GetProperty("Name").GetString();
            var referenceNamespaceModel = referenceProjectModel.Namespaces.First(model => model.Name == namespaceName);

            var classModelsArray = namespacesElement.GetProperty("ClassModels");

            foreach (var classElement in classModelsArray.EnumerateArray())
            {
                DeserializeClassModel(referenceNamespaceModel, classElement);
            }
        }

        private void DeserializeClassModel(ReferenceNamespaceModel referenceNamespaceModel, JsonElement classElement)
        {
            var className = classElement.GetProperty("Name").GetString();
            var classModel = referenceNamespaceModel.ClassModels.First(model => model.Name == className);

            classModel.ClassType = classElement.GetProperty("ClassType").GetString();
            classModel.FilePath = classElement.GetProperty("FilePath").GetString();
            classModel.AccessModifier = classElement.GetProperty("AccessModifier").GetString();
            classModel.Modifier = classElement.GetProperty("Modifier").GetString();

            var baseClassElement = classElement.GetProperty("BaseClass").GetRawText();
            if (baseClassElement != "null")
            {
                var baseClassId = classElement.GetProperty("BaseClass").GetInt32();
                classModel.BaseClass = (ReferenceClassModel) _deserializedEntities[baseClassId];
            }

            var baseInterfacesArray = classElement.GetProperty("BaseInterfaces");
            foreach (var baseInterfaceElement in baseInterfacesArray.EnumerateArray())
            {
                var baseInterfaceId = baseInterfaceElement.GetInt32();
                classModel.BaseInterfaces.Add((ReferenceClassModel) _deserializedEntities[baseInterfaceId]);
            }

            var fieldsArray = classElement.GetProperty("Fields");
            foreach (var fieldElement in fieldsArray.EnumerateArray())
            {
                DeserializeFieldModel(classModel, fieldElement);
            }

            var constructorsArray = classElement.GetProperty("Constructors");
            foreach (var methodElement in constructorsArray.EnumerateArray())
            {
                DeserializeConstructorModel(classModel, methodElement);
            }
            
            var methodsArray = classElement.GetProperty("Methods");
            foreach (var methodElement in methodsArray.EnumerateArray())
            {
                DeserializeMethodModel(classModel, methodElement);
            }

            var metricsArray = classElement.GetProperty("Metrics");
            foreach (var metricElement in metricsArray.EnumerateArray())
            {
                var valueType = metricElement.GetProperty("ValueType").GetString();
                object value = metricElement.GetProperty("Value").ToString();

                if (valueType != null)
                {
                    var type = Type.GetType(valueType);
                    if (type != null)
                    {
                        value = Convert.ChangeType(value, type);
                    }
                }
                
                classModel.Metrics.Add(new ClassMetric
                {
                    ExtractorName = metricElement.GetProperty("ExtractorName").GetString(),
                    ValueType = valueType,
                    Value = value
                });
            }
        }

        private void DeserializeFieldModel(ReferenceClassModel classModel, JsonElement fieldElement)
        {
            var fieldName = fieldElement.GetProperty("Name").GetString();
            var fieldModel = classModel.Fields.First(model => model.Name == fieldName);
            fieldModel.AccessModifier = fieldElement.GetProperty("AccessModifier").GetString();
            fieldModel.Modifier = fieldElement.GetProperty("Modifier").GetString();
            fieldModel.IsEvent = fieldElement.GetProperty("IsEvent").GetBoolean();
            var typeId = fieldElement.GetProperty("Type").GetInt32();
            fieldModel.Type = (ReferenceClassModel) _deserializedEntities[typeId];
        }

        private void DeserializeMethodModel(ReferenceClassModel classModel, JsonElement methodElement)
        {
            var methodName = methodElement.GetProperty("Name").GetString();
            var methodModel = classModel.Methods.First(model => model.Name == methodName);
            DeserializeMethodInfo(methodElement, methodModel);
        }
        
        private void DeserializeConstructorModel(ReferenceClassModel classModel, JsonElement methodElement)
        {
            var methodName = methodElement.GetProperty("Name").GetString();
            var methodModel = classModel.Constructors.First(model => model.Name == methodName);
            DeserializeMethodInfo(methodElement, methodModel);
        }

        private void DeserializeMethodInfo(JsonElement methodElement, ReferenceMethodModel methodModel)
        {
            methodModel.AccessModifier = methodElement.GetProperty("AccessModifier").GetString();
            methodModel.Modifier = methodElement.GetProperty("Modifier").GetString();
            methodModel.IsConstructor = methodElement.GetProperty("IsConstructor").GetBoolean();
            
            var returnTypeText = methodElement.GetProperty("ReturnTypeReferenceClassModel").GetRawText();
            if (returnTypeText != "null")
            {
                var returnTypeId = methodElement.GetProperty("ReturnTypeReferenceClassModel").GetInt32();
                methodModel.ReturnTypeReferenceClassModel = (ReferenceClassModel) _deserializedEntities[returnTypeId];
            }

            var parameterTypesArray = methodElement.GetProperty("ParameterTypes");
            foreach (var parameterElement in parameterTypesArray.EnumerateArray())
            {
                var parameterId = parameterElement.GetProperty("Type").GetInt32();
                methodModel.ParameterTypes.Add(new ReferenceParameterModel
                {
                    Type = (ReferenceClassModel) _deserializedEntities[parameterId],
                    Modifier = parameterElement.GetProperty("Modifier").GetString(),
                    DefaultValue = parameterElement.GetProperty("DefaultValue").GetString(),
                });
            }

            var calledMethodsArray = methodElement.GetProperty("CalledMethods");
            foreach (var calledMethodElement in calledMethodsArray.EnumerateArray())
            {
                var calledMethodId = calledMethodElement.GetInt32();
                methodModel.CalledMethods.Add((ReferenceMethodModel) _deserializedEntities[calledMethodId]);
            }
        }

        private void DeserializeEntities(JsonElement entitiesJsonElement, ReferenceSolutionModel referenceSolutionModel)
        {
            for (var i = 0; i < entitiesJsonElement.GetArrayLength(); i++)
            {
                try
                {
                    var entitySerializedInfo =
                        JsonSerializer.Deserialize<EntitySerializedInfo>(entitiesJsonElement[i]!.ToString()!);
                    switch (entitySerializedInfo!.Type)
                    {
                        case JsonReferenceSolutionModelsConstants.ProjectIdentifier:
                        {
                            var referenceProjectModel = new ReferenceProjectModel
                            {
                                Name = entitySerializedInfo.Name,
                                SolutionReference = referenceSolutionModel
                            };
                            _deserializedEntities.Add(entitySerializedInfo.Id, referenceProjectModel);
                            referenceSolutionModel.Projects.Add(referenceProjectModel);
                        }
                            break;

                        case JsonReferenceSolutionModelsConstants.NamespaceIdentifier:
                        {
                            var projectReference =
                                (ReferenceProjectModel) _deserializedEntities[(int) entitySerializedInfo.Container!];

                            var referenceNamespaceModel = new ReferenceNamespaceModel
                            {
                                Name = entitySerializedInfo.Name,
                                ProjectReference = projectReference
                            };
                            _deserializedEntities.Add(entitySerializedInfo.Id, referenceNamespaceModel);
                            projectReference.Namespaces.Add(referenceNamespaceModel);
                        }
                            break;

                        case JsonReferenceSolutionModelsConstants.ClassIdentifier:
                        {
                            var namespaceReference =
                                (ReferenceNamespaceModel) _deserializedEntities[(int) entitySerializedInfo.Container!];

                            var referenceNamespaceModel = new ReferenceClassModel
                            {
                                Name = entitySerializedInfo.Name,
                                NamespaceReference = namespaceReference
                            };
                            _deserializedEntities.Add(entitySerializedInfo.Id, referenceNamespaceModel);
                            namespaceReference.ClassModels.Add(referenceNamespaceModel);
                        }
                            break;

                        case JsonReferenceSolutionModelsConstants.FieldIdentifier:
                        {
                            var referenceClassModel =
                                (ReferenceClassModel) _deserializedEntities[(int) entitySerializedInfo.Container!];

                            var referenceFieldModel = new ReferenceFieldModel
                            {
                                Name = entitySerializedInfo.Name,
                                ContainingClass = referenceClassModel
                            };
                            _deserializedEntities.Add(entitySerializedInfo.Id, referenceFieldModel);
                            referenceClassModel.Fields.Add(referenceFieldModel);
                        }
                            break;

                        case JsonReferenceSolutionModelsConstants.MethodIdentifier:
                        {
                            var referenceClassModel =
                                (ReferenceClassModel) _deserializedEntities[(int) entitySerializedInfo.Container!];

                            var referenceMethodModel = new ReferenceMethodModel
                            {
                                Name = entitySerializedInfo.Name,
                                ContainingClass = referenceClassModel
                            };
                            _deserializedEntities.Add(entitySerializedInfo.Id, referenceMethodModel);
                            referenceClassModel.Methods.Add(referenceMethodModel);
                            _referenceMethodModels.Add(referenceMethodModel);
                        }
                            break;
                        
                        case JsonReferenceSolutionModelsConstants.ConstructorIdentifier:
                        {
                            var referenceClassModel =
                                (ReferenceClassModel) _deserializedEntities[(int) entitySerializedInfo.Container!];

                            var referenceMethodModel = new ReferenceMethodModel
                            {
                                Name = entitySerializedInfo.Name,
                                ContainingClass = referenceClassModel
                            };
                            _deserializedEntities.Add(entitySerializedInfo.Id, referenceMethodModel);
                            referenceClassModel.Constructors.Add(referenceMethodModel);
                            _referenceMethodModels.Add(referenceMethodModel);
                        }
                            break;

                        case JsonReferenceSolutionModelsConstants.OtherClassIdentifier:
                        {
                            var referenceMethodModel =
                                referenceSolutionModel.FindOrCreateClassModel(entitySerializedInfo.Name);
                            _deserializedEntities.Add(entitySerializedInfo.Id, referenceMethodModel);
                        }
                            break;
                    }
                }
                catch (Exception)
                {
                    // when exception occurs, just ignore the current item
                }
            }
        }
    }
}
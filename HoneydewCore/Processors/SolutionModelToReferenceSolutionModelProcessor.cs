using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Models;
using HoneydewCore.Models.Representations.ReferenceModel;

namespace HoneydewCore.Processors
{
    public class
        SolutionModelToReferenceSolutionModelProcessor : IProcessorFunction<SolutionModel, ReferenceSolutionModel>
    {
        public Func<Processable<SolutionModel>, Processable<ReferenceSolutionModel>> GetFunction()
        {
            return processable =>
            {
                var solutionModel = processable.Value;

                var referenceSolutionModel = new ReferenceSolutionModel();

                if (solutionModel == null)
                    return new Processable<ReferenceSolutionModel>(referenceSolutionModel);

                PopulateModelWithProjectNamespacesAndClasses(solutionModel, referenceSolutionModel);

                PopulateModelWithBaseClassesAndInterfaces(solutionModel, referenceSolutionModel);

                PopulateModelWithMethodsAndFields(solutionModel, referenceSolutionModel);

                PopulateModelWithMethodReferences(solutionModel, referenceSolutionModel);

                return new Processable<ReferenceSolutionModel>(referenceSolutionModel);
            };
        }

        private static void PopulateModelWithProjectNamespacesAndClasses(SolutionModel solutionModel,
            ReferenceSolutionModel referenceSolutionModel)
        {
            foreach (var projectModel in solutionModel.Projects)
            {
                var referenceProjectModel = new ReferenceProjectModel
                {
                    Name = projectModel.Name,
                    SolutionReference = referenceSolutionModel
                };

                foreach (var (namespaceName, namespaceModel) in projectModel.Namespaces)
                {
                    var referenceNamespaceModel = new ReferenceNamespaceModel
                    {
                        Name = namespaceName,
                        ProjectReference = referenceProjectModel
                    };

                    foreach (var classModel in namespaceModel.ClassModels)
                    {
                        var referenceClassModel = new ReferenceClassModel
                        {
                            ClassType = classModel.ClassType,
                            Name = classModel.FullName,
                            FilePath = classModel.FilePath,
                            AccessModifier = classModel.AccessModifier,
                            Modifier = classModel.Modifier,
                            NamespaceReference = referenceNamespaceModel,
                            Metrics = classModel.Metrics
                        };

                        referenceNamespaceModel.ClassModels.Add(referenceClassModel);
                    }

                    referenceProjectModel.Namespaces.Add(referenceNamespaceModel);
                }

                referenceSolutionModel.Projects.Add(referenceProjectModel);
            }
        }

        private void PopulateModelWithBaseClassesAndInterfaces(SolutionModel solutionModel,
            ReferenceSolutionModel referenceSolutionModel)
        {
            foreach (var classModel in solutionModel.GetEnumerable())
            {
                var referenceEntity = referenceSolutionModel.FindFirst(entity =>
                    entity is ReferenceClassModel && entity.Name == classModel.FullName);

                if (referenceEntity == null) continue;
                var referenceClassModel = (ReferenceClassModel) referenceEntity;

                referenceClassModel.BaseClass =
                    GetClassReferenceByName(referenceSolutionModel, classModel.BaseClassFullName);

                foreach (var interfaceName in classModel.BaseInterfaces)
                {
                    referenceClassModel.BaseInterfaces.Add(GetClassReferenceByName(referenceSolutionModel,
                        interfaceName));
                }
            }
        }

        private void PopulateModelWithMethodsAndFields(SolutionModel solutionModel,
            ReferenceSolutionModel referenceSolutionModel)
        {
            foreach (var classModel in solutionModel.GetEnumerable())
            {
                var referenceEntity = referenceSolutionModel.FindFirst(entity =>
                    entity is ReferenceClassModel && entity.Name == classModel.FullName);

                if (referenceEntity == null) continue;
                var referenceClassModel = (ReferenceClassModel) referenceEntity;

                PopulateWithMethodModels(classModel.Methods, referenceClassModel, referenceClassModel.Methods);

                PopulateWithMethodModels(classModel.Constructors, referenceClassModel,
                    referenceClassModel.Constructors);

                foreach (var fieldModel in classModel.Fields)
                {
                    referenceClassModel.Fields.Add(new ReferenceFieldModel
                    {
                        Name = fieldModel.Name,
                        Modifier = fieldModel.Modifier,
                        AccessModifier = fieldModel.AccessModifier,
                        IsEvent = fieldModel.IsEvent,
                        ContainingClass = referenceClassModel,
                        Type = GetClassReferenceByName(referenceSolutionModel, fieldModel.Type)
                    });
                }
            }

            void PopulateWithMethodModels(IEnumerable<MethodModel> methodModels,
                ReferenceClassModel referenceClassModel, ICollection<ReferenceMethodModel> outputList)
            {
                foreach (var constructorModel in methodModels)
                {
                    IList<ReferenceParameterModel> referenceClassModels =
                        ExtractParameterModels(referenceSolutionModel, constructorModel.ParameterTypes);

                    var referenceMethodModel = new ReferenceMethodModel
                    {
                        Modifier = constructorModel.Modifier,
                        Name = constructorModel.Name,
                        IsConstructor = constructorModel.IsConstructor,
                        AccessModifier = constructorModel.AccessModifier,
                        ContainingClass = referenceClassModel,
                        ReturnTypeReferenceClassModel =
                            GetClassReferenceByName(referenceSolutionModel, constructorModel.ReturnType),
                        ParameterTypes = referenceClassModels
                    };

                    outputList.Add(referenceMethodModel);
                }
            }
        }

        private void PopulateModelWithMethodReferences(SolutionModel solutionModel,
            ReferenceSolutionModel referenceSolutionModel)
        {
            foreach (var classModel in solutionModel.GetEnumerable())
            {
                var referenceEntity = referenceSolutionModel.FindFirst(entity =>
                    entity is ReferenceClassModel && entity.Name == classModel.FullName);

                if (referenceEntity == null) continue;
                var referenceClassModel = (ReferenceClassModel) referenceEntity;

                foreach (var methodModel in classModel.Methods)
                {
                    var referenceMethodModel = GetMethodReference(referenceSolutionModel, referenceClassModel,
                        methodModel.Name,
                        methodModel.ParameterTypes);

                    foreach (var calledMethod in methodModel.CalledMethods)
                    {
                        var calledMethodClass = GetClassReferenceByName(referenceSolutionModel,
                            calledMethod.ContainingClassName);
                        var calledReferenceMethodModel = GetMethodReference(referenceSolutionModel,
                            calledMethodClass,
                            calledMethod.MethodName, calledMethod.ParameterTypes);

                        referenceMethodModel.CalledMethods.Add(calledReferenceMethodModel);
                    }
                }
            }
        }

        private ReferenceMethodModel GetMethodReference(ReferenceSolutionModel referenceSolutionModel,
            ReferenceClassModel referenceClassModel, string methodName, IList<ParameterModel> parameterTypes)
        {
            var methodReference = referenceClassModel.Methods.FirstOrDefault(method =>
            {
                if (method.Name != methodName ||
                    method.ParameterTypes.Count != parameterTypes.Count) return false;

                for (var i = 0; i < method.ParameterTypes.Count; i++)
                {
                    if (method.ParameterTypes[i].Type.Name != parameterTypes[i].Type)
                    {
                        return false;
                    }
                }

                return true;
            });

            if (methodReference != null || referenceClassModel.NamespaceReference != null ||
                !string.IsNullOrEmpty(referenceClassModel.FilePath))
            {
                return methodReference;
            }


            var newlyAddReferenceMethodModel = new ReferenceMethodModel
            {
                ContainingClass = referenceClassModel,
                Name = methodName,
                ParameterTypes = ExtractParameterModels(referenceSolutionModel, parameterTypes)
            };
            referenceClassModel.Methods.Add(newlyAddReferenceMethodModel);

            return newlyAddReferenceMethodModel;
        }

        private List<ReferenceParameterModel> ExtractParameterModels(ReferenceSolutionModel referenceSolutionModel,
            IEnumerable<ParameterModel> parameterModels)
        {
            return parameterModels
                .Select(parameterModel =>
                {
                    var classReferenceByName =
                        GetClassReferenceByName(referenceSolutionModel, parameterModel.Type);
                    return new ReferenceParameterModel
                    {
                        Type = classReferenceByName,
                        Modifier = parameterModel.Modifier,
                        DefaultValue = parameterModel.DefaultValue
                    };
                }).ToList();
        }

        private ReferenceClassModel GetClassReferenceByName(ReferenceSolutionModel referenceSolutionModel,
            string className)
        {
            var referenceEntity = referenceSolutionModel.FindFirst(entity =>
                entity is ReferenceClassModel && entity.Name == className);
            if (referenceEntity != null)
            {
                return (ReferenceClassModel) referenceEntity;
            }

            return referenceSolutionModel.FindOrCreateClassModel(className);
        }
    }
}
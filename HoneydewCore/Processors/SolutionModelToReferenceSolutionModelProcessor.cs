﻿using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Models;
using HoneydewCore.Models.Representations.ReferenceModel;

namespace HoneydewCore.Processors
{
    // todo add test case for calling methods with concrete types for method with abstract type
    // todo add test case for metrics

    public class
        SolutionModelToReferenceSolutionModelProcessor : IProcessorFunction<SolutionModel, ReferenceSolutionModel>
    {
        private readonly IMissingClassModelsHandler _missingClassModelsHandler;

        public SolutionModelToReferenceSolutionModelProcessor(IMissingClassModelsHandler missingClassModelsHandler)
        {
            _missingClassModelsHandler = missingClassModelsHandler;
        }

        public SolutionModelToReferenceSolutionModelProcessor()
        {
            _missingClassModelsHandler = new MissingClassModelsHandler();
        }

        public Func<Processable<SolutionModel>, Processable<ReferenceSolutionModel>> GetFunction()
        {
            return processable =>
            {
                var solutionModel = processable.Value;

                var referenceSolutionModel = new ReferenceSolutionModel();

                if (solutionModel == null)
                    return new Processable<ReferenceSolutionModel>(referenceSolutionModel);

                PopulateModelWithProjectNamespacesAndClasses(solutionModel, referenceSolutionModel);

                PopulateModelWithMethodsAndFields(solutionModel, referenceSolutionModel);

                PopulateModelWithMethodReferences(solutionModel, referenceSolutionModel);

                referenceSolutionModel.ClassModelsNotDeclaredInSolution = _missingClassModelsHandler.GetAllReferences();

                return new Processable<ReferenceSolutionModel>(referenceSolutionModel);
            };
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

        private void PopulateModelWithMethodsAndFields(SolutionModel solutionModel,
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
                    IList<ReferenceClassModel> referenceClassModels = methodModel.ParameterTypes
                        .Select(methodModelParameterType =>
                            GetClassReferenceByName(referenceSolutionModel, methodModelParameterType)).ToList();

                    var referenceMethodModel = new ReferenceMethodModel
                    {
                        Modifier = methodModel.Modifier,
                        Name = methodModel.Name,
                        AccessModifier = methodModel.AccessModifier,
                        ContainingClass = referenceClassModel,
                        ReturnTypeReferenceClassModel =
                            GetClassReferenceByName(referenceSolutionModel, methodModel.ReturnType),
                        ParameterTypes = referenceClassModels
                    };

                    referenceClassModel.Methods.Add(referenceMethodModel);
                }

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
                            Name = classModel.FullName,
                            FilePath = classModel.FilePath,
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

        private ReferenceMethodModel GetMethodReference(ReferenceSolutionModel referenceSolutionModel,
            ReferenceClassModel referenceClassModel, string methodName, IList<string> parameterTypes)
        {
            var methodReference = referenceClassModel.Methods.FirstOrDefault(method =>
            {
                if (method.Name != methodName ||
                    method.ParameterTypes.Count != parameterTypes.Count) return false;

                for (var i = 0; i < method.ParameterTypes.Count; i++)
                {
                    if (method.ParameterTypes[i].Name != parameterTypes[i])
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
                ParameterTypes = parameterTypes
                    .Select(parameterType => GetClassReferenceByName(referenceSolutionModel, parameterType)).ToList()
            };
            referenceClassModel.Methods.Add(newlyAddReferenceMethodModel);

            return newlyAddReferenceMethodModel;
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

            return _missingClassModelsHandler.GetAndAddReference(className);
        }
    }
}
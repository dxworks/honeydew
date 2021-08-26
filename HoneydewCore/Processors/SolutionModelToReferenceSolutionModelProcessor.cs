using System.Collections.Generic;
using System.Linq;
using HoneydewModels.CSharp;
using HoneydewModels.CSharp.ReferenceModel;
using HoneydewModels.Types;

namespace HoneydewCore.Processors
{
    public class
        SolutionModelToReferenceSolutionModelProcessor : IProcessorFunction<SolutionModel, ReferenceSolutionModel>
    {
        public ReferenceSolutionModel Process(SolutionModel solutionModel)
        {
            var referenceSolutionModel = new ReferenceSolutionModel();

            if (solutionModel == null)
                return referenceSolutionModel;

            PopulateModelWithProjectNamespacesAndClasses(solutionModel, referenceSolutionModel);

            PopulateModelWithBaseClassesAndInterfaces(solutionModel, referenceSolutionModel);

            PopulateModelWithMethodsAndFields(solutionModel, referenceSolutionModel);

            PopulateModelWithMethodReferences(solutionModel, referenceSolutionModel);

            return referenceSolutionModel;
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

                foreach (var namespaceModel in projectModel.Namespaces)
                {
                    var referenceNamespaceModel = new ReferenceNamespaceModel
                    {
                        Name = namespaceModel.Name,
                        ProjectReference = referenceProjectModel
                    };

                    foreach (var classModel in namespaceModel.ClassModels)
                    {
                        var referenceClassModel = new ReferenceClassModel
                        {
                            ClassType = classModel.ClassType,
                            Name = classModel.Name,
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
                var referenceClassModel =
                    referenceSolutionModel.FindFirstClass(entity => entity.Name == classModel.Name);

                if (referenceClassModel == null) continue;

                var firstOrDefault = classModel.BaseTypes.FirstOrDefault(baseType => baseType.Kind == "class");
                if (firstOrDefault != null)
                {
                    referenceClassModel.BaseClass =
                        GetClassReferenceByName(referenceSolutionModel, firstOrDefault.Type.Name);
                }

                foreach (var baseType in classModel.BaseTypes)
                {
                    if (baseType.Kind == "interface")
                    {
                        referenceClassModel.BaseInterfaces.Add(GetClassReferenceByName(referenceSolutionModel,
                            baseType.Type.Name));
                    }
                }
            }
        }

        private void PopulateModelWithMethodsAndFields(SolutionModel solutionModel,
            ReferenceSolutionModel referenceSolutionModel)
        {
            foreach (var classType in solutionModel.GetEnumerable())
            {
                if (classType is not ClassModel classModel)
                {
                    continue;
                }
                
                var referenceClassModel =
                    referenceSolutionModel.FindFirstClass(entity => entity.Name == classModel.Name);

                PopulateWithMethodModels(classModel.Methods, referenceClassModel,
                    referenceClassModel.Methods);

                PopulateWithConstructorModels(classModel.Constructors, referenceClassModel,
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
                        Type = GetClassReferenceByName(referenceSolutionModel, fieldModel.Type.Name)
                    });
                }
            }

            void PopulateWithMethodModels(IEnumerable<IMethodType> methodModels,
                ReferenceClassModel referenceClassModel, ICollection<ReferenceMethodModel> outputList)
            {
                foreach (var methodModel in methodModels)
                {
                    IList<ReferenceParameterModel> referenceClassModels =
                        ExtractParameterModels(referenceSolutionModel, methodModel.ParameterTypes);

                    var referenceMethodModel = new ReferenceMethodModel
                    {
                        Modifier = methodModel.Modifier,
                        Name = methodModel.Name,
                        AccessModifier = methodModel.AccessModifier,
                        ContainingClass = referenceClassModel,
                        ReturnTypeReferenceClassModel = methodModel.ReturnValue != null
                            ? GetClassReferenceByName(referenceSolutionModel, methodModel.ReturnValue.Type.Name)
                            : null,
                        Parameters = referenceClassModels
                    };

                    outputList.Add(referenceMethodModel);
                }
            }

            void PopulateWithConstructorModels(IEnumerable<IConstructorType> constructorTypes,
                ReferenceClassModel referenceClassModel, ICollection<ReferenceMethodModel> outputList)
            {
                foreach (var constructorType in constructorTypes)
                {
                    IList<ReferenceParameterModel> referenceClassModels =
                        ExtractParameterModels(referenceSolutionModel, constructorType.ParameterTypes);

                    var referenceMethodModel = new ReferenceMethodModel
                    {
                        Modifier = constructorType.Modifier,
                        Name = constructorType.Name,
                        AccessModifier = constructorType.AccessModifier,
                        ContainingClass = referenceClassModel,
                        Parameters = referenceClassModels
                    };

                    outputList.Add(referenceMethodModel);
                }
            }
        }

        private void PopulateModelWithMethodReferences(SolutionModel solutionModel,
            ReferenceSolutionModel referenceSolutionModel)
        {
            foreach (var classType in solutionModel.GetEnumerable())
            {
                if (classType is not ClassModel classModel)
                {
                    continue;
                }
                
                var referenceClassModel =
                    referenceSolutionModel.FindFirstClass(entity => entity.Name == classModel.Name);

                foreach (var methodModel in classModel.Methods)
                {
                    var referenceMethodModel = GetMethodReference(referenceSolutionModel, referenceClassModel,
                        methodModel.Name,
                        methodModel.ParameterTypes);

                    foreach (var calledMethod in methodModel.CalledMethods)
                    {
                        var calledMethodClass = GetClassReferenceByName(referenceSolutionModel,
                            calledMethod.ContainingTypeName);
                        var calledReferenceMethodModel = GetMethodReference(referenceSolutionModel,
                            calledMethodClass,
                            calledMethod.Name, calledMethod.ParameterTypes);

                        referenceMethodModel.CalledMethods.Add(calledReferenceMethodModel);
                    }
                }
            }
        }

        private ReferenceMethodModel GetMethodReference(ReferenceSolutionModel referenceSolutionModel,
            ReferenceClassModel referenceClassModel, string methodName, IList<IParameterType> parameters)
        {
            var methodReference = referenceClassModel.Methods.FirstOrDefault(method =>
            {
                if (method.Name != methodName ||
                    method.Parameters.Count != parameters.Count) return false;

                for (var i = 0; i < method.Parameters.Count; i++)
                {
                    if (method.Parameters[i].Type.Name != parameters[i].Type.Name)
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
                Parameters = ExtractParameterModels(referenceSolutionModel, parameters)
            };
            referenceClassModel.Methods.Add(newlyAddReferenceMethodModel);

            return newlyAddReferenceMethodModel;
        }

        private List<ReferenceParameterModel> ExtractParameterModels(ReferenceSolutionModel referenceSolutionModel,
            IEnumerable<IParameterType> parameterModels)
        {
            return parameterModels
                .Select(parameterType =>
                {
                    var parameterModel = (ParameterModel)parameterType;
                    var classReferenceByName =
                        GetClassReferenceByName(referenceSolutionModel, parameterModel.Type.Name);
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
            var referenceClassModel = referenceSolutionModel.FindFirstClass(entity => entity.Name == className);
            return referenceClassModel != null
                ? referenceClassModel
                : referenceSolutionModel.FindOrCreateClassModel(className);
        }
    }
}

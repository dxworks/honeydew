using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Utils;
using HoneydewModels.CSharp;
using HoneydewModels.Reference;
using HoneydewModels.Types;
using AccessedField = HoneydewModels.Reference.AccessedField;
using AttributeModel = HoneydewModels.Reference.AttributeModel;
using ClassModel = HoneydewModels.Reference.ClassModel;
using DelegateModel = HoneydewModels.CSharp.DelegateModel;
using FieldModel = HoneydewModels.Reference.FieldModel;
using GenericParameterModel = HoneydewModels.Reference.GenericParameterModel;
using GenericType = HoneydewModels.Reference.GenericType;
using LocalVariableModel = HoneydewModels.Reference.LocalVariableModel;
using MethodModel = HoneydewModels.Reference.MethodModel;
using MetricModel = HoneydewModels.Reference.MetricModel;
using ParameterModel = HoneydewModels.Reference.ParameterModel;
using ProjectModel = HoneydewModels.Reference.ProjectModel;
using PropertyModel = HoneydewModels.Reference.PropertyModel;
using RepositoryModel = HoneydewModels.CSharp.RepositoryModel;
using ReturnValueModel = HoneydewModels.Reference.ReturnValueModel;
using SolutionModel = HoneydewModels.Reference.SolutionModel;

namespace HoneydewCore.Processors
{
    public class RepositoryModelToReferenceRepositoryModelProcessor : IProcessorFunction<RepositoryModel,
        HoneydewModels.Reference.RepositoryModel>
    {
        private readonly Dictionary<string, ClassModel> _generatedTypes = new();

        private readonly Dictionary<(ProjectModel, string), ReferenceEntity> _classModels = new();

        private readonly FullTypeNameBuilder _fullTypeNameBuilder = new();

        public HoneydewModels.Reference.RepositoryModel Process(RepositoryModel inputRepositoryModel)
        {
            var repositoryModel = new HoneydewModels.Reference.RepositoryModel();

            if (inputRepositoryModel == null)
                return repositoryModel;

            repositoryModel.Version = inputRepositoryModel.Version;

            PopulateModelWithSolutionProjectNamespacesCompilationUnitsAndClasses(inputRepositoryModel, repositoryModel);

            PopulateWithTypeReferences(inputRepositoryModel, repositoryModel);

            PopulateProjectWithProjectReferences(inputRepositoryModel, repositoryModel);

            PopulateModelWithBaseTypes(inputRepositoryModel, repositoryModel);

            PopulateModelWithMethodsConstructorsPropertiesAndFields(inputRepositoryModel, repositoryModel);

            PopulateModelWithMethodReferences(inputRepositoryModel, repositoryModel);

            repositoryModel.CreatedClasses = _generatedTypes.Select(pair => pair.Value).ToList();

            return repositoryModel;
        }

        private void PopulateModelWithSolutionProjectNamespacesCompilationUnitsAndClasses(
            RepositoryModel repositoryModel, HoneydewModels.Reference.RepositoryModel referenceRepositoryModel)
        {
            var namespaceTreeHandler = new NamespaceTreeHandler();

            foreach (var solutionModel in repositoryModel.Solutions)
            {
                var referenceSolutionModel = new SolutionModel
                {
                    FilePath = solutionModel.FilePath,
                    Repository = referenceRepositoryModel
                };
                referenceRepositoryModel.Solutions.Add(referenceSolutionModel);
            }

            foreach (var projectModel in repositoryModel.Projects)
            {
                var presentNamespacesSet = new HashSet<string>();

                var referenceProjectModel = new ProjectModel
                {
                    Name = projectModel.Name,
                    FilePath = projectModel.FilePath,
                    Repository = referenceRepositoryModel,
                };

                referenceRepositoryModel.Projects.Add(referenceProjectModel);

                foreach (var compilationUnitType in projectModel.CompilationUnits)
                {
                    var referenceCompilationUnit = new FileModel
                    {
                        FilePath = compilationUnitType.FilePath,
                        Project = referenceProjectModel,
                        Loc = ConvertLoc(compilationUnitType.Loc),
                        Imports = compilationUnitType.Imports.Select(import => new ImportModel
                        {
                            Alias = import.Alias,
                            Name = import.Name,
                            AliasType = import.AliasType,
                            IsStatic = import.IsStatic
                        }).ToList(),
                        Metrics = ConvertMetrics(compilationUnitType)
                    };

                    referenceProjectModel.Files.Add(referenceCompilationUnit);

                    foreach (var classType in compilationUnitType.ClassTypes)
                    {
                        var imports = classType.Imports.Select(import => new ImportModel
                        {
                            Alias = import.Alias,
                            Name = import.Name,
                            AliasType = import.AliasType,
                            IsStatic = import.IsStatic
                        }).ToList();

                        var metrics = ConvertMetrics(classType);

                        var genericParameters =
                            ConvertGenericParameters(classType.GenericParameters, referenceProjectModel);

                        var namespaceModel = namespaceTreeHandler.GetOrAdd(classType.ContainingTypeName);
                        presentNamespacesSet.Add(classType.ContainingTypeName);

                        switch (classType)
                        {
                            case DelegateModel delegateModel:
                            {
                                var model = new HoneydewModels.Reference.DelegateModel
                                {
                                    Name = delegateModel.Name,
                                    FilePath = delegateModel.FilePath,
                                    Modifier = delegateModel.Modifier,
                                    AccessModifier = delegateModel.AccessModifier,
                                    Imports = imports,
                                    ClassType = delegateModel.ClassType,
                                    File = referenceCompilationUnit,
                                    Metrics = metrics,
                                    GenericParameters = genericParameters,
                                    Namespace = namespaceModel
                                };
                                model.Attributes = ConvertAttributes(model, delegateModel.Attributes,
                                    referenceProjectModel);
                                model.Parameters = ConvertParameters(model, delegateModel.ParameterTypes,
                                    referenceProjectModel);
                                model.ReturnValue = ConvertReturnValue(model, delegateModel.ReturnValue,
                                    referenceProjectModel);

                                namespaceModel.Delegates.Add(model);
                                referenceCompilationUnit.Delegates.Add(model);

                                _classModels.Add((referenceProjectModel, delegateModel.Name), model);
                            }
                                break;
                            case HoneydewModels.CSharp.ClassModel classModel:
                            {
                                var model = new ClassModel
                                {
                                    Name = classModel.Name,
                                    FilePath = classModel.FilePath,
                                    Modifier = classModel.Modifier,
                                    AccessModifier = classModel.AccessModifier,
                                    Imports = imports,
                                    ClassType = classModel.ClassType,
                                    File = referenceCompilationUnit,
                                    Metrics = metrics,
                                    Loc = ConvertLoc(classModel.Loc),
                                    GenericParameters = genericParameters,
                                    Namespace = namespaceModel
                                };
                                model.Attributes = ConvertAttributes(model, classModel.Attributes,
                                    referenceProjectModel);

                                namespaceModel.Classes.Add(model);
                                referenceCompilationUnit.Classes.Add(model);

                                if (_classModels.TryGetValue((referenceProjectModel, classModel.Name),
                                        out var existingReferenceEntity))
                                {
                                    // this is a partial class, so combine contents
                                    if (existingReferenceEntity is ClassModel existingClassModel)
                                    {
                                        existingClassModel.Attributes.AddRange(existingClassModel.Attributes);
                                        existingClassModel.Imports.AddRange(existingClassModel.Imports);
                                        existingClassModel.Metrics.AddRange(existingClassModel.Metrics);
                                        existingClassModel.Constructors.AddRange(existingClassModel.Constructors);
                                        existingClassModel.Methods.AddRange(existingClassModel.Methods);
                                        existingClassModel.Properties.AddRange(existingClassModel.Properties);
                                        existingClassModel.BaseTypes.AddRange(existingClassModel.BaseTypes);
                                        existingClassModel.Fields.AddRange(existingClassModel.Fields);
                                    }
                                }
                                else
                                {
                                    _classModels.Add((referenceProjectModel, classModel.Name), model);
                                }
                            }
                                break;
                            default:
                                continue;
                        }
                    }
                }

                foreach (var namespaceModel in projectModel.Namespaces)
                {
                    if (!presentNamespacesSet.Contains(namespaceModel.Name))
                    {
                        presentNamespacesSet.Add(namespaceModel.Name);
                    }
                }

                foreach (var namespaceName in presentNamespacesSet)
                {
                    var namespaceModel = namespaceTreeHandler.GetOrAdd(namespaceName);

                    var replaced = false;
                    for (var i = 0; i < referenceProjectModel.Namespaces.Count; i++)
                    {
                        if (referenceProjectModel.Namespaces[i].FullName == namespaceModel.FullName)
                        {
                            referenceProjectModel.Namespaces[i] = namespaceModel;
                            replaced = true;
                            break;
                        }
                    }

                    if (!replaced)
                    {
                        referenceProjectModel.Namespaces.Add(namespaceModel);
                    }
                }
            }

            referenceRepositoryModel.Namespaces = namespaceTreeHandler.GetRootNamespaces();

            for (var solutionIndex = 0; solutionIndex < repositoryModel.Solutions.Count; solutionIndex++)
            {
                var solutionModel = repositoryModel.Solutions[solutionIndex];
                var referenceSolutionModel = referenceRepositoryModel.Solutions[solutionIndex];

                foreach (var path in solutionModel.ProjectsPaths)
                {
                    var projectModel = referenceRepositoryModel.Projects.FirstOrDefault(p => p.FilePath == path);
                    if (projectModel == null)
                    {
                        continue;
                    }

                    referenceSolutionModel.Projects.Add(projectModel);
                    projectModel.Solutions.Add(referenceSolutionModel);
                }
            }
        }

        private void PopulateWithTypeReferences(RepositoryModel repositoryModel,
            HoneydewModels.Reference.RepositoryModel referenceRepositoryModel)
        {
            for (var projectIndex = 0; projectIndex < referenceRepositoryModel.Projects.Count; projectIndex++)
            {
                var projectModel = referenceRepositoryModel.Projects[projectIndex];
                for (var compilationUnitIndex = 0;
                     compilationUnitIndex < projectModel.Files.Count;
                     compilationUnitIndex++)
                {
                    var compilationUnit = projectModel.Files[compilationUnitIndex];
                    var compilationUnitType =
                        repositoryModel.Projects[projectIndex].CompilationUnits[compilationUnitIndex];

                    foreach (var classModel in compilationUnit.Classes)
                    {
                        var classType =
                            compilationUnitType.ClassTypes.FirstOrDefault(c => c.Name == classModel.Name);
                        if (classType == null)
                        {
                            continue;
                        }

                        classModel.Type = ConvertEntityType(classType.Name, classModel, projectModel);
                    }

                    foreach (var delegateModel in compilationUnit.Delegates)
                    {
                        var classType =
                            compilationUnitType.ClassTypes.FirstOrDefault(c => c.Name == delegateModel.Name);
                        if (classType == null)
                        {
                            continue;
                        }

                        delegateModel.Type = ConvertEntityType(classType.Name, delegateModel, projectModel);
                    }
                }
            }
        }

        private static void PopulateProjectWithProjectReferences(RepositoryModel repositoryModel,
            HoneydewModels.Reference.RepositoryModel referenceRepositoryModel)
        {
            var allProjects = referenceRepositoryModel.Solutions.SelectMany(model => model.Projects).ToList();

            for (var projectIndex = 0;
                 projectIndex < repositoryModel.Projects.Count;
                 projectIndex++)
            {
                var projectModel = referenceRepositoryModel.Projects[projectIndex];
                foreach (var projectReference in repositoryModel.Projects[projectIndex].ProjectReferences)
                {
                    var project = allProjects.FirstOrDefault(project => project.FilePath == projectReference);
                    if (project == null)
                    {
                        projectModel.ExternalProjectReferences.Add(projectReference);
                    }
                    else
                    {
                        projectModel.ProjectReferences.Add(project);
                    }
                }
            }
        }

        private void PopulateModelWithBaseTypes(RepositoryModel repositoryModel,
            HoneydewModels.Reference.RepositoryModel referenceRepositoryModel)
        {
            for (var projectIndex = 0; projectIndex < referenceRepositoryModel.Projects.Count; projectIndex++)
            {
                var projectModel = referenceRepositoryModel.Projects[projectIndex];
                for (var compilationUnitIndex = 0;
                     compilationUnitIndex < projectModel.Files.Count;
                     compilationUnitIndex++)
                {
                    var compilationUnit = projectModel.Files[compilationUnitIndex];
                    var compilationUnitType =
                        repositoryModel.Projects[projectIndex].CompilationUnits[compilationUnitIndex];

                    foreach (var classModel in compilationUnit.Classes)
                    {
                        var classType =
                            compilationUnitType.ClassTypes.FirstOrDefault(c => c.Name == classModel.Name);
                        if (classType == null)
                        {
                            continue;
                        }

                        foreach (var baseType in classType.BaseTypes)
                        {
                            if (SearchEntityByName(baseType.Type.Name, projectModel) is ClassModel baseTypeReference)
                            {
                                classModel.BaseTypes.Add(baseTypeReference);
                            }
                        }
                    }

                    foreach (var delegateModel in compilationUnit.Delegates)
                    {
                        var classType =
                            compilationUnitType.ClassTypes.FirstOrDefault(c => c.Name == delegateModel.Name);
                        if (classType == null)
                        {
                            continue;
                        }

                        foreach (var baseType in classType.BaseTypes)
                        {
                            if (SearchEntityByName(baseType.Type.Name, projectModel) is ClassModel baseTypeReference)
                            {
                                delegateModel.BaseTypes.Add(baseTypeReference);
                            }
                        }
                    }
                }
            }
        }

        private ReferenceEntity SearchEntityByName(string className, ProjectModel projectModel, bool shouldAdd = true)
        {
            if (string.IsNullOrEmpty(className))
            {
                return null;
            }

            if (className.EndsWith('?'))
            {
                className = className[..^1];
            }

            if (_classModels.TryGetValue((projectModel, className), out var referenceEntity))
            {
                return referenceEntity;
            }

            if (_generatedTypes.TryGetValue(className, out var generatedType))
            {
                return generatedType;
            }

            var partNames = className.Split('.');

            var namespaceModel = projectModel.Namespaces.FirstOrDefault(n => n.Name == partNames[0]);

            if (namespaceModel != null)
            {
                for (var i = 1; i < partNames.Length - 1; i++)
                {
                    if (namespaceModel == null)
                    {
                        break;
                    }

                    namespaceModel = namespaceModel.ChildNamespaces.FirstOrDefault(n => n.Name == partNames[i]);
                }
            }

            if (!CSharpConstants.IsPrimitive(className) && !shouldAdd)
            {
                return null;
            }

            generatedType = new ClassModel
            {
                Name = className,
                Namespace = namespaceModel,
            };
            generatedType.Type = ConvertEntityType(className, generatedType);

            _generatedTypes.Add(className, generatedType);

            return generatedType;
        }

        private void PopulateModelWithMethodsConstructorsPropertiesAndFields(RepositoryModel repositoryModel,
            HoneydewModels.Reference.RepositoryModel referenceRepositoryModel)
        {
            for (var projectIndex = 0; projectIndex < referenceRepositoryModel.Projects.Count; projectIndex++)
            {
                var projectModel = referenceRepositoryModel.Projects[projectIndex];
                for (var compilationUnitIndex = 0;
                     compilationUnitIndex < projectModel.Files.Count;
                     compilationUnitIndex++)
                {
                    var compilationUnit = projectModel.Files[compilationUnitIndex];
                    var compilationUnitType = repositoryModel.Projects[projectIndex]
                        .CompilationUnits[compilationUnitIndex];

                    foreach (var classModel in compilationUnit.Classes)
                    {
                        var classType =
                            compilationUnitType.ClassTypes.FirstOrDefault(c => c.Name == classModel.Name);

                        if (classType is not IMembersClassType membersClassType)
                        {
                            continue;
                        }

                        classModel.Methods.AddRange(PopulateWithMethodModels(classModel, membersClassType.Methods)
                            .Concat(PopulateWithConstructorModels(classModel, membersClassType.Constructors))
                            .Concat(PopulateWithDestructorModel(classModel, membersClassType.Destructor))
                            .ToList());
                        classModel.Fields.AddRange(PopulateWithFieldModels(classModel, membersClassType.Fields));

                        if (classType is not IPropertyMembersClassType propertyMembersClassType)
                        {
                            continue;
                        }

                        classModel.Fields = classModel.Fields
                            .Concat(PopulateWithPropertyModels(classModel, propertyMembersClassType.Properties))
                            .ToList();
                    }

                    IEnumerable<MethodModel> PopulateWithMethodModels(ClassModel parentClass,
                        IEnumerable<IMethodType> methodModels)
                    {
                        return methodModels.Select(methodType => ConvertMethod(parentClass, methodType, projectModel))
                            .ToList();
                    }

                    IEnumerable<MethodModel> PopulateWithConstructorModels(ClassModel parentClass,
                        IEnumerable<IConstructorType> constructorTypes)
                    {
                        return constructorTypes
                            .Select(constructorType => ConvertConstructor(parentClass, constructorType, projectModel))
                            .ToList();
                    }

                    IEnumerable<MethodModel> PopulateWithDestructorModel(ClassModel parentClass,
                        IDestructorType destructorType)
                    {
                        if (destructorType == null)
                        {
                            return new List<MethodModel>();
                        }

                        return new List<MethodModel>
                        {
                            ConvertDestructor(parentClass, destructorType, projectModel)
                        };
                    }

                    IList<FieldModel> PopulateWithFieldModels(ClassModel classModel,
                        IEnumerable<IFieldType> fields)
                    {
                        return fields.Select(fieldType => ConvertField(classModel, fieldType, projectModel)).ToList();
                    }

                    IEnumerable<PropertyModel> PopulateWithPropertyModels(ClassModel classModel,
                        IEnumerable<IPropertyType> properties)
                    {
                        return properties
                            .Select(propertyType => ConvertProperty(classModel, propertyType, projectModel))
                            .ToList();
                    }
                }
            }
        }

        private PropertyModel ConvertProperty(ClassModel classModel, IPropertyType propertyType,
            ProjectModel projectModel)
        {
            var model = new PropertyModel
            {
                Class = classModel,
                Name = propertyType.Name,
                Modifier = propertyType.Modifier,
                AccessModifier = propertyType.AccessModifier,
                IsEvent = propertyType.IsEvent,
                Metrics = ConvertMetrics(propertyType),
                Loc = ConvertLoc(propertyType.Loc),
                CyclomaticComplexity = propertyType.CyclomaticComplexity,
                Type = ConvertEntityType(propertyType.Type, projectModel),
                IsNullable = propertyType.IsNullable,
            };
            model.Attributes = ConvertAttributes(model, propertyType.Attributes, projectModel);
            model.Accessors = propertyType.Accessors.Select(accessor => ConvertMethod(model, accessor, projectModel))
                .ToList();

            return model;
        }

        private FieldModel ConvertField(ClassModel classModel, IFieldType fieldType, ProjectModel projectModel)
        {
            var model = new FieldModel
            {
                Class = classModel,
                Name = fieldType.Name,
                Modifier = fieldType.Modifier,
                AccessModifier = fieldType.AccessModifier,
                IsEvent = fieldType.IsEvent,
                Metrics = ConvertMetrics(fieldType),
                Type = ConvertEntityType(fieldType.Type, projectModel),
                IsNullable = fieldType.IsNullable,
            };
            model.Attributes = ConvertAttributes(model, fieldType.Attributes, projectModel);

            return model;
        }

        private MethodModel ConvertConstructor(ClassModel parentClass, IConstructorType constructorType,
            ProjectModel projectModel)
        {
            var model = new MethodModel
            {
                Class = parentClass,
                ContainingType = parentClass,
                MethodType = nameof(MethodType.Constructor),
                Name = constructorType.Name,
                Modifier = constructorType.Modifier,
                AccessModifier = constructorType.AccessModifier,
                Loc = ConvertLoc(constructorType.Loc),
                CyclomaticComplexity = constructorType.CyclomaticComplexity,
                Metrics = ConvertMetrics(constructorType),
            };

            model.Attributes = ConvertAttributes(model, constructorType.Attributes, projectModel);
            model.Parameters = ConvertParameters(model, constructorType.ParameterTypes, projectModel);
            model.LocalVariables = ConvertLocalVariables(model, constructorType.LocalVariableTypes, projectModel);

            model.ReturnValue = ConvertReturnValue(model, new HoneydewModels.CSharp.ReturnValueModel
            {
                Type = new EntityTypeModel
                {
                    Name = "void",
                    FullType = new HoneydewModels.Types.GenericType
                    {
                        Name = "void",
                        ContainedTypes = new List<HoneydewModels.Types.GenericType>()
                    }
                }
            }, projectModel);
            if (constructorType is ITypeWithLocalFunctions typeWithLocalFunctions)
            {
                model.LocalFunctions =
                    ConvertLocalFunctions(model, typeWithLocalFunctions.LocalFunctions, projectModel);
            }

            return model;
        }

        private MethodModel ConvertDestructor(ClassModel parentClass, IDestructorType destructorType,
            ProjectModel projectModel)
        {
            var model = new MethodModel
            {
                Class = parentClass,
                ContainingType = parentClass,
                MethodType = nameof(MethodType.Destructor),
                Name = destructorType.Name,
                Modifier = destructorType.Modifier,
                AccessModifier = destructorType.AccessModifier,
                Loc = ConvertLoc(destructorType.Loc),
                CyclomaticComplexity = destructorType.CyclomaticComplexity,
                Metrics = ConvertMetrics(destructorType),
            };

            model.Attributes = ConvertAttributes(model, destructorType.Attributes, projectModel);
            model.Parameters = ConvertParameters(model, destructorType.ParameterTypes, projectModel);
            model.LocalVariables = ConvertLocalVariables(model, destructorType.LocalVariableTypes, projectModel);

            model.ReturnValue = ConvertReturnValue(model, new HoneydewModels.CSharp.ReturnValueModel
            {
                Type = new EntityTypeModel
                {
                    Name = "void",
                    FullType = new HoneydewModels.Types.GenericType
                    {
                        Name = "void",
                        ContainedTypes = new List<HoneydewModels.Types.GenericType>()
                    }
                }
            }, projectModel);

            if (destructorType is ITypeWithLocalFunctions typeWithLocalFunctions)
            {
                model.LocalFunctions =
                    ConvertLocalFunctions(model, typeWithLocalFunctions.LocalFunctions, projectModel);
            }

            return model;
        }

        private MethodModel ConvertMethod(ReferenceEntity parentModel, IMethodType methodType,
            ProjectModel projectModel)
        {
            var model = new MethodModel
            {
                ContainingType = parentModel,
                Class = GetClass(parentModel),
                Name = methodType.Name,
                MethodType = nameof(MethodType.Method),
                Loc = ConvertLoc(methodType.Loc),
                Modifier = methodType.Modifier,
                AccessModifier = methodType.AccessModifier,
                GenericParameters = ConvertGenericParameters(methodType.GenericParameters, projectModel),
                CyclomaticComplexity = methodType.CyclomaticComplexity,
                Metrics = ConvertMetrics(methodType),
            };

            model.Attributes = ConvertAttributes(model, methodType.Attributes, projectModel);
            model.ReturnValue = ConvertReturnValue(model, methodType.ReturnValue, projectModel);
            model.Parameters = ConvertParameters(model, methodType.ParameterTypes, projectModel);
            model.LocalVariables = ConvertLocalVariables(model, methodType.LocalVariableTypes, projectModel);

            if (model.Parameters.Count > 0 && model.Parameters[0].Modifier == "this")
            {
                model.MethodType = nameof(MethodType.Extension);
            }

            if (methodType is ITypeWithLocalFunctions typeWithLocalFunctions)
            {
                model.LocalFunctions =
                    ConvertLocalFunctions(model, typeWithLocalFunctions.LocalFunctions, projectModel);
            }

            return model;
        }

        private ClassModel GetClass(ReferenceEntity parentModel)
        {
            return parentModel switch
            {
                ClassModel classModel => classModel,
                MethodModel methodModel => GetClass(methodModel.ContainingType),
                null or _ => null,
            };
        }

        private IList<MethodModel> ConvertLocalFunctions(ReferenceEntity parentModel,
            IEnumerable<IMethodTypeWithLocalFunctions> localFunctions, ProjectModel projectModel)
        {
            return localFunctions.Select(localFunction =>
                {
                    var localFunctionMethod = ConvertMethod(parentModel, localFunction, projectModel);
                    localFunctionMethod.MethodType = nameof(MethodType.LocalFunction);
                    return localFunctionMethod;
                })
                .ToList();
        }

        private IList<LocalVariableModel> ConvertLocalVariables(ReferenceEntity parentModel,
            IEnumerable<ILocalVariableType> localVariableTypes, ProjectModel projectModel)
        {
            var localVariables = new List<LocalVariableModel>();

            foreach (var localVariableType in localVariableTypes)
            {
                localVariables.Add(new LocalVariableModel
                {
                    Type = ConvertEntityType(localVariableType.Type, projectModel),
                    ContainingType = parentModel,
                    IsNullable = localVariableType.IsNullable,
                });
            }

            return localVariables;
        }

        private static List<MetricModel> ConvertMetrics(ITypeWithMetrics typeWithMetrics)
        {
            return typeWithMetrics.Metrics.Select(metric => new MetricModel
            {
                ExtractorName = metric.ExtractorName,
                Value = metric.Value,
                ValueType = metric.ValueType
            }).ToList();
        }

        private void PopulateModelWithMethodReferences(RepositoryModel repositoryModel,
            HoneydewModels.Reference.RepositoryModel referenceRepositoryModel)
        {
            for (var projectIndex = 0; projectIndex < referenceRepositoryModel.Projects.Count; projectIndex++)
            {
                var projectModel = referenceRepositoryModel.Projects[projectIndex];
                for (var compilationUnitIndex = 0;
                     compilationUnitIndex < projectModel.Files.Count;
                     compilationUnitIndex++)
                {
                    var compilationUnit = projectModel.Files[compilationUnitIndex];
                    var compilationUnitType =
                        repositoryModel.Projects[projectIndex].CompilationUnits[compilationUnitIndex];

                    foreach (var classModel in compilationUnit.Classes)
                    {
                        var classType =
                            compilationUnitType.ClassTypes.FirstOrDefault(c => c.Name == classModel.Name);

                        if (classType is not IMembersClassType membersClassType)
                        {
                            continue;
                        }

                        for (var methodIndex = 0; methodIndex < membersClassType.Methods.Count; methodIndex++)
                        {
                            var methodModel = classModel.Methods[methodIndex];
                            var methodType = membersClassType.Methods[methodIndex];

                            methodModel.CalledMethods = ConvertCalledMethods(methodType.CalledMethods);

                            methodModel.AccessedFields = ConvertAccessFields(methodType.AccessedFields);

                            SetCalledMethodsForMethodLocalFunctions(methodModel, methodType);
                        }

                        for (var constructorIndex = 0;
                             constructorIndex < membersClassType.Constructors.Count;
                             constructorIndex++)
                        {
                            var constructorModel = classModel.Constructors[constructorIndex];
                            var constructorType = membersClassType.Constructors[constructorIndex];

                            constructorModel.CalledMethods =
                                ConvertCalledMethods(constructorType.CalledMethods);

                            SetCalledMethodsForConstructorLocalFunctions(constructorModel, constructorType);
                        }

                        if (classType is not IPropertyMembersClassType propertyMembersClassType)
                        {
                            continue;
                        }

                        for (var propertyIndex = 0; propertyIndex < classModel.Properties.Count; propertyIndex++)
                        {
                            var propertyModel = classModel.Properties[propertyIndex];
                            var propertyType = propertyMembersClassType.Properties[propertyIndex];
                            for (var accessorIndex = 0; accessorIndex < propertyModel.Accessors.Count; accessorIndex++)
                            {
                                var accessor = propertyModel.Accessors[accessorIndex];
                                var accessorType = propertyType.Accessors[accessorIndex];

                                accessor.CalledMethods = ConvertCalledMethods(accessorType.CalledMethods);

                                accessor.AccessedFields = ConvertAccessFields(accessorType.AccessedFields);

                                SetCalledMethodsForMethodLocalFunctions(accessor, accessorType);
                            }
                        }
                    }

                    IList<MethodModel> ConvertCalledMethods(IEnumerable<IMethodSignatureType> calledMethods)
                    {
                        var calledMethodModels = new List<MethodModel>();

                        foreach (var calledMethod in calledMethods)
                        {
                            calledMethodModels.Add(GetMethodReference(calledMethod, projectModel));
                        }

                        return calledMethodModels;
                    }

                    IList<AccessedField> ConvertAccessFields(
                        IEnumerable<HoneydewModels.Types.AccessedField> accessedFields)
                    {
                        return accessedFields.Select(field =>
                        {
                            if (SearchEntityByName(field.ContainingTypeName, projectModel) is not ClassModel
                                containingClass)
                            {
                                return null;
                            }

                            var fieldReference = containingClass.Fields.FirstOrDefault(f => f.Name == field.Name);
                            if (fieldReference != null)
                            {
                                return new AccessedField
                                {
                                    Field = fieldReference,
                                    AccessType = nameof(field.Kind),
                                };
                            }

                            var propertyReference =
                                containingClass.Properties.FirstOrDefault(p => p.Name == field.Name);
                            if (propertyReference != null)
                            {
                                return new AccessedField
                                {
                                    Field = propertyReference,
                                    AccessType = nameof(field.Kind),
                                };
                            }

                            return null;
                        }).Where(x => x != null).ToList();
                    }

                    void SetCalledMethodsForMethodLocalFunctions(MethodModel methodModel, IMethodType methodType)
                    {
                        methodModel.CalledMethods = ConvertCalledMethods(methodType.CalledMethods);

                        for (var localFunctionIndex = 0;
                             localFunctionIndex < methodModel.LocalFunctions.Count;
                             localFunctionIndex++)
                        {
                            if (methodType is not ITypeWithLocalFunctions typeWithLocalFunctions)
                            {
                                continue;
                            }

                            var localFunction = methodModel.LocalFunctions[localFunctionIndex];
                            var localFunctionType = typeWithLocalFunctions.LocalFunctions[localFunctionIndex];

                            localFunction.CalledMethods = ConvertCalledMethods(localFunctionType.CalledMethods);

                            for (var i = 0; i < localFunction.LocalFunctions.Count; i++)
                            {
                                SetCalledMethodsForMethodLocalFunctions(localFunction.LocalFunctions[i],
                                    localFunctionType.LocalFunctions[i]);
                            }
                        }
                    }

                    void SetCalledMethodsForConstructorLocalFunctions(MethodModel constructorModel,
                        IConstructorType constructorType)
                    {
                        constructorModel.CalledMethods = ConvertCalledMethods(constructorType.CalledMethods);


                        for (var localFunctionIndex = 0;
                             localFunctionIndex < constructorModel.LocalFunctions.Count;
                             localFunctionIndex++)
                        {
                            if (constructorType is not ITypeWithLocalFunctions typeWithLocalFunctions)
                            {
                                continue;
                            }

                            var localFunction = constructorModel.LocalFunctions[localFunctionIndex];
                            var localFunctionType = typeWithLocalFunctions.LocalFunctions[localFunctionIndex];

                            localFunction.CalledMethods = ConvertCalledMethods(localFunctionType.CalledMethods);

                            for (int i = 0; i < localFunction.LocalFunctions.Count; i++)
                            {
                                SetCalledMethodsForMethodLocalFunctions(localFunction.LocalFunctions[i],
                                    localFunctionType.LocalFunctions[i]);
                            }
                        }
                    }
                }
            }
        }


        private MethodModel GetMethodReference(IMethodSignatureType methodSignatureType, ProjectModel projectModel)
        {
            var classModel =
                SearchEntityByName(methodSignatureType.ContainingTypeName, projectModel, false) as ClassModel;

            if (classModel != null)
            {
                foreach (var methodModel in classModel.Methods)
                {
                    if (methodModel.Name != methodSignatureType.Name)
                    {
                        continue;
                    }

                    if (methodModel.Parameters.Count != methodSignatureType.ParameterTypes.Count)
                    {
                        continue;
                    }

                    var allParametersMatch = true;
                    for (var i = 0; i < methodModel.Parameters.Count; i++)
                    {
                        var parameterModel = methodModel.Parameters[i];
                        var parameterType = methodSignatureType.ParameterTypes[i];

                        if (parameterModel.IsNullable && !parameterType.IsNullable ||
                            !parameterModel.IsNullable && parameterType.IsNullable)
                        {
                            continue;
                        }

                        if (parameterModel.Type.Name != parameterType.Type.Name)
                        {
                            allParametersMatch = false;
                            break;
                        }
                    }

                    if (allParametersMatch)
                    {
                        return methodModel;
                    }
                }
            }
            else
            {
                try
                {
                    var entity = GetMethodReferenceFromLocalFunction(methodSignatureType, projectModel);
                    if (entity != null)
                    {
                        return entity;
                    }
                }
                catch
                {
                    // ignored
                }
            }

            var createdMethodModel = new MethodModel
            {
                ContainingType = classModel,
                Class = classModel,
                Name = methodSignatureType.Name,
                Parameters = methodSignatureType.ParameterTypes.Select(p =>
                {
                    var param = p as HoneydewModels.CSharp.ParameterModel;
                    var parameterModel = new ParameterModel
                    {
                        Type = ConvertEntityType(p.Type, projectModel),
                        IsNullable = p.IsNullable,
                    };
                    parameterModel.Attributes = ConvertAttributes(parameterModel, p.Attributes, projectModel);
                    if (param != null)
                    {
                        parameterModel.Modifier = param.Modifier;
                        parameterModel.DefaultValue = param.DefaultValue;
                    }

                    return parameterModel;
                }).ToList()
            };
            foreach (var parameter in createdMethodModel.Parameters)
            {
                parameter.ContainingType = createdMethodModel;
            }

            classModel?.Methods.Add(createdMethodModel);

            return createdMethodModel;
        }

        private MethodModel GetMethodReferenceFromLocalFunction(IMethodSignatureType methodSignatureType,
            ProjectModel projectModel)
        {
            var containingTypeName = methodSignatureType.ContainingTypeName;
            var indexOfMethodParenthesis = containingTypeName.IndexOf('(');
            ClassModel classModel;
            if (indexOfMethodParenthesis < 0)
            {
                var indexOfAccessor = containingTypeName.LastIndexOf('.');
                var indexOfPropertyName = containingTypeName.LastIndexOf('.', indexOfAccessor - 1);
                var propertyName = containingTypeName[(indexOfPropertyName + 1)..indexOfAccessor];
                var accessorName = containingTypeName[(indexOfAccessor + 1)..];
                var classNameWithProperty = containingTypeName[..indexOfPropertyName];

                classModel = SearchEntityByName(classNameWithProperty, projectModel, false) as ClassModel;

                if (classModel == null)
                {
                    return null;
                }

                var propertyModel = classModel.Properties.FirstOrDefault(p => p.Name == propertyName);
                var accessorModel = propertyModel?.Accessors.FirstOrDefault(a => a.Name == accessorName);

                return accessorModel?.LocalFunctions.FirstOrDefault(localFunction =>
                    localFunction.Name == methodSignatureType.Name);
            }

            var className = containingTypeName[..indexOfMethodParenthesis];
            var indexOfMethodName = className.LastIndexOf('.');
            className = className[..indexOfMethodName];

            classModel = SearchEntityByName(className, projectModel, false) as ClassModel;
            MethodModel methodReference = null;

            string localFunctionChain;

            try
            {
                localFunctionChain = containingTypeName[(containingTypeName.IndexOf(')', indexOfMethodName) + 1)..];
            }
            catch
            {
                localFunctionChain = "";
            }

            if (classModel == null)
            {
                var indexOfAccessor = className.LastIndexOf('.');
                var indexOfPropertyName = className.LastIndexOf('.', indexOfAccessor - 1);
                var propertyName = className[(indexOfPropertyName + 1)..indexOfAccessor];
                var accessorName = className[(indexOfAccessor + 1)..];
                className = className[..indexOfPropertyName];

                classModel = SearchEntityByName(className, projectModel, false) as ClassModel;

                if (classModel == null)
                {
                    return null;
                }

                var propertyModel = classModel.Properties.FirstOrDefault(p => p.Name == propertyName);
                if (propertyModel == null)
                {
                    return null;
                }

                methodReference = propertyModel.Accessors.FirstOrDefault(a => a.Name == accessorName);
                if (methodReference == null)
                {
                    return null;
                }

                localFunctionChain = containingTypeName[(indexOfAccessor + accessorName.Length + 1)..];
            }

            MethodModel localFunctionReference = null;


            var localFunctionName = string.IsNullOrEmpty(localFunctionChain)
                ? ""
                : localFunctionChain[1..localFunctionChain.IndexOf('(')];

            if (methodReference == null)
            {
                var methodSignatureName = containingTypeName[(indexOfMethodName + 1)..];
                var indexOfParenthesis = methodSignatureName.IndexOf('(');
                var methodName = methodSignatureName[..indexOfParenthesis];
                var indexOfMethodNameEndParenthesis = methodSignatureName.IndexOf(')');
                var parameters = methodSignatureName[(indexOfParenthesis + 1)..indexOfMethodNameEndParenthesis]
                    .Split(',').ToList();
                if (parameters.Count == 1 && parameters[0] == "")
                {
                    parameters.Clear();
                }

                methodReference = GetMethodReferenceFromClass(classModel, methodName, parameters);
                if (methodReference == null)
                {
                    var constructorReference = GetConstructorReferenceFromClass(classModel, methodName, parameters);

                    if (constructorReference == null)
                    {
                        return null;
                    }

                    if (string.IsNullOrEmpty(localFunctionChain))
                    {
                        return constructorReference.LocalFunctions.FirstOrDefault(localFunction =>
                            localFunction.Name == methodSignatureType.Name);
                    }

                    localFunctionReference = constructorReference.LocalFunctions.FirstOrDefault(localFunction =>
                        localFunction.Name == localFunctionName);
                }
                else
                {
                    if (string.IsNullOrEmpty(localFunctionChain))
                    {
                        return methodReference.LocalFunctions.FirstOrDefault(localFunction =>
                            localFunction.Name == methodSignatureType.Name);
                    }
                }
            }

            if (localFunctionReference == null)
            {
                localFunctionReference = methodReference?.LocalFunctions.FirstOrDefault(localFunction =>
                    localFunction.Name == localFunctionName);
            }

            if (localFunctionReference == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(localFunctionChain))
            {
                return localFunctionReference.LocalFunctions.FirstOrDefault(localFunction =>
                    localFunction.Name == methodSignatureType.Name);
            }

            localFunctionChain = localFunctionChain[(localFunctionChain.IndexOf(')') + 1)..];
            // remove first local function because it was already processed

            while (!string.IsNullOrEmpty(localFunctionChain))
            {
                var name = localFunctionChain[1..localFunctionChain.IndexOf('(')];
                localFunctionReference =
                    localFunctionReference.LocalFunctions.FirstOrDefault(localFunction => localFunction.Name == name);
                if (localFunctionReference == null)
                {
                    return null;
                }

                localFunctionChain = localFunctionChain[(localFunctionChain.IndexOf(')') + 1)..];
            }

            return localFunctionReference.LocalFunctions.FirstOrDefault(localFunction =>
                localFunction.Name == methodSignatureType.Name);
        }

        private MethodModel GetMethodReferenceFromClass(ClassModel classModel, string methodName,
            IReadOnlyList<string> parameters)
        {
            MethodModel reference = null;

            foreach (var methodModel in classModel.Methods)
            {
                if (methodName != methodModel.Name)
                {
                    continue;
                }

                if (methodModel.Parameters.Count != parameters.Count)
                {
                    continue;
                }

                var allParametersMatch = true;
                for (var i = 0; i < methodModel.Parameters.Count; i++)
                {
                    var parameterModel = methodModel.Parameters[i];
                    var parameterType = parameters[i];

                    if (parameterModel.IsNullable && parameterType.EndsWith('?'))
                    {
                        parameterType = parameterType[..^1];
                        if (parameterModel.Type.Name != parameterType)
                        {
                            allParametersMatch = false;
                            break;
                        }
                    }

                    if (!(!parameterModel.IsNullable && !parameterType.EndsWith('?')))
                    {
                        allParametersMatch = false;
                        break;
                    }

                    if (parameterModel.Type.Name != parameterType)
                    {
                        allParametersMatch = false;
                        break;
                    }
                }

                if (allParametersMatch)
                {
                    reference = methodModel;
                    break;
                }
            }

            return reference;
        }

        private MethodModel GetConstructorReferenceFromClass(ClassModel classModel, string constructorName,
            IReadOnlyList<string> parameters)
        {
            MethodModel reference = null;

            foreach (var constructorModel in classModel.Constructors)
            {
                if (constructorName != constructorModel.Name)
                {
                    continue;
                }

                if (constructorModel.Parameters.Count != parameters.Count)
                {
                    continue;
                }

                var allParametersMatch = true;
                for (var i = 0; i < constructorModel.Parameters.Count; i++)
                {
                    var parameterModel = constructorModel.Parameters[i];
                    var parameterType = parameters[i];

                    if (parameterModel.IsNullable && parameterType.EndsWith('?'))
                    {
                        parameterType = parameterType[..^1];
                        if (parameterModel.Type.Name != parameterType)
                        {
                            allParametersMatch = false;
                            break;
                        }
                    }

                    if (!(!parameterModel.IsNullable && !parameterType.EndsWith('?')))
                    {
                        allParametersMatch = false;
                        break;
                    }

                    if (parameterModel.Type.Name != parameterType)
                    {
                        allParametersMatch = false;
                        break;
                    }
                }

                if (allParametersMatch)
                {
                    reference = constructorModel;
                    break;
                }
            }

            return reference;
        }

        private ReturnValueModel ConvertReturnValue(ReferenceEntity parentModel, IReturnValueType returnValueType,
            ProjectModel projectModel)
        {
            var returnValueModel = new ReturnValueModel
            {
                ContainingType = parentModel,
                Type = ConvertEntityType(returnValueType.Type, projectModel),
                IsNullable = returnValueType.IsNullable,
            };

            returnValueModel.Attributes = ConvertAttributes(returnValueModel, returnValueType.Attributes, projectModel);

            if (returnValueType is HoneydewModels.CSharp.ReturnValueModel returnValue)
            {
                returnValueModel.Modifier = returnValue.Modifier;
            }

            return returnValueModel;
        }

        private IList<AttributeModel> ConvertAttributes(ReferenceEntity parentModel,
            IEnumerable<IAttributeType> attributeTypes, ProjectModel projectModel)
        {
            var attributes = new List<AttributeModel>();

            foreach (var attributeType in attributeTypes)
            {
                var attributeModel = new AttributeModel
                {
                    Name = attributeType.Name,
                    Target = attributeType.Target,
                    ContainingType = parentModel,
                };

                attributeModel.Parameters =
                    ConvertParameters(attributeModel, attributeType.ParameterTypes, projectModel);

                attributes.Add(attributeModel);
            }

            return attributes;
        }

        private IList<ParameterModel> ConvertParameters(ReferenceEntity parentModel,
            IEnumerable<IParameterType> parameterTypes, ProjectModel projectModel)
        {
            var parameters = new List<ParameterModel>();

            foreach (var parameterType in parameterTypes)
            {
                var parameterModel = new ParameterModel
                {
                    ContainingType = parentModel,
                    Type = ConvertEntityType(parameterType.Type, projectModel),
                    IsNullable = parameterType.IsNullable,
                };
                parameterModel.Attributes = ConvertAttributes(parameterModel, parameterType.Attributes, projectModel);

                if (parameterType is HoneydewModels.CSharp.ParameterModel param)
                {
                    parameterModel.Modifier = param.Modifier;
                    parameterModel.DefaultValue = param.DefaultValue;
                }

                parameters.Add(parameterModel);
            }

            return parameters;
        }

        private IList<GenericParameterModel> ConvertGenericParameters(
            IEnumerable<IGenericParameterType> genericParameters, ProjectModel projectModel)
        {
            var parameters = new List<GenericParameterModel>();

            foreach (var parameterType in genericParameters)
            {
                var parameterModel = new GenericParameterModel
                {
                    Name = parameterType.Name,
                    Modifier = parameterType.Modifier,
                    Constraints = parameterType.Constraints.Select(c => ConvertEntityType(c, projectModel)).ToList(),
                };

                parameterModel.Attributes = ConvertAttributes(parameterModel, parameterType.Attributes, projectModel);

                parameters.Add(parameterModel);
            }

            return parameters;
        }

        private static LinesOfCode ConvertLoc(HoneydewModels.LinesOfCode linesOfCode)
        {
            return new LinesOfCode
            {
                SourceLines = linesOfCode.SourceLines,
                CommentedLines = linesOfCode.CommentedLines,
                EmptyLines = linesOfCode.EmptyLines
            };
        }

        private EntityType ConvertEntityType(string name, ReferenceEntity referenceEntity,
            ProjectModel projectModel = null)
        {
            var entityTypeModel = _fullTypeNameBuilder.CreateEntityTypeModel(name);

            var entityType = new EntityType
            {
                TypeReference = referenceEntity,
                IsExtern = entityTypeModel.IsExtern,
                FullType = ConvertGeneric(entityTypeModel.FullType, projectModel),
                Name = entityTypeModel.Name,
            };

            return entityType;
        }

        private EntityType ConvertEntityType(IEntityType type, ProjectModel projectModel)
        {
            var referenceEntity = SearchEntityByName(type.Name, projectModel);

            var entityType = new EntityType
            {
                TypeReference = referenceEntity,
                IsExtern = type.IsExtern,
                FullType = ConvertGeneric(type.FullType, projectModel),
                Name = type.Name,
            };

            return entityType;
        }

        private GenericType ConvertGeneric(HoneydewModels.Types.GenericType genType, ProjectModel projectModel)
        {
            if (genType == null)
            {
                return new GenericType();
            }

            var genericType = new GenericType
            {
                Reference = projectModel != null
                    ? SearchEntityByName(GetNonNullableName(genType.Name), projectModel)
                    : null,
                IsNullable = genType.IsNullable,
            };

            if (projectModel != null)
            {
                foreach (var containedType in genType.ContainedTypes)
                {
                    genericType.ContainedTypes.Add(ConvertGeneric(containedType, projectModel));
                }
            }

            return genericType;
        }

        private static string GetNonNullableName(string name)
        {
            return name.EndsWith('?') ? name[..^1] : name;
        }
    }
}

using System.Collections.Concurrent;
using HoneydewCore.Processors;
using HoneydewCore.Utils;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using HoneydewScriptBeePlugin.Models;
using static HoneydewScriptBeePlugin.Loaders.MetricAdder;
using AttributeModel = HoneydewScriptBeePlugin.Models.AttributeModel;
using ClassModel = HoneydewScriptBeePlugin.Models.ClassModel;
using DelegateModel = HoneydewModels.CSharp.DelegateModel;
using FieldModel = HoneydewScriptBeePlugin.Models.FieldModel;
using GenericParameterModel = HoneydewScriptBeePlugin.Models.GenericParameterModel;
using LocalVariableModel = HoneydewScriptBeePlugin.Models.LocalVariableModel;
using MethodModel = HoneydewScriptBeePlugin.Models.MethodModel;
using NamespaceModel = HoneydewScriptBeePlugin.Models.NamespaceModel;
using ParameterModel = HoneydewScriptBeePlugin.Models.ParameterModel;
using ProjectModel = HoneydewScriptBeePlugin.Models.ProjectModel;
using PropertyModel = HoneydewScriptBeePlugin.Models.PropertyModel;
using RepositoryModel = HoneydewModels.CSharp.RepositoryModel;
using ReturnValueModel = HoneydewScriptBeePlugin.Models.ReturnValueModel;
using SolutionModel = HoneydewScriptBeePlugin.Models.SolutionModel;

namespace HoneydewScriptBeePlugin.Loaders;

public class RepositoryModelToReferenceRepositoryModelProcessor : IProcessorFunction<RepositoryModel,
    Models.RepositoryModel>
{
    private readonly IDictionary<(string className, int genericParameterCount), ClassModel> _generatedTypes =
        new ConcurrentDictionary<( string className, int genericParameterCount), ClassModel>();

    private readonly IDictionary<(ProjectModel, string className, int genericParameterCount), IList<EntityModel>>
        _entityModels =
            new ConcurrentDictionary<(ProjectModel, string className, int genericParameterCount), IList<EntityModel>>();

    private readonly FullTypeNameBuilder _fullTypeNameBuilder = new();

    public Models.RepositoryModel Process(RepositoryModel? inputRepositoryModel)
    {
        if (inputRepositoryModel == null)
        {
            return new Models.RepositoryModel();
        }

        var repositoryModel = new Models.RepositoryModel
        {
            Version = inputRepositoryModel.Version
        };

        PopulateModelWithSolutionProjectNamespacesCompilationUnitsAndClasses(inputRepositoryModel, repositoryModel);

        PopulateProjectWithProjectReferences(inputRepositoryModel, repositoryModel);

        PopulateWithEntityReferences(inputRepositoryModel, repositoryModel);

        PopulateModelWithMethodsConstructorsPropertiesAndFields(inputRepositoryModel, repositoryModel);

        PopulateModelWithMethodReferencesAndFieldAccesses(inputRepositoryModel, repositoryModel);

        repositoryModel.CreatedClasses = _generatedTypes.Select(pair => pair.Value).ToList();

        return repositoryModel;
    }

    #region Solutions Project Namespaces CompilationUnits Classes

    private void PopulateModelWithSolutionProjectNamespacesCompilationUnitsAndClasses(RepositoryModel repositoryModel,
        Models.RepositoryModel referenceRepositoryModel)
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
                var fileModel = new FileModel
                {
                    FilePath = compilationUnitType.FilePath,
                    Project = referenceProjectModel,
                    Loc = ConvertLoc(compilationUnitType.Loc),
                };
                AddMetrics(fileModel, compilationUnitType);

                referenceProjectModel.Files.Add(fileModel);

                foreach (var classType in compilationUnitType.ClassTypes)
                {
                    var namespaceModel = namespaceTreeHandler.GetOrAdd(classType.ContainingNamespaceName);
                    presentNamespacesSet.Add(classType.ContainingNamespaceName);

                    switch (classType)
                    {
                        case DelegateModel delegateModel:
                        {
                            var model = new Models.DelegateModel
                            {
                                Name = delegateModel.Name,
                                FilePath = delegateModel.FilePath,
                                Modifier = delegateModel.Modifier,
                                AccessModifier = ConvertAccessModifier(delegateModel.AccessModifier),
                                Modifiers = ConvertModifierToModifierList(delegateModel.Modifier),
                                File = fileModel,
                                Namespace = namespaceModel,
                                IsInternal = true,
                                IsExternal = false,
                                IsPrimitive = false,
                                LinesOfCode = ConvertLoc(delegateModel.Loc),
                            };

                            namespaceModel.Entities.Add(model);
                            fileModel.Entities.Add(model);

                            _entityModels.Add((referenceProjectModel, delegateModel.Name, 0), new List<EntityModel>
                            {
                                model
                            });
                        }
                            break;

                        case HoneydewModels.CSharp.ClassModel classModel:
                        {
                            EntityModel entityModel = classModel.ClassType switch
                            {
                                "interface" => new InterfaceModel
                                {
                                    Name = classModel.Name,
                                    FilePath = classModel.FilePath,
                                    Modifier = classModel.Modifier,
                                    AccessModifier = ConvertAccessModifier(classModel.AccessModifier),
                                    Modifiers = ConvertModifierToModifierList(classModel.Modifier),
                                    File = fileModel,
                                    Namespace = namespaceModel,
                                    IsInternal = true,
                                    IsExternal = false,
                                    IsPrimitive = false,
                                    LinesOfCode = ConvertLoc(classModel.Loc),
                                },
                                "enum" => new EnumModel
                                {
                                    Name = classModel.Name,
                                    FilePath = classModel.FilePath,
                                    Modifier = classModel.Modifier,
                                    AccessModifier = ConvertAccessModifier(classModel.AccessModifier),
                                    Modifiers = ConvertModifierToModifierList(classModel.Modifier),
                                    File = fileModel,
                                    Namespace = namespaceModel,
                                    IsInternal = true,
                                    IsExternal = false,
                                    IsPrimitive = false,
                                    LinesOfCode = ConvertLoc(classModel.Loc),
                                    // todo Labels 
                                    // todo Type
                                },
                                _ => new ClassModel
                                {
                                    Name = classModel.Name,
                                    FilePath = classModel.FilePath,
                                    Type = ConvertClassType(classModel.ClassType),
                                    Modifier = classModel.Modifier,
                                    AccessModifier = ConvertAccessModifier(classModel.AccessModifier),
                                    Modifiers = ConvertModifierToModifierList(classModel.Modifier),
                                    File = fileModel,
                                    Namespace = namespaceModel,
                                    IsInternal = true,
                                    IsExternal = false,
                                    IsPrimitive = false,
                                    LinesOfCode = ConvertLoc(classModel.Loc),
                                }
                            };

                            AddMetrics(entityModel, classType);

                            namespaceModel.Entities.Add(entityModel);
                            fileModel.Entities.Add(entityModel);

                            if (_entityModels.TryGetValue(
                                    (referenceProjectModel, entityModel.Name, classModel.GenericParameters.Count),
                                    out var entities))
                            {
                                foreach (var entity in entities)
                                {
                                    switch (entity)
                                    {
                                        case ClassModel @class when entityModel is ClassModel entityClassModel:
                                            @class.Partials.Add(entityClassModel);
                                            entityClassModel.Partials.Add(@class);
                                            break;
                                        case InterfaceModel @interface
                                            when entityModel is InterfaceModel entityInterfaceModel:
                                            @interface.Partials.Add(entityInterfaceModel);
                                            entityInterfaceModel.Partials.Add(@interface);
                                            break;
                                    }
                                }

                                entities.Add(entityModel);
                            }
                            else
                            {
                                _entityModels.Add(
                                    (referenceProjectModel, entityModel.Name, classModel.GenericParameters.Count),
                                    new List<EntityModel>
                                    {
                                        entityModel
                                    });
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
                if (namespaceModel == null)
                {
                    continue;
                }

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


    private static LinesOfCode ConvertLoc(HoneydewModels.LinesOfCode linesOfCode)
    {
        return new LinesOfCode
        {
            SourceLines = linesOfCode.SourceLines,
            CommentedLines = linesOfCode.CommentedLines,
            EmptyLines = linesOfCode.EmptyLines
        };
    }

    private static ClassType ConvertClassType(string classType)
    {
        return Enum.TryParse<ClassType>(classType, true, out var result)
            ? result
            : ClassType.Class;
    }

    private static IList<Modifier> ConvertModifierToModifierList(string modifier)
    {
        if (string.IsNullOrWhiteSpace(modifier))
        {
            return new List<Modifier>();
        }

        return modifier.Split(' ').Select(ConvertModifier).ToList();
    }

    private static Modifier ConvertModifier(string modifier)
    {
        return Enum.TryParse<Modifier>(modifier, true, out var result)
            ? result
            : Modifier.None;
    }

    private static AccessModifier ConvertAccessModifier(string accessModifier)
    {
        return Enum.TryParse<AccessModifier>(accessModifier, true, out var result)
            ? result
            : AccessModifier.None;
    }

    #endregion

    #region Project References

    private static void PopulateProjectWithProjectReferences(RepositoryModel repositoryModel,
        Models.RepositoryModel referenceRepositoryModel)
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

    #endregion

    #region Imports, Attributes, Base Types, Generic Parameters

    private void PopulateWithEntityReferences(RepositoryModel repositoryModel,
        Models.RepositoryModel referenceRepositoryModel)
    {
        for (var projectIndex = 0; projectIndex < referenceRepositoryModel.Projects.Count; projectIndex++)
        {
            var projectModel = referenceRepositoryModel.Projects[projectIndex];
            for (var compilationUnitIndex = 0; compilationUnitIndex < projectModel.Files.Count; compilationUnitIndex++)
            {
                var file = projectModel.Files[compilationUnitIndex];
                var compilationUnitType = repositoryModel.Projects[projectIndex].CompilationUnits[compilationUnitIndex];

                file.Imports = compilationUnitType.Imports.Select(import => ConvertImportType(import, projectModel))
                    .ToList();

                for (var classTypeIndex = 0; classTypeIndex < compilationUnitType.ClassTypes.Count; classTypeIndex++)
                {
                    var entityModel = file.Entities[classTypeIndex];
                    var classType = compilationUnitType.ClassTypes[classTypeIndex];

                    entityModel.Imports = classType.Imports.Select(import => ConvertImportType(import, projectModel))
                        .ToList();
                    entityModel.Attributes = ConvertAttributes(classType.Attributes, projectModel);

                    switch (entityModel)
                    {
                        case ClassModel classModel:
                        {
                            classModel.GenericParameters =
                                ConvertGenericParameters(classType.GenericParameters, projectModel);

                            foreach (var baseType in classType.BaseTypes)
                            {
                                classModel.BaseTypes.Add(ConvertEntityType(baseType.Type, projectModel));
                            }

                            break;
                        }
                        case InterfaceModel interfaceModel:
                        {
                            interfaceModel.GenericParameters =
                                ConvertGenericParameters(classType.GenericParameters, projectModel);

                            foreach (var baseType in classType.BaseTypes)
                            {
                                interfaceModel.BaseTypes.Add(ConvertEntityType(baseType.Type, projectModel));
                            }

                            break;
                        }

                        case Models.DelegateModel delegateModel
                            when classType is DelegateModel delegateType:
                        {
                            delegateModel.Parameters =
                                ConvertParameters(delegateType.ParameterTypes, projectModel);
                            delegateModel.ReturnValue =
                                ConvertReturnValue(delegateType.ReturnValue, projectModel);
                            delegateModel.GenericParameters =
                                ConvertGenericParameters(classType.GenericParameters, projectModel);

                            break;
                        }
                    }
                }
            }
        }
    }

    private ReturnValueModel ConvertReturnValue(IReturnValueType returnValueType, ProjectModel projectModel)
    {
        return new ReturnValueModel
        {
            Type = ConvertEntityType(returnValueType.Type, projectModel),
            Attributes = ConvertAttributes(returnValueType.Attributes, projectModel),
            Modifier =
                returnValueType is HoneydewModels.CSharp.ReturnValueModel returnValue ? returnValue.Modifier : "",
        };
    }

    private ImportModel ConvertImportType(IImportType importType, ProjectModel projectModel)
    {
        var aliasType = ConvertAliasType(importType.AliasType);

        return new ImportModel
        {
            Alias = importType.Alias,
            IsStatic = importType.IsStatic,
            AliasType = aliasType,
            Entity = aliasType == AliasType.Class
                ? SearchEntityByName(importType.Name, projectModel).FirstOrDefault() ??
                  CreateClassModel(importType.Name)
                : null,
            Namespace = aliasType == AliasType.Namespace ? SearchNamespaceByName(importType.Name, projectModel) : null,
        };

        AliasType ConvertAliasType(string type)
        {
            return Enum.TryParse<AliasType>(type, true, out var result)
                ? result
                : AliasType.None;
        }
    }

    private static NamespaceModel? SearchNamespaceByName(string namespaceName, ProjectModel projectModel)
    {
        // todo check if condition is enough or should search in children too
        var namespaceModel = projectModel.Namespaces.FirstOrDefault(model => model.FullName == namespaceName);

        if (namespaceModel != null)
        {
            return namespaceModel;
        }

        return projectModel.ProjectReferences
            .SelectMany(project => project.Namespaces)
            .FirstOrDefault(model => model.FullName == namespaceName);
    }

    #endregion

    private IEnumerable<EntityModel> SearchEntityByName(string entityName, int genericParameterCount,
        ProjectModel projectModel)
    {
        if (string.IsNullOrEmpty(entityName))
        {
            return new List<EntityModel>();
        }

        if (_entityModels.TryGetValue((projectModel, entityName, genericParameterCount), out var entityModels))
        {
            return entityModels;
        }

        if (_generatedTypes.TryGetValue((entityName, genericParameterCount), out var generatedType))
        {
            return new List<EntityModel> { generatedType };
        }

        return new List<EntityModel>();
    }

    private IEnumerable<EntityModel> SearchEntityByName(string entityName, ProjectModel projectModel)
    {
        if (string.IsNullOrEmpty(entityName))
        {
            return new List<EntityModel>();
        }

        var entityTypeModel = _fullTypeNameBuilder.CreateEntityTypeModel(entityName);
        return SearchEntityByName(entityTypeModel.FullType.Name, entityTypeModel.FullType.ContainedTypes.Count,
            projectModel);
    }

    #region Methods Constructors Properties Fields

    private void PopulateModelWithMethodsConstructorsPropertiesAndFields(RepositoryModel repositoryModel,
        Models.RepositoryModel referenceRepositoryModel)
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

                foreach (var entityModel in compilationUnit.Entities)
                {
                    var classType = compilationUnitType.ClassTypes.FirstOrDefault(c => c.Name == entityModel.Name);

                    if (classType is not IMembersClassType membersClassType)
                    {
                        continue;
                    }

                    switch (entityModel)
                    {
                        case ClassModel classModel:
                            classModel.Methods = PopulateWithMethodModels(classModel, membersClassType.Methods)
                                .ToList();
                            classModel.Constructors = membersClassType.Constructors
                                .Select(constructorType =>
                                    ConvertConstructor(classModel, constructorType, projectModel))
                                .ToList();
                            classModel.Destructor =
                                ConvertDestructor(classModel, membersClassType.Destructor, projectModel);

                            classModel.Fields = membersClassType.Fields
                                .Select(fieldType => ConvertField(classModel, fieldType, projectModel))
                                .ToList();
                            break;

                        case InterfaceModel interfaceModel:
                            interfaceModel.Methods =
                                PopulateWithMethodModels(interfaceModel, membersClassType.Methods).ToList();
                            break;
                    }

                    if (classType is not IPropertyMembersClassType propertyMembersClassType)
                    {
                        continue;
                    }

                    switch (entityModel)
                    {
                        case ClassModel classModel:
                            classModel.Properties =
                                PopulateWithPropertyModels(classModel, propertyMembersClassType.Properties).ToList();
                            break;
                        case InterfaceModel interfaceModel:
                            interfaceModel.Properties =
                                PopulateWithPropertyModels(interfaceModel, propertyMembersClassType.Properties)
                                    .ToList();
                            break;
                    }
                }
            }

            IEnumerable<MethodModel> PopulateWithMethodModels(EntityModel parentEntity,
                IEnumerable<IMethodType> methodModels)
            {
                return methodModels.Select(methodType => ConvertMethod(parentEntity, null, methodType, projectModel));
            }

            IEnumerable<PropertyModel> PopulateWithPropertyModels(EntityModel entityModel,
                IEnumerable<IPropertyType> properties)
            {
                return properties.Select(propertyType => ConvertProperty(entityModel, propertyType, projectModel));
            }
        }
    }

    private PropertyModel ConvertProperty(EntityModel entityModel, IPropertyType propertyType,
        ProjectModel projectModel)
    {
        var model = new PropertyModel
        {
            Entity = entityModel,
            Name = propertyType.Name,
            Modifier = propertyType.Modifier,
            AccessModifier = ConvertAccessModifier(propertyType.AccessModifier),
            Modifiers = ConvertModifierToModifierList(propertyType.Modifier),
            IsEvent = propertyType.IsEvent,
            LinesOfCode = ConvertLoc(propertyType.Loc),
            CyclomaticComplexity = propertyType.CyclomaticComplexity,
            Type = ConvertEntityType(propertyType.Type, projectModel),
            Attributes = ConvertAttributes(propertyType.Attributes, projectModel),
            // todo accesses
        };

        AddMetrics(model, propertyType);
        model.Accessors = propertyType.Accessors
            .Select(accessor => ConvertAccessor(entityModel, model, accessor, projectModel))
            .ToList();

        return model;
    }

    private FieldModel ConvertField(ClassModel classModel, IFieldType fieldType, ProjectModel projectModel)
    {
        var model = new FieldModel
        {
            Entity = classModel,
            Name = fieldType.Name,
            Modifier = fieldType.Modifier,
            AccessModifier = ConvertAccessModifier(fieldType.AccessModifier),
            Modifiers = ConvertModifierToModifierList(fieldType.Modifier),
            IsEvent = fieldType.IsEvent,
            Type = ConvertEntityType(fieldType.Type, projectModel),
            Attributes = ConvertAttributes(fieldType.Attributes, projectModel),
            // todo accesses
        };
        AddMetrics(model, fieldType);

        return model;
    }

    private MethodModel ConvertConstructor(ClassModel parentClass, IConstructorType constructorType,
        ProjectModel projectModel)
    {
        var model = new MethodModel
        {
            Entity = parentClass,
            ContainingMethod = null,
            ContainingProperty = null,
            Type = MethodType.Constructor,
            Name = constructorType.Name,
            Modifier = constructorType.Modifier,
            AccessModifier = ConvertAccessModifier(constructorType.AccessModifier),
            Modifiers = ConvertModifierToModifierList(constructorType.Modifier),
            LinesOfCode = ConvertLoc(constructorType.Loc),
            CyclomaticComplexity = constructorType.CyclomaticComplexity,
            Attributes = ConvertAttributes(constructorType.Attributes, projectModel),
            Parameters = ConvertParameters(constructorType.ParameterTypes, projectModel),
            LocalVariables = ConvertLocalVariables(constructorType.LocalVariableTypes, projectModel),
            ReturnValue = null,
            GenericParameters = new List<GenericParameterModel>(),
        };

        AddMetrics(model, constructorType);
        model.LocalFunctions = ConvertLocalFunctions(parentClass, model, constructorType.LocalFunctions, projectModel);

        return model;
    }

    private MethodModel? ConvertDestructor(ClassModel parentClass, IDestructorType? destructorType,
        ProjectModel projectModel)
    {
        if (destructorType == null)
        {
            return null;
        }

        var model = new MethodModel
        {
            Entity = parentClass,
            ContainingMethod = null,
            ContainingProperty = null,
            Type = MethodType.Destructor,
            Name = destructorType.Name,
            Modifier = destructorType.Modifier,
            AccessModifier = ConvertAccessModifier(destructorType.AccessModifier),
            Modifiers = ConvertModifierToModifierList(destructorType.Modifier),
            LinesOfCode = ConvertLoc(destructorType.Loc),
            CyclomaticComplexity = destructorType.CyclomaticComplexity,
            Attributes = ConvertAttributes(destructorType.Attributes, projectModel),
            Parameters = ConvertParameters(destructorType.ParameterTypes, projectModel),
            LocalVariables = ConvertLocalVariables(destructorType.LocalVariableTypes, projectModel),
            ReturnValue = null,
            GenericParameters = new List<GenericParameterModel>(),
        };

        AddMetrics(model, destructorType);
        model.LocalFunctions = ConvertLocalFunctions(parentClass, model, destructorType.LocalFunctions, projectModel);

        return model;
    }

    private MethodModel ConvertMethod(EntityModel entityModel, MethodModel? parentMethod, IMethodType methodType,
        ProjectModel projectModel)
    {
        var model = new MethodModel
        {
            ContainingMethod = parentMethod,
            Entity = entityModel,
            Name = methodType.Name,
            Type = MethodType.Method,
            LinesOfCode = ConvertLoc(methodType.Loc),
            Modifier = methodType.Modifier,
            AccessModifier = ConvertAccessModifier(methodType.AccessModifier),
            Modifiers = ConvertModifierToModifierList(methodType.Modifier),
            GenericParameters = ConvertGenericParameters(methodType.GenericParameters, projectModel),
            CyclomaticComplexity = methodType.CyclomaticComplexity,
            Attributes = ConvertAttributes(methodType.Attributes, projectModel),
            ReturnValue = ConvertReturnValue(methodType.ReturnValue, projectModel),
            Parameters = ConvertParameters(methodType.ParameterTypes, projectModel),
            LocalVariables = ConvertLocalVariables(methodType.LocalVariableTypes, projectModel),
        };

        AddMetrics(model, methodType);

        if (methodType.ParameterTypes.Count > 0 && model.Parameters[0].Modifier == ParameterModifier.This)
        {
            model.Type = MethodType.Extension;
        }

        model.LocalFunctions = ConvertLocalFunctions(entityModel, model, methodType.LocalFunctions, projectModel);

        return model;
    }

    private MethodModel ConvertAccessor(EntityModel entity, PropertyModel parentProperty, IAccessorType accessorType,
        ProjectModel projectModel)
    {
        var model = new MethodModel
        {
            ContainingMethod = null,
            ContainingProperty = parentProperty,
            Entity = entity,
            Name = accessorType.Name,
            Type = MethodType.Accessor,
            LinesOfCode = ConvertLoc(accessorType.Loc),
            Modifier = accessorType.Modifier,
            AccessModifier = ConvertAccessModifier(accessorType.AccessModifier),
            Modifiers = ConvertModifierToModifierList(accessorType.Modifier),
            GenericParameters = new List<GenericParameterModel>(),
            CyclomaticComplexity = accessorType.CyclomaticComplexity,
            Attributes = ConvertAttributes(accessorType.Attributes, projectModel),
            ReturnValue = ConvertReturnValue(accessorType.ReturnValue, projectModel),
            Parameters = ConvertParameters(accessorType.ParameterTypes, projectModel),
            LocalVariables = ConvertLocalVariables(accessorType.LocalVariableTypes, projectModel),
        };

        AddMetrics(model, accessorType);
        model.LocalFunctions = ConvertLocalFunctions(entity, model, accessorType.LocalFunctions, projectModel);

        return model;
    }

    private IList<MethodModel> ConvertLocalFunctions(EntityModel entityModel, MethodModel parentMethod,
        IEnumerable<IMethodTypeWithLocalFunctions> localFunctions, ProjectModel projectModel)
    {
        return localFunctions.Select(localFunction =>
            {
                var localFunctionMethod = ConvertMethod(entityModel, parentMethod, localFunction, projectModel);
                localFunctionMethod.Type = MethodType.LocalFunction;
                return localFunctionMethod;
            })
            .ToList();
    }

    private IList<LocalVariableModel> ConvertLocalVariables(IEnumerable<ILocalVariableType> localVariableTypes,
        ProjectModel projectModel)
    {
        return localVariableTypes.Select(localVariableType => new LocalVariableModel
        {
            Type = ConvertEntityType(localVariableType.Type, projectModel),
            Name = localVariableType.Name,
            Modifier = localVariableType.Modifier,
        }).ToList();
    }

    #endregion

    #region MethodCalls FieldAccesses

    private void PopulateModelWithMethodReferencesAndFieldAccesses(RepositoryModel repositoryModel,
        Models.RepositoryModel referenceRepositoryModel)
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

                foreach (var entityModel in compilationUnit.Entities)
                {
                    var classType =
                        compilationUnitType.ClassTypes.FirstOrDefault(c => c.Name == entityModel.Name);

                    if (classType is not IMembersClassType membersClassType)
                    {
                        continue;
                    }

                    switch (entityModel)
                    {
                        case ClassModel classModel:
                        {
                            for (var methodIndex = 0; methodIndex < membersClassType.Methods.Count; methodIndex++)
                            {
                                var methodModel = classModel.Methods[methodIndex];
                                var methodType = membersClassType.Methods[methodIndex];

                                ConvertFieldAccesses(methodType.AccessedFields, methodModel);
                                ConvertOutgoingCalls(methodModel, methodType);
                            }

                            for (var constructorIndex = 0;
                                 constructorIndex < membersClassType.Constructors.Count;
                                 constructorIndex++)
                            {
                                var constructorModel = classModel.Constructors[constructorIndex];
                                var constructorType = membersClassType.Constructors[constructorIndex];

                                ConvertFieldAccesses(constructorType.AccessedFields, constructorModel);
                                ConvertOutgoingCalls(constructorModel, constructorType);
                            }

                            if (membersClassType.Destructor != null)
                            {
                                ConvertFieldAccesses(membersClassType.Destructor.AccessedFields, classModel.Destructor);
                                ConvertOutgoingCalls(classModel.Destructor, membersClassType.Destructor);
                            }
                        }
                            break;

                        case InterfaceModel interfaceModel:
                        {
                            for (var methodIndex = 0; methodIndex < membersClassType.Methods.Count; methodIndex++)
                            {
                                var methodModel = interfaceModel.Methods[methodIndex];
                                var methodType = membersClassType.Methods[methodIndex];

                                ConvertFieldAccesses(methodType.AccessedFields, methodModel);
                                ConvertOutgoingCalls(methodModel, methodType);
                            }
                        }
                            break;
                    }

                    if (classType is not IPropertyMembersClassType propertyMembersClassType)
                    {
                        continue;
                    }

                    switch (entityModel)
                    {
                        case ClassModel classModel:
                        {
                            for (var propertyIndex = 0; propertyIndex < classModel.Properties.Count; propertyIndex++)
                            {
                                var propertyModel = classModel.Properties[propertyIndex];
                                var propertyType = propertyMembersClassType.Properties[propertyIndex];
                                for (var accessorIndex = 0;
                                     accessorIndex < propertyModel.Accessors.Count;
                                     accessorIndex++)
                                {
                                    var accessor = propertyModel.Accessors[accessorIndex];
                                    var accessorType = propertyType.Accessors[accessorIndex];

                                    ConvertFieldAccesses(accessorType.AccessedFields, accessor);
                                    ConvertOutgoingCalls(accessor, accessorType);
                                }
                            }
                        }
                            break;

                        case InterfaceModel interfaceModel:
                        {
                            for (var propertyIndex = 0;
                                 propertyIndex < interfaceModel.Properties.Count;
                                 propertyIndex++)
                            {
                                var propertyModel = interfaceModel.Properties[propertyIndex];
                                var propertyType = propertyMembersClassType.Properties[propertyIndex];
                                for (var accessorIndex = 0;
                                     accessorIndex < propertyModel.Accessors.Count;
                                     accessorIndex++)
                                {
                                    var accessor = propertyModel.Accessors[accessorIndex];
                                    var accessorType = propertyType.Accessors[accessorIndex];

                                    ConvertFieldAccesses(accessorType.AccessedFields, accessor);
                                    ConvertOutgoingCalls(accessor, accessorType);
                                }
                            }
                        }
                            break;
                    }
                }

                void ConvertFieldAccesses(IEnumerable<AccessedField> accessedFields, MethodModel? methodModel)
                {
                    if (methodModel == null)
                    {
                        return;
                    }

                    foreach (var accessedField in accessedFields)
                    {
                        methodModel.FieldAccesses.Add(new FieldAccess
                        {
                            Field = GetFieldReference(accessedField, projectModel),
                            Caller = methodModel,
                            AccessEntityType = ConvertEntityType(accessedField.LocationClassName, projectModel),
                            AccessKind = ConvertAccessKind(accessedField.Kind),
                        });
                    }
                }

                void ConvertOutgoingCalls(MethodModel methodModel, ICallingMethodsType methodType)
                {
                    ConvertCalledMethods(methodType.CalledMethods, methodModel);

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

                        ConvertCalledMethods(localFunctionType.CalledMethods, localFunction);

                        for (var i = 0; i < localFunction.LocalFunctions.Count; i++)
                        {
                            ConvertOutgoingCalls(localFunction.LocalFunctions[i],
                                localFunctionType.LocalFunctions[i]);
                        }
                    }
                }

                void ConvertCalledMethods(IEnumerable<IMethodCallType> calledMethods, MethodModel methodModel)
                {
                    foreach (var calledMethod in calledMethods)
                    {
                        methodModel.OutgoingCalls.Add(new MethodCall
                        {
                            Caller = methodModel,
                            Called = GetMethodReference(calledMethod, projectModel),
                            CalledEnitityType = ConvertEntityType(calledMethod.LocationClassName, projectModel),
                            // todo
                        });
                    }
                }
            }
        }
    }

    private static MethodModel GetMethodReferenceByName(ClassModel classModel, string methodName,
        int genericParameterCount, IList<IParameterType> parameterTypes)
    {
        foreach (var methodModel in classModel.Methods)
        {
            if (methodModel.Name != methodName)
            {
                continue;
            }

            if (methodModel.Parameters.Count != parameterTypes.Count)
            {
                continue;
            }

            if (methodModel.GenericParameters.Count != genericParameterCount)
            {
                // todo ???
            }

            var allParametersMatch = true;
            var methodModelParameters = methodModel.Parameters;
            for (var i = 0; i < methodModelParameters.Count; i++)
            {
                var parameterModel = methodModelParameters[i];
                var parameterType = parameterTypes[i];

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

        return null;
    }

    private FieldModel GetFieldReference(AccessedField accessedField, ProjectModel projectModel)
    {
        var locationClassModelPossibilities = SearchEntityByName(accessedField.LocationClassName, projectModel)
            .Where(classModel => classModel is ClassModel)
            .Cast<ClassModel>();
        foreach (var locationClassModel in locationClassModelPossibilities)
        {
            var fieldReference = locationClassModel.Fields.FirstOrDefault(field => field.Name == accessedField.Name);
            if (fieldReference != null)
            {
                return fieldReference;
            }
        }

        var definitionClassModelPossibilities = SearchEntityByName(accessedField.DefinitionClassName, projectModel)
            .Where(classModel => classModel is ClassModel)
            .Cast<ClassModel>();

        foreach (var definitionClassModel in definitionClassModelPossibilities)
        {
            var fieldReference = definitionClassModel.Fields.FirstOrDefault(field => field.Name == accessedField.Name);
            if (fieldReference != null)
            {
                return fieldReference;
            }
        }

        var classModel = CreateClassModel(accessedField.LocationClassName);

        var fieldModel = new FieldModel
        {
            Name = accessedField.Name,
            Entity = classModel,
            AccessModifier = AccessModifier.Public,
        };
        classModel.Fields.Add(fieldModel);

        return fieldModel;
    }

    private static AccessKind ConvertAccessKind(AccessedField.AccessKind accessedFieldKind)
    {
        return accessedFieldKind switch
        {
            AccessedField.AccessKind.Getter => AccessKind.Getter,
            AccessedField.AccessKind.Setter => AccessKind.Setter,
            _ => AccessKind.Getter,
        };
    }

    private MethodModel GetMethodReference(IMethodCallType methodCallType, ProjectModel projectModel)
    {
        var locationClassModelPossibilities = SearchEntityByName(methodCallType.LocationClassName, projectModel)
            .Where(classModel => classModel is ClassModel)
            .Cast<ClassModel>();

        foreach (var classModel in locationClassModelPossibilities)
        {
            if (methodCallType.MethodDefinitionNames.Count == 0)
            {
                var methodReference = GetMethodReferenceByName(classModel, methodCallType.Name,
                    methodCallType.GenericParameters.Count, methodCallType.ParameterTypes);
                if (methodReference != null)
                {
                    return methodReference;
                }
            }
            else
            {
                var methodReference = GetMethodReferenceFromLocalFunction(methodCallType, classModel);
                if (methodReference != null)
                {
                    return methodReference;
                }
            }
        }

        var definitionClassModelPossibilities = SearchEntityByName(methodCallType.DefinitionClassName, projectModel)
            .Where(classModel => classModel is ClassModel)
            .Cast<ClassModel>();

        foreach (var classModel in definitionClassModelPossibilities)
        {
            if (methodCallType.MethodDefinitionNames.Count == 0)
            {
                var methodReference = GetMethodReferenceByName(classModel, methodCallType.Name,
                    methodCallType.GenericParameters.Count, methodCallType.ParameterTypes);
                if (methodReference != null)
                {
                    return methodReference;
                }
            }
            else
            {
                var methodReference = GetMethodReferenceFromLocalFunction(methodCallType, classModel);
                if (methodReference != null)
                {
                    return methodReference;
                }
            }
        }

        var createdClassModel = CreateClassModel(methodCallType.LocationClassName);

        var genericParameters = new List<GenericParameterModel>();
        for (var i = 0; i < methodCallType.GenericParameters.Count; i++)
        {
            genericParameters.Add(new GenericParameterModel
            {
                Name = $"T{i}",
            });
        }

        var methodModel = new MethodModel
        {
            Name = methodCallType.Name,
            GenericParameters = genericParameters,
            Entity = createdClassModel,
            Parameters = methodCallType.ParameterTypes.Select(parameter => new ParameterModel
            {
                TypeName = parameter.Type?.Name,
                Type = ConvertEntityType(parameter.Type, projectModel),
            }).ToList()
        };

        createdClassModel.Methods.Add(methodModel);

        return methodModel;
    }

    private static MethodModel? GetMethodReferenceFromLocalFunction(IMethodCallType methodCallType,
        ClassModel classModel)
    {
        if (methodCallType.MethodDefinitionNames.Count == 0)
        {
            return null;
        }

        int indexOfLocalFunctionChain;
        MethodModel methodModelToSearchLocalFunctions;

        var indexOfParenthesis = methodCallType.MethodDefinitionNames[0].IndexOf('(');
        if (indexOfParenthesis >= 0)
        {
            // search for method

            var indexOfGenericParenthesis = methodCallType.MethodDefinitionNames[0].IndexOf('<');
            var genericParametersCount = indexOfGenericParenthesis == -1
                ? 0
                : methodCallType.MethodDefinitionNames[0][..indexOfParenthesis].Count(c => c == ',') + 1;

            if (genericParametersCount > 0)
            {
                indexOfParenthesis = indexOfGenericParenthesis;
            }

            var methodName = methodCallType.MethodDefinitionNames[0][..indexOfParenthesis];
            var parametersText = methodCallType.MethodDefinitionNames[0][(indexOfParenthesis + 1)..^1];
            var parameters = string.IsNullOrEmpty(parametersText)
                ? new List<string>()
                : parametersText.Split(',').Select(p => p.Trim());
            IList<IParameterType> parameterTypes = parameters
                .Select(parameter =>
                {
                    var indexOfNullable = parameter.IndexOf('?');
                    return new HoneydewModels.CSharp.ParameterModel
                    {
                        Type = new EntityTypeModel
                        {
                            Name = indexOfNullable >= 0 ? parameter[..indexOfNullable] : parameter
                        },
                        IsNullable = indexOfNullable >= 0
                    };
                }).Cast<IParameterType>().ToList();
            var methodModel = GetMethodReferenceByName(classModel, methodName, genericParametersCount, parameterTypes);
            if (methodModel == null)
            {
                return null;
            }

            methodModelToSearchLocalFunctions = methodModel;
            indexOfLocalFunctionChain = 1;
        }
        else
        {
            // search for property
            var property = classModel.Properties.FirstOrDefault(p => p.Name == methodCallType.MethodDefinitionNames[0]);
            if (property == null)
            {
                return null;
            }

            if (methodCallType.MethodDefinitionNames.Count == 1) // it doesn't have an accessor
            {
                return null;
            }

            var accessor = property.Accessors.FirstOrDefault(a => a.Name == methodCallType.MethodDefinitionNames[1]);
            if (accessor == null)
            {
                return null;
            }

            indexOfLocalFunctionChain = 2;
            methodModelToSearchLocalFunctions = accessor;
        }

        for (var i = indexOfLocalFunctionChain; i < methodCallType.MethodDefinitionNames.Count; i++)
        {
            // todo
            var localFunctionName = methodCallType.MethodDefinitionNames[i];
            var nextLocalFunction = methodModelToSearchLocalFunctions.LocalFunctions.FirstOrDefault(function =>
                localFunctionName.StartsWith(function.Name));
            if (nextLocalFunction == null)
            {
                return null;
            }

            methodModelToSearchLocalFunctions = nextLocalFunction;
        }

        return methodModelToSearchLocalFunctions.LocalFunctions.FirstOrDefault(function =>
            function.Name == methodCallType.Name);
    }

    #endregion

    private IList<AttributeModel> ConvertAttributes(IEnumerable<IAttributeType> attributeTypes,
        ProjectModel projectModel)
    {
        return attributeTypes.Select(attributeType => new AttributeModel
            {
                Type = ConvertEntityType(attributeType.Type, projectModel),
                Target = ConvertAttributeTarget(attributeType.Target),
                Parameters = ConvertParameters(attributeType.ParameterTypes, projectModel),
                // todo Generic Concrete Generic Parameters
            })
            .ToList();
    }

    private static AttributeTarget ConvertAttributeTarget(string target)
    {
        return Enum.TryParse<AttributeTarget>(target, true, out var result)
            ? result
            : AttributeTarget.None;
    }

    private IList<ParameterModel> ConvertParameters(IEnumerable<IParameterType> parameterTypes,
        ProjectModel projectModel)
    {
        var parameters = new List<ParameterModel>();

        foreach (var parameterType in parameterTypes)
        {
            var parameterModel = new ParameterModel
            {
                TypeName = parameterType.Type?.Name,
                Type = ConvertEntityType(parameterType.Type, projectModel),
                Attributes = ConvertAttributes(parameterType.Attributes, projectModel),
            };

            if (parameterType is HoneydewModels.CSharp.ParameterModel param)
            {
                parameterModel.Modifier = ConvertParameterModifier(param.Modifier);
                parameterModel.DefaultValue = param.DefaultValue;
            }

            parameters.Add(parameterModel);
        }

        return parameters;
    }

    private static ParameterModifier ConvertParameterModifier(string modifier)
    {
        return Enum.TryParse<ParameterModifier>(modifier, true, out var result)
            ? result
            : ParameterModifier.None;
    }

    private IList<GenericParameterModel> ConvertGenericParameters(
        IEnumerable<IGenericParameterType> genericParameters, ProjectModel projectModel)
    {
        return genericParameters.Select(parameterType => new GenericParameterModel
            {
                Name = parameterType.Name,
                Modifier = ConvertGenericParameterModifier(parameterType.Modifier),
                Constraints = parameterType.Constraints.Select(c => ConvertEntityType(c, projectModel)).ToList(),
                Attributes = ConvertAttributes(parameterType.Attributes, projectModel),
            })
            .ToList();
    }

    private static GenericParameterModifier ConvertGenericParameterModifier(string modifier)
    {
        return Enum.TryParse<GenericParameterModifier>(modifier, true, out var result)
            ? result
            : GenericParameterModifier.None;
    }

    private EntityType ConvertEntityType(string typeName, ProjectModel projectModel)
    {
        return ConvertEntityType(_fullTypeNameBuilder.CreateEntityTypeModel(typeName), projectModel);
    }

    private EntityType ConvertEntityType(IEntityType type, ProjectModel projectModel)
    {
        var entityModel =
            type.FullType == null
                ? SearchEntityByName(type.Name, projectModel).FirstOrDefault()
                : SearchEntityByName(type.FullType.Name, type.FullType.ContainedTypes.Count, projectModel)
                      .FirstOrDefault() ??
                  CreateClassModel(type.Name);

        var entityType = new EntityType
        {
            Name = type.Name.TrimEnd('?'),
            Entity = entityModel,
            IsNullable = type.FullType?.IsNullable ?? type.Name.EndsWith('?'),
            GenericTypes = type.FullType == null
                ? new List<EntityType>()
                : ConvertGeneric(type.FullType.ContainedTypes, projectModel),
        };

        return entityType;
    }

    private IList<EntityType> ConvertGeneric(IEnumerable<GenericType> genericTypes, ProjectModel projectModel)
    {
        var entityTypes = new List<EntityType>();

        foreach (var genericType in genericTypes)
        {
            var entityModel = SearchEntityByName(genericType.Name, genericType.ContainedTypes.Count, projectModel)
                                  .FirstOrDefault() ??
                              CreateClassModel(genericType.Name);

            var entityType = new EntityType
            {
                Name = genericType.Name,
                IsNullable = genericType.IsNullable,
                Entity = entityModel,
                GenericTypes = ConvertGeneric(genericType.ContainedTypes, projectModel),
            };

            entityTypes.Add(entityType);
        }

        return entityTypes;
    }

    private ClassModel CreateClassModel(string className)
    {
        var entityTypeModel = _fullTypeNameBuilder.CreateEntityTypeModel(className);

        var entityName = entityTypeModel.FullType.Name;
        var genericParameterCount = entityTypeModel.FullType.ContainedTypes.Count;
        if (_generatedTypes.TryGetValue((entityName, genericParameterCount),
                out var classModel))
        {
            return classModel;
        }

        var genericParameters = new List<GenericParameterModel>();
        for (var i = 0; i < genericParameterCount; i++)
        {
            genericParameters.Add(new GenericParameterModel
            {
                Name = $"T{i}",
            });
        }

        classModel = new ClassModel
        {
            Name = entityName,
            IsInternal = false,
            GenericParameters = genericParameters,
            // namespace ?
        };
        var isPrimitive = CSharpConstants.IsPrimitive(classModel.Name);
        classModel.IsPrimitive = isPrimitive;
        classModel.IsExternal = !isPrimitive;

        _generatedTypes.Add((className, genericParameterCount), classModel);

        return classModel;
    }
}

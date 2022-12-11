using System.Collections.Concurrent;
using DxWorks.ScriptBee.Plugins.Honeydew.Models;
using Honeydew.Logging;
using Honeydew.Models.Types;
using HoneydewRepositoryModel = Honeydew.Models.RepositoryModel;
using HoneydewLinesOfCode = Honeydew.Models.LinesOfCode;
using static DxWorks.ScriptBee.Plugins.Honeydew.Loaders.MetricAdder;

namespace DxWorks.ScriptBee.Plugins.Honeydew.Loaders;

public class RepositoryModelToReferenceRepositoryModelProcessor
{
    private readonly ILogger _logger;
    private readonly IProgressLogger _progressLogger;

    private readonly IDictionary<(string className, int genericParameterCount), ClassModel> _generatedTypes =
        new ConcurrentDictionary<(string className, int genericParameterCount), ClassModel>();

    private readonly IDictionary<string, NamespaceModel> _generatedNamespaces =
        new ConcurrentDictionary<string, NamespaceModel>();

    private readonly IDictionary<(ProjectModel, string className, int genericParameterCount), IList<EntityModel>>
        _entityModels =
            new ConcurrentDictionary<(ProjectModel, string className, int genericParameterCount), IList<EntityModel>>();

    public RepositoryModelToReferenceRepositoryModelProcessor(ILogger logger, IProgressLogger progressLogger)
    {
        _logger = logger;
        _progressLogger = progressLogger;
    }

    public RepositoryModel Process(HoneydewRepositoryModel? inputRepositoryModel)
    {
        if (inputRepositoryModel == null)
        {
            return new RepositoryModel();
        }

        Log("ReferenceModel - Updating Model Version");

        var repositoryModel = new RepositoryModel
        {
            Version = inputRepositoryModel.Version
        };

        Log("ReferenceModel - Populating Solutions, Projects and File Models");
        PopulateModelWithSolutionProjectNamespacesCompilationUnitsAndClasses(inputRepositoryModel, repositoryModel);

        Log("ReferenceModel - Populating Project References");
        PopulateProjectWithProjectReferences(inputRepositoryModel, repositoryModel);

        Log("ReferenceModel - Populating Entity References");
        PopulateWithEntityReferences(inputRepositoryModel, repositoryModel);

        Log("ReferenceModel - Populating Method Calls and Field Accesses");
        PopulateModelWithMethodReferencesAndFieldAccesses(inputRepositoryModel, repositoryModel);

        repositoryModel.CreatedClasses = _generatedTypes.Select(pair => pair.Value).ToList();
        repositoryModel.CreatedNamespaces = _generatedNamespaces.Select(pair => pair.Value).ToList();

        return repositoryModel;
    }

    #region Solutions Project Namespaces CompilationUnits Classes

    private void PopulateModelWithSolutionProjectNamespacesCompilationUnitsAndClasses(
        HoneydewRepositoryModel repositoryModel,
        RepositoryModel referenceRepositoryModel)
    {
        var namespaceTreeHandler = new NamespaceTreeHandler();

        foreach (var solutionModel in repositoryModel.Solutions)
        {
            Log($"ReferenceModel - Populating Solution {solutionModel.FilePath}");
            var referenceSolutionModel = new SolutionModel
            {
                FilePath = solutionModel.FilePath,
                Repository = referenceRepositoryModel
            };
            referenceRepositoryModel.Solutions.Add(referenceSolutionModel);
        }

        foreach (var projectModel in repositoryModel.Projects)
        {
            Log($"ReferenceModel - Populating Project {projectModel.FilePath}");
            var presentNamespacesSet = new HashSet<string>();

            var referenceProjectModel = new ProjectModel
            {
                Name = projectModel.Name,
                FilePath = projectModel.FilePath,
                Repository = referenceRepositoryModel,
                Language = projectModel.Language,
            };

            referenceRepositoryModel.Projects.Add(referenceProjectModel);

            var repositoryModelConversionStrategy = GetRepositoryModelConversionStrategy(projectModel.Language);

            foreach (var compilationUnitType in projectModel.CompilationUnits)
            {
                Log($"ReferenceModel - Populating File {compilationUnitType.FilePath}");
                var fileModel = new FileModel
                {
                    FilePath = compilationUnitType.FilePath,
                    Project = referenceProjectModel,
                    Loc = ConvertLoc(compilationUnitType.Loc),
                };

                referenceProjectModel.Files.Add(fileModel);

                foreach (var classType in compilationUnitType.ClassTypes)
                {
                    var namespaceModel = namespaceTreeHandler.GetOrAdd(classType.ContainingNamespaceName);
                    presentNamespacesSet.Add(classType.ContainingNamespaceName);

                    if (repositoryModelConversionStrategy.IsDelegateModel(classType))
                    {
                        var model = new DelegateModel
                        {
                            Name = classType.Name,
                            FilePath = classType.FilePath,
                            Modifier = classType.Modifier,
                            AccessModifier = ConvertAccessModifier(classType.AccessModifier),
                            Modifiers = ConvertModifierToModifierList(classType.Modifier),
                            File = fileModel,
                            Namespace = namespaceModel,
                            IsInternal = true,
                            IsExternal = false,
                            IsPrimitive = false,
                            LinesOfCode = ConvertLoc(classType.Loc),
                        };

                        namespaceModel.Entities.Add(model);
                        fileModel.Entities.Add(model);

                        // todo check case when there is a delegate with the same name in the same project 
                        var genericParameterCount =
                            repositoryModelConversionStrategy.GetGenericParameterCount(classType);
                        _entityModels[(referenceProjectModel, classType.Name, genericParameterCount)] =
                            new List<EntityModel>
                            {
                                model
                            };
                    }
                    else if (repositoryModelConversionStrategy.IsEnumModel(classType))
                    {
                        var model = new EnumModel
                        {
                            Name = classType.Name,
                            FilePath = classType.FilePath,
                            Modifier = classType.Modifier,
                            AccessModifier = ConvertAccessModifier(classType.AccessModifier),
                            Modifiers = ConvertModifierToModifierList(classType.Modifier),
                            File = fileModel,
                            Namespace = namespaceModel,
                            IsInternal = true,
                            IsExternal = false,
                            IsPrimitive = false,
                            LinesOfCode = ConvertLoc(classType.Loc),
                            Type = repositoryModelConversionStrategy.GetEnumType(classType)
                        };
                        namespaceModel.Entities.Add(model);
                        fileModel.Entities.Add(model);

                        // todo check case when there is an enum with the same name in the same project
                        _entityModels[(referenceProjectModel, classType.Name, 0)] = new List<EntityModel>
                        {
                            model
                        };
                    }
                    else if (repositoryModelConversionStrategy.IsInterfaceModel(classType))
                    {
                        var model = new InterfaceModel
                        {
                            Name = classType.Name,
                            FilePath = classType.FilePath,
                            Modifier = classType.Modifier,
                            AccessModifier = ConvertAccessModifier(classType.AccessModifier),
                            Modifiers = ConvertModifierToModifierList(classType.Modifier),
                            File = fileModel,
                            Namespace = namespaceModel,
                            IsInternal = true,
                            IsExternal = false,
                            IsPrimitive = false,
                            LinesOfCode = ConvertLoc(classType.Loc),
                        };

                        namespaceModel.Entities.Add(model);
                        fileModel.Entities.Add(model);

                        var genericParameterCount =
                            repositoryModelConversionStrategy.GetGenericParameterCount(classType);
                        if (_entityModels.TryGetValue((referenceProjectModel, model.Name, genericParameterCount),
                                out var entities))
                        {
                            foreach (var entity in entities.OfType<InterfaceModel>())
                            {
                                entity.Partials.Add(model);
                                model.Partials.Add(entity);
                            }

                            entities.Add(model);
                        }
                        else
                        {
                            _entityModels.Add((referenceProjectModel, model.Name, genericParameterCount),
                                new List<EntityModel>
                                {
                                    model
                                });
                        }
                    }
                    else if (repositoryModelConversionStrategy.IsClassModel(classType))
                    {
                        var model = new ClassModel
                        {
                            Name = classType.Name,
                            FilePath = classType.FilePath,
                            Type = ConvertClassType(classType.ClassType),
                            Modifier = classType.Modifier,
                            AccessModifier = ConvertAccessModifier(classType.AccessModifier),
                            Modifiers = ConvertModifierToModifierList(classType.Modifier),
                            File = fileModel,
                            Namespace = namespaceModel,
                            IsInternal = true,
                            IsExternal = false,
                            IsPrimitive = false,
                            LinesOfCode = ConvertLoc(classType.Loc),
                        };

                        namespaceModel.Entities.Add(model);
                        fileModel.Entities.Add(model);

                        var genericParameterCount =
                            repositoryModelConversionStrategy.GetGenericParameterCount(classType);
                        if (_entityModels.TryGetValue((referenceProjectModel, model.Name, genericParameterCount),
                                out var entities))
                        {
                            foreach (var entity in entities.OfType<ClassModel>())
                            {
                                model.Partials.Add(entity);
                                entity.Partials.Add(model);
                            }

                            entities.Add(model);
                        }
                        else
                        {
                            _entityModels.Add(
                                (referenceProjectModel, model.Name, genericParameterCount),
                                new List<EntityModel>
                                {
                                    model
                                });
                        }
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


    private static LinesOfCode ConvertLoc(HoneydewLinesOfCode linesOfCode)
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

    private void PopulateProjectWithProjectReferences(HoneydewRepositoryModel repositoryModel,
        RepositoryModel referenceRepositoryModel)
    {
        var allProjects = referenceRepositoryModel.Solutions.SelectMany(model => model.Projects).ToList();

        for (var projectIndex = 0; projectIndex < repositoryModel.Projects.Count; projectIndex++)
        {
            var projectModel = referenceRepositoryModel.Projects[projectIndex];
            Log($"ReferenceModel - Setting Project References for {projectModel.FilePath}");

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

    private void PopulateWithEntityReferences(HoneydewRepositoryModel repositoryModel,
        RepositoryModel referenceRepositoryModel)
    {
        for (var projectIndex = 0; projectIndex < referenceRepositoryModel.Projects.Count; projectIndex++)
        {
            var projectModel = referenceRepositoryModel.Projects[projectIndex];
            var repositoryModelConversionStrategy = GetRepositoryModelConversionStrategy(projectModel.Language);

            for (var compilationUnitIndex = 0; compilationUnitIndex < projectModel.Files.Count; compilationUnitIndex++)
            {
                var file = projectModel.Files[compilationUnitIndex];
                var compilationUnitType = repositoryModel.Projects[projectIndex].CompilationUnits[compilationUnitIndex];

                AddMetrics(file, compilationUnitType);

                file.Imports = compilationUnitType.Imports.Select(import =>
                        ConvertImportType(import, projectModel, repositoryModelConversionStrategy))
                    .ToList();

                for (var classTypeIndex = 0; classTypeIndex < compilationUnitType.ClassTypes.Count; classTypeIndex++)
                {
                    var entityModel = file.Entities[classTypeIndex];
                    var classType = compilationUnitType.ClassTypes[classTypeIndex];

                    Log($"ReferenceModel - Populating Entity References for {entityModel.FilePath}");

                    entityModel.Imports = classType.Imports.Select(import =>
                            ConvertImportType(import, projectModel, repositoryModelConversionStrategy))
                        .ToList();
                    entityModel.Attributes = ConvertAttributes(classType.Attributes, projectModel,
                        repositoryModelConversionStrategy);

                    AddMetrics(entityModel, classType);

                    switch (entityModel)
                    {
                        case ClassModel classModel when classType is IPropertyMembersClassType propertyMembersClassType:
                        {
                            if (classType is ITypeWithGenericParameters typeWithGenericParameters)
                            {
                                classModel.GenericParameters =
                                    ConvertGenericParameters(typeWithGenericParameters.GenericParameters, projectModel,
                                        repositoryModelConversionStrategy);
                            }

                            foreach (var baseType in classType.BaseTypes)
                            {
                                // TODO: Bug: If baseType is external, a ClassModel is always created, even if the baseType.Kind is an interface 
                                classModel.BaseTypes.Add(ConvertEntityType(baseType.Type, projectModel,
                                    repositoryModelConversionStrategy));
                            }

                            classModel.Methods = PopulateWithMethodModels(classModel, propertyMembersClassType.Methods)
                                .ToList();
                            classModel.Constructors = propertyMembersClassType.Constructors
                                .Select(constructorType =>
                                    ConvertConstructor(classModel, constructorType, projectModel,
                                        repositoryModelConversionStrategy))
                                .ToList();
                            classModel.Destructor =
                                ConvertDestructor(classModel, propertyMembersClassType.Destructor, projectModel,
                                    repositoryModelConversionStrategy);

                            classModel.Fields = propertyMembersClassType.Fields
                                .Select(fieldType => ConvertField(classModel, fieldType, projectModel,
                                    repositoryModelConversionStrategy))
                                .ToList();

                            classModel.Properties =
                                PopulateWithPropertyModels(classModel, propertyMembersClassType.Properties).ToList();

                            break;
                        }
                        case InterfaceModel interfaceModel
                            when classType is IPropertyMembersClassType propertyMembersClassType:
                        {
                            if (classType is ITypeWithGenericParameters typeWithGenericParameters)
                            {
                                interfaceModel.GenericParameters =
                                    ConvertGenericParameters(typeWithGenericParameters.GenericParameters, projectModel,
                                        repositoryModelConversionStrategy);
                            }

                            foreach (var baseType in classType.BaseTypes)
                            {
                                interfaceModel.BaseTypes.Add(ConvertEntityType(baseType.Type, projectModel,
                                    repositoryModelConversionStrategy));
                            }

                            interfaceModel.Methods =
                                PopulateWithMethodModels(interfaceModel, propertyMembersClassType.Methods).ToList();

                            interfaceModel.Properties =
                                PopulateWithPropertyModels(interfaceModel, propertyMembersClassType.Properties)
                                    .ToList();

                            break;
                        }

                        case DelegateModel delegateModel
                            when classType is IDelegateType delegateType:
                        {
                            delegateModel.Parameters =
                                ConvertParameters(delegateType.ParameterTypes, projectModel,
                                    repositoryModelConversionStrategy);
                            delegateModel.ReturnValue =
                                ConvertReturnValue(delegateType.ReturnValue, projectModel,
                                    repositoryModelConversionStrategy);

                            if (classType is ITypeWithGenericParameters typeWithGenericParameters)
                            {
                                delegateModel.GenericParameters =
                                    ConvertGenericParameters(typeWithGenericParameters.GenericParameters, projectModel,
                                        repositoryModelConversionStrategy);
                            }

                            break;
                        }
                        case EnumModel enumModel when classType is IEnumType enumType:
                            enumModel.Labels = ConvertEnumLabels(enumType.Labels, projectModel,
                                repositoryModelConversionStrategy).ToList();
                            break;
                    }
                }
            }

            IEnumerable<MethodModel> PopulateWithMethodModels(EntityModel parentEntity,
                IEnumerable<IMethodType> methodModels)
            {
                return methodModels.Select(methodType => ConvertMethod(parentEntity, null, methodType, projectModel,
                    repositoryModelConversionStrategy));
            }

            IEnumerable<PropertyModel> PopulateWithPropertyModels(EntityModel entityModel,
                IEnumerable<IPropertyType> properties)
            {
                return properties.Select(propertyType =>
                    ConvertProperty(entityModel, propertyType, projectModel, repositoryModelConversionStrategy));
            }
        }
    }

    private IEnumerable<EnumLabelModel> ConvertEnumLabels(IEnumerable<IEnumLabelType> enumLabels,
        ProjectModel projectModel, IRepositoryModelConversionStrategy repositoryModelConversionStrategy)
    {
        return enumLabels.Select(label => new EnumLabelModel
        {
            Name = label.Name,
            Attributes = ConvertAttributes(label.Attributes, projectModel, repositoryModelConversionStrategy),
        });
    }

    private ReturnValueModel ConvertReturnValue(IReturnValueType returnValueType, ProjectModel projectModel,
        IRepositoryModelConversionStrategy repositoryModelConversionStrategy)
    {
        return new ReturnValueModel
        {
            Type = ConvertEntityType(returnValueType.Type, projectModel, repositoryModelConversionStrategy),
            Attributes = ConvertAttributes(returnValueType.Attributes, projectModel, repositoryModelConversionStrategy),
            Modifier = returnValueType.Modifier
        };
    }

    private ImportModel ConvertImportType(IImportType importType, ProjectModel projectModel,
        IRepositoryModelConversionStrategy repositoryModelConversionStrategy)
    {
        var aliasType = ConvertAliasType(importType.AliasType);

        return new ImportModel
        {
            Alias = importType.Alias,
            IsStatic = importType.IsStatic,
            AliasType = aliasType,
            Entity = aliasType == AliasType.Class || importType.IsStatic
                ? SearchEntityByName(importType.Name, projectModel, repositoryModelConversionStrategy)
                      .FirstOrDefault() ??
                  CreateClassModel(importType.Name, repositoryModelConversionStrategy)
                : null,
            Namespace = !importType.IsStatic && aliasType is AliasType.Namespace or AliasType.None
                ? SearchNamespaceByName(importType.Name, projectModel)
                : null,
        };
    }

    private static AliasType ConvertAliasType(string type)
    {
        return Enum.TryParse<AliasType>(type, true, out var result)
            ? result
            : AliasType.None;
    }

    private NamespaceModel SearchNamespaceByName(string namespaceName, ProjectModel projectModel)
    {
        if (_generatedNamespaces.TryGetValue(namespaceName, out var namespaceModel))
        {
            return namespaceModel;
        }

        namespaceModel = projectModel.Namespaces.FirstOrDefault(model => model.FullName == namespaceName);

        if (namespaceModel != null)
        {
            return namespaceModel;
        }

        namespaceModel = projectModel.ProjectReferences
            .SelectMany(project => project.Namespaces)
            .FirstOrDefault(model => model.FullName == namespaceName);

        if (namespaceModel != null)
        {
            return namespaceModel;
        }

        namespaceModel = new NamespaceModel
        {
            FullName = namespaceName,
            Name = namespaceName,
        };

        _generatedNamespaces.Add(namespaceName, namespaceModel);

        return namespaceModel;
    }

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

        if (projectModel.ProjectReferences.Any(reference =>
                _entityModels.TryGetValue((reference, entityName, genericParameterCount), out entityModels)))
        {
            return entityModels ?? new List<EntityModel>();
        }

        if (_generatedTypes.TryGetValue((entityName, genericParameterCount), out var generatedType))
        {
            return new List<EntityModel> { generatedType };
        }

        return new List<EntityModel>();
    }

    private IEnumerable<EntityModel> SearchEntityByName(string entityName, ProjectModel projectModel,
        IRepositoryModelConversionStrategy repositoryModelConversionStrategy)
    {
        if (string.IsNullOrEmpty(entityName))
        {
            return new List<EntityModel>();
        }

        var entityTypeModel = repositoryModelConversionStrategy.CreateEntityTypeModel(entityName);
        return SearchEntityByName(entityTypeModel.FullType.Name, entityTypeModel.FullType.ContainedTypes.Count,
            projectModel);
    }

    private PropertyModel ConvertProperty(EntityModel entityModel, IPropertyType propertyType,
        ProjectModel projectModel, IRepositoryModelConversionStrategy repositoryModelConversionStrategy)
    {
        var model = new PropertyModel
        {
            Entity = entityModel,
            Name = propertyType.Name,
            Modifier = propertyType.Modifier,
            AccessModifier = ConvertAccessModifier(propertyType.AccessModifier),
            Modifiers = ConvertModifierToModifierList(propertyType.Modifier),
            IsEvent = repositoryModelConversionStrategy.IsEvent(propertyType),
            LinesOfCode = ConvertLoc(propertyType.Loc),
            CyclomaticComplexity = propertyType.CyclomaticComplexity,
            Type = ConvertEntityType(propertyType.Type, projectModel, repositoryModelConversionStrategy),
            Attributes = ConvertAttributes(propertyType.Attributes, projectModel, repositoryModelConversionStrategy),
        };

        AddMetrics(model, propertyType);
        model.Accessors = propertyType.Accessors
            .Select(accessor =>
                ConvertAccessor(entityModel, model, accessor, projectModel, repositoryModelConversionStrategy))
            .ToList();

        return model;
    }

    private FieldModel ConvertField(ClassModel classModel, IFieldType fieldType, ProjectModel projectModel,
        IRepositoryModelConversionStrategy repositoryModelConversionStrategy)
    {
        var model = new FieldModel
        {
            Entity = classModel,
            Name = fieldType.Name,
            Modifier = fieldType.Modifier,
            AccessModifier = ConvertAccessModifier(fieldType.AccessModifier),
            Modifiers = ConvertModifierToModifierList(fieldType.Modifier),
            IsEvent = repositoryModelConversionStrategy.IsEvent(fieldType),
            Type = ConvertEntityType(fieldType.Type, projectModel, repositoryModelConversionStrategy),
            Attributes = ConvertAttributes(fieldType.Attributes, projectModel, repositoryModelConversionStrategy),
        };
        AddMetrics(model, fieldType);

        return model;
    }

    private MethodModel ConvertConstructor(ClassModel parentClass, IConstructorType constructorType,
        ProjectModel projectModel, IRepositoryModelConversionStrategy repositoryModelConversionStrategy)
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
            Attributes = ConvertAttributes(constructorType.Attributes, projectModel, repositoryModelConversionStrategy),
            Parameters = ConvertParameters(constructorType.ParameterTypes, projectModel,
                repositoryModelConversionStrategy),
            LocalVariables = ConvertLocalVariables(constructorType.LocalVariableTypes, projectModel,
                repositoryModelConversionStrategy),
            ReturnValue = null,
            GenericParameters = new List<GenericParameterModel>(),
        };

        AddMetrics(model, constructorType);
        model.LocalFunctions = ConvertLocalFunctions(parentClass, model, constructorType.LocalFunctions, projectModel,
            repositoryModelConversionStrategy);

        return model;
    }

    private MethodModel? ConvertDestructor(ClassModel parentClass, IDestructorType? destructorType,
        ProjectModel projectModel, IRepositoryModelConversionStrategy repositoryModelConversionStrategy)
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
            Attributes = ConvertAttributes(destructorType.Attributes, projectModel, repositoryModelConversionStrategy),
            Parameters = ConvertParameters(destructorType.ParameterTypes, projectModel,
                repositoryModelConversionStrategy),
            LocalVariables = ConvertLocalVariables(destructorType.LocalVariableTypes, projectModel,
                repositoryModelConversionStrategy),
            ReturnValue = null,
            GenericParameters = new List<GenericParameterModel>(),
        };

        AddMetrics(model, destructorType);
        model.LocalFunctions = ConvertLocalFunctions(parentClass, model, destructorType.LocalFunctions, projectModel,
            repositoryModelConversionStrategy);

        return model;
    }

    private MethodModel ConvertMethod(EntityModel entityModel, MethodModel? parentMethod, IMethodType methodType,
        ProjectModel projectModel, IRepositoryModelConversionStrategy repositoryModelConversionStrategy)
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
            GenericParameters = ConvertGenericParameters(methodType.GenericParameters, projectModel,
                repositoryModelConversionStrategy),
            CyclomaticComplexity = methodType.CyclomaticComplexity,
            Attributes = ConvertAttributes(methodType.Attributes, projectModel, repositoryModelConversionStrategy),
            ReturnValue = ConvertReturnValue(methodType.ReturnValue, projectModel, repositoryModelConversionStrategy),
            Parameters = ConvertParameters(methodType.ParameterTypes, projectModel, repositoryModelConversionStrategy),
            LocalVariables = ConvertLocalVariables(methodType.LocalVariableTypes, projectModel,
                repositoryModelConversionStrategy),
        };

        AddMetrics(model, methodType);

        if (model.Parameters.Count > 0 && model.Parameters[0].Modifier == ParameterModifier.This)
        {
            model.Type = MethodType.Extension;
        }

        model.LocalFunctions = ConvertLocalFunctions(entityModel, model, methodType.LocalFunctions, projectModel,
            repositoryModelConversionStrategy);

        return model;
    }

    private MethodModel ConvertAccessor(EntityModel entity, PropertyModel parentProperty,
        IAccessorMethodType accessorMethodType, ProjectModel projectModel,
        IRepositoryModelConversionStrategy repositoryModelConversionStrategy)
    {
        var model = new MethodModel
        {
            ContainingMethod = null,
            ContainingProperty = parentProperty,
            Entity = entity,
            Name = accessorMethodType.Name,
            Type = MethodType.Accessor,
            LinesOfCode = ConvertLoc(accessorMethodType.Loc),
            Modifier = accessorMethodType.Modifier,
            AccessModifier = ConvertAccessModifier(accessorMethodType.AccessModifier),
            Modifiers = ConvertModifierToModifierList(accessorMethodType.Modifier),
            GenericParameters = new List<GenericParameterModel>(),
            CyclomaticComplexity = accessorMethodType.CyclomaticComplexity,
            Attributes = ConvertAttributes(accessorMethodType.Attributes, projectModel,
                repositoryModelConversionStrategy),
            ReturnValue = ConvertReturnValue(accessorMethodType.ReturnValue, projectModel,
                repositoryModelConversionStrategy),
            Parameters = ConvertParameters(accessorMethodType.ParameterTypes, projectModel,
                repositoryModelConversionStrategy),
            LocalVariables = ConvertLocalVariables(accessorMethodType.LocalVariableTypes, projectModel,
                repositoryModelConversionStrategy),
        };

        AddMetrics(model, accessorMethodType);
        model.LocalFunctions = ConvertLocalFunctions(entity, model, accessorMethodType.LocalFunctions, projectModel,
            repositoryModelConversionStrategy);

        return model;
    }

    private IList<MethodModel> ConvertLocalFunctions(EntityModel entityModel, MethodModel parentMethod,
        IEnumerable<IMethodTypeWithLocalFunctions> localFunctions, ProjectModel projectModel,
        IRepositoryModelConversionStrategy repositoryModelConversionStrategy)
    {
        return localFunctions.Select(localFunction =>
            {
                var localFunctionMethod = ConvertMethod(entityModel, parentMethod, localFunction, projectModel,
                    repositoryModelConversionStrategy);
                localFunctionMethod.Type = MethodType.LocalFunction;
                return localFunctionMethod;
            })
            .ToList();
    }

    private IList<LocalVariableModel> ConvertLocalVariables(IEnumerable<ILocalVariableType> localVariableTypes,
        ProjectModel projectModel, IRepositoryModelConversionStrategy repositoryModelConversionStrategy)
    {
        return localVariableTypes.Select(localVariableType => new LocalVariableModel
        {
            Type = ConvertEntityType(localVariableType.Type, projectModel, repositoryModelConversionStrategy),
            Name = localVariableType.Name,
            Modifier = localVariableType.Modifier,
        }).ToList();
    }

    #endregion

    #region MethodCalls FieldAccesses

    private void PopulateModelWithMethodReferencesAndFieldAccesses(HoneydewRepositoryModel repositoryModel,
        RepositoryModel referenceRepositoryModel)
    {
        for (var projectIndex = 0; projectIndex < referenceRepositoryModel.Projects.Count; projectIndex++)
        {
            var projectModel = referenceRepositoryModel.Projects[projectIndex];
            var repositoryModelConversionStrategy = GetRepositoryModelConversionStrategy(projectModel.Language);

            for (var compilationUnitIndex = 0;
                 compilationUnitIndex < projectModel.Files.Count;
                 compilationUnitIndex++)
            {
                var compilationUnit = projectModel.Files[compilationUnitIndex];
                var compilationUnitType =
                    repositoryModel.Projects[projectIndex].CompilationUnits[compilationUnitIndex];

                foreach (var entityModel in compilationUnit.Entities)
                {
                    Log(
                        $"ReferenceModel - Populating Method Calls and Field Accesses for {entityModel.Name} from {entityModel.FilePath}");

                    switch (entityModel)
                    {
                        case ClassModel classModel:
                        {
                            var classType =
                                compilationUnitType.ClassTypes.FirstOrDefault(c =>
                                    repositoryModelConversionStrategy.IsClassModel(c) && c.Name == classModel.Name &&
                                    repositoryModelConversionStrategy.MethodCount(c) == classModel.Methods.Count &&
                                    repositoryModelConversionStrategy.ConstructorCount(c) ==
                                    classModel.Constructors.Count);

                            if (classType is not IMembersClassType membersClassType)
                            {
                                continue;
                            }

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

                            if (classType is not IPropertyMembersClassType propertyMembersClassType)
                            {
                                continue;
                            }

                            if (propertyMembersClassType.Properties.Count != classModel.Properties.Count)
                            {
                                continue;
                            }

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
                            var classType =
                                compilationUnitType.ClassTypes.FirstOrDefault(c =>
                                    repositoryModelConversionStrategy.IsInterfaceModel(c) &&
                                    c.Name == interfaceModel.Name &&
                                    repositoryModelConversionStrategy.MethodCount(c) == interfaceModel.Methods.Count);

                            if (classType is not IMembersClassType membersClassType)
                            {
                                continue;
                            }


                            for (var methodIndex = 0; methodIndex < membersClassType.Methods.Count; methodIndex++)
                            {
                                var methodModel = interfaceModel.Methods[methodIndex];
                                var methodType = membersClassType.Methods[methodIndex];

                                ConvertFieldAccesses(methodType.AccessedFields, methodModel);
                                ConvertOutgoingCalls(methodModel, methodType);
                            }


                            if (classType is not IPropertyMembersClassType propertyMembersClassType)
                            {
                                continue;
                            }

                            if (propertyMembersClassType.Properties.Count != interfaceModel.Properties.Count)
                            {
                                continue;
                            }

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
                        var fieldAccess = new FieldAccess
                        {
                            Field = GetFieldReference(accessedField, projectModel, repositoryModelConversionStrategy),
                            Caller = methodModel,
                            AccessEntityType = ConvertEntityType(accessedField.LocationClassName, projectModel,
                                repositoryModelConversionStrategy),
                            AccessKind = ConvertAccessKind(accessedField.Kind),
                        };
                        methodModel.FieldAccesses.Add(fieldAccess);
                        fieldAccess.Field.Accesses.Add(fieldAccess);
                    }
                }

                void ConvertOutgoingCalls(MethodModel? methodModel, ICallingMethodsType methodType)
                {
                    if (methodModel == null)
                    {
                        return;
                    }

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

                void ConvertCalledMethods(IEnumerable<IMethodCallType> calledMethods, MethodModel callerMethodModel)
                {
                    foreach (var calledMethod in calledMethods)
                    {
                        var calledMethodReference = GetMethodReference(calledMethod, projectModel,
                            repositoryModelConversionStrategy);
                        var methodCall = new MethodCall
                        {
                            Caller = callerMethodModel,
                            Called = calledMethodReference,
                            CalledEnitityType = ConvertEntityType(calledMethod.LocationClassName, projectModel,
                                repositoryModelConversionStrategy),
                            GenericParameters = calledMethod.GenericParameters.Select(parameter =>
                                ConvertEntityType(parameter, projectModel, repositoryModelConversionStrategy)).ToList(),
                            ConcreteParameters = ConvertParameters(calledMethod.ParameterTypes, projectModel,
                                    repositoryModelConversionStrategy)
                                .Select(p => p.Type).ToList()
                        };
                        callerMethodModel.OutgoingCalls.Add(methodCall);
                        calledMethodReference.IncomingCalls.Add(methodCall);
                    }
                }
            }
        }
    }

    private static MethodModel? GetMethodReferenceByName(ClassModel classModel, string methodName,
        int genericParameterCount, IList<IParameterType> parameterTypes)
    {
        var methods = classModel.Methods
            .Concat(classModel.Constructors).ToList();
        if (classModel.Destructor != null)
        {
            methods.Add(classModel.Destructor);
        }

        foreach (var methodModel in methods)
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
                continue;
            }

            var allParametersMatch = true;
            var methodModelParameters = methodModel.Parameters;
            for (var i = 0; i < methodModelParameters.Count; i++)
            {
                // TODO maybe we should do a fuzzy match, in case the type is ? (which happens if the analysed solution has not been built before)
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

    private FieldModel GetFieldReference(AccessedField accessedField, ProjectModel projectModel,
        IRepositoryModelConversionStrategy repositoryModelConversionStrategy)
    {
        var locationClassModelPossibilities = SearchEntityByName(accessedField.LocationClassName, projectModel,
                repositoryModelConversionStrategy)
            .Where(classModel => classModel is ClassModel)
            .Cast<ClassModel>();
        foreach (var locationClassModel in locationClassModelPossibilities)
        {
            var fieldReference = locationClassModel.Fields.FirstOrDefault(field => field.Name == accessedField.Name);
            if (fieldReference != null)
            {
                return fieldReference;
            }

            fieldReference =
                locationClassModel.Properties.FirstOrDefault(property => property.Name == accessedField.Name);
            if (fieldReference != null)
            {
                return fieldReference;
            }
        }

        var definitionClassModelPossibilities = SearchEntityByName(accessedField.DefinitionClassName, projectModel,
                repositoryModelConversionStrategy)
            .Where(classModel => classModel is ClassModel)
            .Cast<ClassModel>();

        foreach (var definitionClassModel in definitionClassModelPossibilities)
        {
            var fieldReference = definitionClassModel.Fields.FirstOrDefault(field => field.Name == accessedField.Name);
            if (fieldReference != null)
            {
                return fieldReference;
            }

            fieldReference =
                definitionClassModel.Properties.FirstOrDefault(property => property.Name == accessedField.Name);
            if (fieldReference != null)
            {
                return fieldReference;
            }
        }

        var classModel = CreateClassModel(accessedField.LocationClassName, repositoryModelConversionStrategy);

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

    private MethodModel GetMethodReference(IMethodCallType methodCallType, ProjectModel projectModel,
        IRepositoryModelConversionStrategy repositoryModelConversionStrategy)
    {
        var locationClassModelPossibilities = SearchEntityByName(methodCallType.LocationClassName, projectModel,
                repositoryModelConversionStrategy)
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
                var methodReference =
                    GetMethodReferenceFromLocalFunction(methodCallType, classModel, repositoryModelConversionStrategy);
                if (methodReference != null)
                {
                    return methodReference;
                }
            }
        }

        var definitionClassModelPossibilities = SearchEntityByName(methodCallType.DefinitionClassName, projectModel,
                repositoryModelConversionStrategy)
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
                var methodReference =
                    GetMethodReferenceFromLocalFunction(methodCallType, classModel, repositoryModelConversionStrategy);
                if (methodReference != null)
                {
                    return methodReference;
                }
            }
        }

        var createdClassModel = CreateClassModel(methodCallType.LocationClassName, repositoryModelConversionStrategy);

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
            Parameters = methodCallType.ParameterTypes
                .Where(parameter => parameter.Type != null)
                .Select(parameter => new ParameterModel
                {
                    TypeName = parameter.Type.Name ?? "",
                    Type = ConvertEntityType(parameter.Type, projectModel, repositoryModelConversionStrategy),
                }).ToList()
        };

        createdClassModel.Methods.Add(methodModel);

        return methodModel;
    }

    private MethodModel? GetMethodReferenceFromLocalFunction(IMethodCallType methodCallType, ClassModel classModel,
        IRepositoryModelConversionStrategy repositoryModelConversionStrategy)
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
                .Select(repositoryModelConversionStrategy.CreateParameterType)
                .ToList();
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
            var localFunctionName = GetFunctionNameWithoutGeneric(methodCallType.MethodDefinitionNames[i]);
            var nextLocalFunction = methodModelToSearchLocalFunctions.LocalFunctions.FirstOrDefault(function =>
                localFunctionName.StartsWith(GetFunctionNameWithoutGeneric(function.Name)));
            if (nextLocalFunction == null)
            {
                return null;
            }

            methodModelToSearchLocalFunctions = nextLocalFunction;
        }

        string GetFunctionNameWithoutGeneric(string name)
        {
            var indexOfAngleBracket = name.IndexOf('<');

            return indexOfAngleBracket >= 0 ? name[..indexOfAngleBracket] : name;
        }

        return methodModelToSearchLocalFunctions.LocalFunctions.FirstOrDefault(function =>
            function.Name == methodCallType.Name);
    }

    #endregion

    private IList<AttributeModel> ConvertAttributes(IEnumerable<IAttributeType> attributeTypes,
        ProjectModel projectModel, IRepositoryModelConversionStrategy repositoryModelConversionStrategy)
    {
        return attributeTypes.Select(attributeType => new AttributeModel
            {
                Type = ConvertEntityType(attributeType.Type, projectModel, repositoryModelConversionStrategy),
                Target = ConvertAttributeTarget(attributeType.Target),
                Parameters = ConvertParameters(attributeType.ParameterTypes, projectModel,
                    repositoryModelConversionStrategy),
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
        ProjectModel projectModel, IRepositoryModelConversionStrategy repositoryModelConversionStrategy)
    {
        var parameters = new List<ParameterModel>();

        foreach (var parameterType in parameterTypes)
        {
            if (parameterType.Type == null)
            {
                continue;
            }

            var parameterModel = new ParameterModel
            {
                TypeName = parameterType.Type.Name ?? "",
                Type = ConvertEntityType(parameterType.Type, projectModel, repositoryModelConversionStrategy),
                Attributes = ConvertAttributes(parameterType.Attributes, projectModel,
                    repositoryModelConversionStrategy),
                Modifier = ConvertParameterModifier(parameterType.Modifier),
                DefaultValue = parameterType.DefaultValue,
            };

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

    private IList<GenericParameterModel> ConvertGenericParameters(IEnumerable<IGenericParameterType> genericParameters,
        ProjectModel projectModel, IRepositoryModelConversionStrategy repositoryModelConversionStrategy)
    {
        return genericParameters.Select(parameterType => new GenericParameterModel
            {
                Name = parameterType.Name,
                Modifier = ConvertGenericParameterModifier(parameterType.Modifier),
                Constraints = parameterType.Constraints
                    .Select(c => ConvertEntityType(c, projectModel, repositoryModelConversionStrategy)).ToList(),
                Attributes = ConvertAttributes(parameterType.Attributes, projectModel,
                    repositoryModelConversionStrategy),
            })
            .ToList();
    }

    private static GenericParameterModifier ConvertGenericParameterModifier(string modifier)
    {
        return Enum.TryParse<GenericParameterModifier>(modifier, true, out var result)
            ? result
            : GenericParameterModifier.None;
    }

    private EntityType ConvertEntityType(string typeName, ProjectModel projectModel,
        IRepositoryModelConversionStrategy repositoryModelConversionStrategy)
    {
        return ConvertEntityType(repositoryModelConversionStrategy.CreateEntityTypeModel(typeName), projectModel,
            repositoryModelConversionStrategy);
    }

    private EntityType ConvertEntityType(IEntityType type, ProjectModel projectModel,
        IRepositoryModelConversionStrategy repositoryModelConversionStrategy)
    {
        var entityModel =
            (type.FullType == null
                ? SearchEntityByName(type.Name, projectModel, repositoryModelConversionStrategy).FirstOrDefault()
                : SearchEntityByName(type.FullType.Name, type.FullType.ContainedTypes.Count, projectModel)
                    .FirstOrDefault()
            ) ?? CreateClassModel(type.Name, repositoryModelConversionStrategy);

        var entityType = new EntityType
        {
            Name = type.Name.TrimEnd('?'),
            Entity = entityModel,
            IsNullable = type.FullType?.IsNullable ?? type.Name.EndsWith('?'),
            GenericTypes = type.FullType == null
                ? new List<EntityType>()
                : ConvertGeneric(type.FullType.ContainedTypes, projectModel, repositoryModelConversionStrategy),
        };

        return entityType;
    }

    private IList<EntityType> ConvertGeneric(IEnumerable<GenericType> genericTypes, ProjectModel projectModel,
        IRepositoryModelConversionStrategy repositoryModelConversionStrategy)
    {
        var entityTypes = new List<EntityType>();

        foreach (var genericType in genericTypes)
        {
            var entityModel = SearchEntityByName(genericType.Name, genericType.ContainedTypes.Count, projectModel)
                                  .FirstOrDefault() ??
                              CreateClassModel(genericType.Name, repositoryModelConversionStrategy);

            var entityType = new EntityType
            {
                Name = genericType.Name,
                IsNullable = genericType.IsNullable,
                Entity = entityModel,
                GenericTypes = ConvertGeneric(genericType.ContainedTypes, projectModel,
                    repositoryModelConversionStrategy),
            };

            entityTypes.Add(entityType);
        }

        return entityTypes;
    }

    private ClassModel CreateClassModel(string className,
        IRepositoryModelConversionStrategy repositoryModelConversionStrategy)
    {
        var entityTypeModel = repositoryModelConversionStrategy.CreateEntityTypeModel(className);

        var entityName = entityTypeModel.FullType.Name;
        var genericParameterCount = entityTypeModel.FullType.ContainedTypes.Count;
        if (_generatedTypes.TryGetValue((entityName, genericParameterCount), out var classModel))
        {
            return classModel;
        }

        if (_generatedTypes.TryGetValue((className, genericParameterCount), out classModel))
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
            // namespace ? todo ask someone
        };
        var isPrimitive = repositoryModelConversionStrategy.IsPrimitive(classModel.Name);
        classModel.IsPrimitive = isPrimitive;
        classModel.IsExternal = !isPrimitive;

        _generatedTypes.Add((entityName, genericParameterCount), classModel);

        return classModel;
    }

    private void Log(string message)
    {
        _logger.Log(message);
        _progressLogger.Log(message);
    }

    private IRepositoryModelConversionStrategy GetRepositoryModelConversionStrategy(string projectLanguage)
    {
        switch (projectLanguage)
        {
            case "Visual Basic":
                return new VisualBasicRepositoryModelConversionStrategy();

            default:
                // case "C#":
                return new CSharpRepositoryModelConversionStrategy();
        }
    }
}

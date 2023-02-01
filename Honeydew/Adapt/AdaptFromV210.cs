using Honeydew.Logging;
using Honeydew.ModelAdapters.V2._1._0.CSharp;
using Honeydew.Models;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;

namespace Honeydew.Adapt;

public static class AdaptFromV210
{
    public static RepositoryModel Adapt(RepositoryModel_V210 repositoryModelV210, ILogger logger,
        IProgressLogger progressLogger)
    {
        logger.Log("Adapting V2.1.0 RepositoryModel to latest version");
        progressLogger.Log("Adapting V2.1.0 RepositoryModel to latest version");

        var repositoryModel = new RepositoryModel
        {
            Solutions = new List<SolutionModel>(),
            Projects = new List<ProjectModel>()
        };

        foreach (var solutionModelV210 in repositoryModelV210.Solutions)
        {
            repositoryModel.Solutions.Add(Adapt(solutionModelV210, logger, progressLogger));
        }

        foreach (var projectModelV210 in repositoryModelV210.Projects)
        {
            repositoryModel.Projects.Add(Adapt(projectModelV210, logger, progressLogger));
        }

        return repositoryModel;
    }


    private static SolutionModel Adapt(SolutionModel_V210 solutionModelV210, ILogger logger,
        IProgressLogger progressLogger)
    {
        logger.Log($"Adapting {solutionModelV210.FilePath} V2.1.0 SolutionModel to latest version");
        progressLogger.Log($"Adapting {solutionModelV210.FilePath} V2.1.0 SolutionModel to latest version");

        return new SolutionModel
        {
            FilePath = solutionModelV210.FilePath,
            ProjectsPaths = solutionModelV210.ProjectsPaths,
        };
    }


    private static ProjectModel Adapt(ProjectModel_V210 projectModelV210, ILogger logger,
        IProgressLogger progressLogger)
    {
        logger.Log($"Adapting {projectModelV210.FilePath} V2.1.0 ProjectModel to latest version");
        progressLogger.Log($"Adapting {projectModelV210.FilePath} V2.1.0 ProjectModel to latest version");

        return new ProjectModel
        {
            Language = "C#",
            FilePath = projectModelV210.FilePath,
            Name = projectModelV210.Name,
            ProjectReferences = projectModelV210.ProjectReferences,
            Namespaces = projectModelV210.Namespaces.Select(n => Adapt(n, logger, progressLogger)).ToList(),
            CompilationUnits = projectModelV210.CompilationUnits.Select(c => Adapt(c, logger, progressLogger)).ToList(),
            Metadata = new Dictionary<string, Dictionary<string, string>>()
        };
    }

    private static NamespaceModel Adapt(NamespaceModel_V210 namespaceModelV210, ILogger logger,
        IProgressLogger progressLogger)
    {
        logger.Log($"Adapting {namespaceModelV210.Name} V2.1.0 NamespaceModel to latest version");
        progressLogger.Log($"Adapting {namespaceModelV210.Name} V2.1.0 NamespaceModel to latest version");

        return new NamespaceModel
        {
            Name = namespaceModelV210.Name,
            ClassNames = namespaceModelV210.ClassNames
        };
    }

    private static ICompilationUnitType Adapt(CompilationUnitModel_V210 compilationUnitModelV210, ILogger logger,
        IProgressLogger progressLogger)
    {
        logger.Log($"Adapting {compilationUnitModelV210.FilePath} V2.1.0 CompilationUnitModel to latest version");
        progressLogger.Log(
            $"Adapting {compilationUnitModelV210.FilePath} V2.1.0 CompilationUnitModel to latest version");


        return new CSharpCompilationUnitModel
        {
            FilePath = compilationUnitModelV210.FilePath,
            Loc = Adapt(compilationUnitModelV210.Loc),
            Imports = compilationUnitModelV210.Imports.Select(Adapt).ToList(),
            Metrics = compilationUnitModelV210.Metrics.Select(Adapt).ToList(),
            ClassTypes = compilationUnitModelV210.ClassTypes.Select(c => Adapt(c, logger, progressLogger)).ToList(),
        };
    }

    private static IClassType Adapt(IClassType_V210 classTypeV210, ILogger logger, IProgressLogger progressLogger)
    {
        logger.Log($"Adapting {classTypeV210.Name} V2.1.0 ClassType to latest version");
        progressLogger.Log($"Adapting {classTypeV210.Name} V2.1.0 ClassType to latest version");

        return classTypeV210.ClassType switch
        {
            "delegate" => AdaptDelegate(classTypeV210),
            "enum" => AdaptEnum(classTypeV210),
            _ => AdaptClass(classTypeV210)
        };
    }

    private static IConstructorType Adapt(ConstructorModel_V210 constructorModelV210)
    {
        return new CSharpConstructorModel
        {
            Name = constructorModelV210.Name,
            Attributes = constructorModelV210.Attributes.Select(Adapt).ToList(),
            Loc = Adapt(constructorModelV210.Loc),
            Metrics = constructorModelV210.Metrics.Select(Adapt).ToList(),
            Modifier = constructorModelV210.Modifier,
            AccessModifier = constructorModelV210.Modifier,
            ParameterTypes = constructorModelV210.ParameterTypes.Select(Adapt).ToList(),
            AccessedFields = constructorModelV210.AccessedFields.Select(Adapt).ToList(),
            CalledMethods = constructorModelV210.CalledMethods.Select(Adapt).ToList(),
            CyclomaticComplexity = constructorModelV210.CyclomaticComplexity,
            LocalFunctions = constructorModelV210.LocalFunctions.Select(Adapt).ToList(),
            LocalVariableTypes = constructorModelV210.LocalVariableTypes.Select(Adapt).ToList(),
        };
    }

    private static ILocalVariableType Adapt(LocalVariableModel_V210 localVariableModelV210)
    {
        return new CSharpLocalVariableModel
        {
            IsNullable = localVariableModelV210.IsNullable,
            Modifier = localVariableModelV210.Modifier,
            Type = Adapt(localVariableModelV210.Type),
            Name = $"lv-{Guid.NewGuid()}"
        };
    }

    private static IMethodTypeWithLocalFunctions Adapt(MethodModel_V210 methodModelV210)
    {
        return new CSharpMethodModel
        {
            Name = methodModelV210.Name,
            Modifier = methodModelV210.Name,
            AccessModifier = methodModelV210.AccessModifier,
            Loc = Adapt(methodModelV210.Loc),
            Metrics = methodModelV210.Metrics.Select(Adapt).ToList(),
            Attributes = methodModelV210.Attributes.Select(Adapt).ToList(),
            AccessedFields = methodModelV210.AccessedFields.Select(Adapt).ToList(),
            CalledMethods = methodModelV210.CalledMethods.Select(Adapt).ToList(),
            CyclomaticComplexity = methodModelV210.CyclomaticComplexity,
            ReturnValue = Adapt(methodModelV210.ReturnValue),
            GenericParameters = methodModelV210.GenericParameters.Select(Adapt).ToList(),
            ParameterTypes = methodModelV210.ParameterTypes.Select(Adapt).ToList(),
            LocalVariableTypes = methodModelV210.LocalVariableTypes.Select(Adapt).ToList(),
            LocalFunctions = methodModelV210.LocalFunctions.Select(Adapt).ToList(),
        };
    }

    private static IMethodCallType Adapt(MethodCallModel_V210 methodCallModelV210)
    {
        return new CSharpMethodCallModel
        {
            Name = methodCallModelV210.Name,
            GenericParameters = new List<IEntityType>(),
            ParameterTypes = methodCallModelV210.ParameterTypes.Select(Adapt).ToList(),
            DefinitionClassName = methodCallModelV210.ContainingTypeName,
            LocationClassName = methodCallModelV210.ContainingTypeName,
            MethodDefinitionNames = new List<string>()
        };
    }

    private static AccessedField Adapt(AccessedField_V210 accessedFieldV210)
    {
        return new AccessedField
        {
            Name = accessedFieldV210.Name,
            Kind = accessedFieldV210.Kind == AccessedField_V210.AccessKind.Getter
                ? AccessedField.AccessKind.Getter
                : AccessedField.AccessKind.Setter,
            DefinitionClassName = accessedFieldV210.ContainingTypeName,
            LocationClassName = accessedFieldV210.ContainingTypeName
        };
    }

    private static IDestructorType? Adapt(DestructorModel_V210? destructorModelV210)
    {
        if (destructorModelV210 is null)
        {
            return null;
        }

        return new CSharpDestructorModel
        {
            Name = destructorModelV210.Name,
            AccessModifier = destructorModelV210.AccessModifier,
            Modifier = destructorModelV210.Modifier,
            Loc = Adapt(destructorModelV210.Loc),
            Attributes = destructorModelV210.Attributes.Select(Adapt).ToList(),
            Metrics = destructorModelV210.Metrics.Select(Adapt).ToList(),
            AccessedFields = destructorModelV210.AccessedFields.Select(Adapt).ToList(),
            CalledMethods = destructorModelV210.CalledMethods.Select(Adapt).ToList(),
            CyclomaticComplexity = destructorModelV210.CyclomaticComplexity,
            LocalFunctions = destructorModelV210.LocalFunctions.Select(Adapt).ToList(),
            ParameterTypes = destructorModelV210.ParameterTypes.Select(Adapt).ToList(),
            LocalVariableTypes = destructorModelV210.LocalVariableTypes.Select(Adapt).ToList(),
        };
    }

    private static IFieldType Adapt(FieldModel_V210 fieldModelV210)
    {
        return new CSharpFieldModel
        {
            Name = fieldModelV210.Name,
            IsEvent = fieldModelV210.IsEvent,
            IsNullable = fieldModelV210.IsNullable,
            Modifier = fieldModelV210.Modifier,
            AccessModifier = fieldModelV210.AccessModifier,
            Type = Adapt(fieldModelV210.Type),
            Metrics = fieldModelV210.Metrics.Select(Adapt).ToList(),
            Attributes = fieldModelV210.Attributes.Select(Adapt).ToList(),
        };
    }

    private static IPropertyType Adapt(PropertyModel_V210 propertyModelV210)
    {
        return new CSharpPropertyModel
        {
            Name = propertyModelV210.Name,
            IsEvent = propertyModelV210.IsEvent,
            IsNullable = propertyModelV210.IsNullable,
            Modifier = propertyModelV210.Modifier,
            AccessModifier = propertyModelV210.AccessModifier,
            Type = Adapt(propertyModelV210.Type),
            Metrics = propertyModelV210.Metrics.Select(Adapt).ToList(),
            Attributes = propertyModelV210.Attributes.Select(Adapt).ToList(),
            Loc = Adapt(propertyModelV210.Loc),
            CyclomaticComplexity = propertyModelV210.CyclomaticComplexity,
            Accessors = propertyModelV210.Accessors.Select(AdaptAccessor).ToList(),
        };
    }

    private static IReturnValueType Adapt(ReturnValueModel_V210? returnValueModelV210)
    {
        if (returnValueModelV210 is null)
        {
            return new CSharpReturnValueModel
            {
                Type = new CSharpEntityTypeModel
                {
                    Name = "void",
                    FullType = new GenericType
                    {
                        Name = "void"
                    }
                },
                Attributes = new List<IAttributeType>(),
                Modifier = "",
            };
        }

        return new CSharpReturnValueModel
        {
            Type = Adapt(returnValueModelV210.Type),
            Attributes = returnValueModelV210.Attributes.Select(Adapt).ToList(),
            Modifier = returnValueModelV210.Modifier,
            IsNullable = returnValueModelV210.IsNullable
        };
    }

    private static IBaseType Adapt(BaseTypeModel_V210 baseTypeModelV210)
    {
        return new CSharpBaseTypeModel
        {
            Kind = baseTypeModelV210.Kind,
            Type = Adapt(baseTypeModelV210.Type),
        };
    }

    private static IGenericParameterType Adapt(GenericParameterModel_V210 genericParameterModelV210)
    {
        return new CSharpGenericParameterModel
        {
            Name = genericParameterModelV210.Name,
            Modifier = genericParameterModelV210.Modifier,
            Attributes = genericParameterModelV210.Attributes.Select(Adapt).ToList(),
            Constraints = genericParameterModelV210.Constraints.Select(Adapt).ToList(),
        };
    }

    private static IAttributeType Adapt(AttributeModel_V210 attributeModelV210)
    {
        return new CSharpAttributeModel
        {
            Name = attributeModelV210.Name,
            Target = attributeModelV210.Target,
            Type = Adapt(attributeModelV210.Type),
            ParameterTypes = attributeModelV210.ParameterTypes.Select(Adapt).ToList(),
        };
    }

    private static IParameterType Adapt(ParameterModel_V210 parameterModelV210)
    {
        return new CSharpParameterModel
        {
            IsNullable = parameterModelV210.IsNullable,
            Modifier = parameterModelV210.Modifier,
            DefaultValue = parameterModelV210.DefaultValue,
            Type = Adapt(parameterModelV210.Type),
            Attributes = parameterModelV210.Attributes.Select(Adapt).ToList(),
        };
    }

    private static IEntityType Adapt(EntityTypeModel_V210 entityTypeModelV210)
    {
        return new CSharpEntityTypeModel
        {
            Name = entityTypeModelV210.Name,
            IsExtern = entityTypeModelV210.IsExtern,
            FullType = Adapt(entityTypeModelV210.FullType),
        };
    }

    private static GenericType Adapt(GenericType_V210? genericTypeV210)
    {
        if (genericTypeV210 is null)
        {
            return new GenericType
            {
                Name = "<unknown>",
            };
        }

        return new GenericType
        {
            Name = genericTypeV210.Name,
            IsNullable = genericTypeV210.IsNullable,
            ContainedTypes = genericTypeV210.ContainedTypes.Select(Adapt).ToList()
        };
    }

    private static MetricModel Adapt(MetricModel_V210 metricModelV210)
    {
        return new MetricModel($"metric-{metricModelV210.ExtractorName}", metricModelV210.ExtractorName,
            metricModelV210.ValueType, metricModelV210.Value);
    }

    private static IImportType Adapt(UsingModel_V210 usingModelV210)
    {
        return new CSharpUsingModel
        {
            Name = usingModelV210.Name,
            IsStatic = usingModelV210.IsStatic,
            Alias = usingModelV210.Alias,
            AliasType = usingModelV210.AliasType
        };
    }

    private static LinesOfCode Adapt(LinesOfCode_V210 compilationUnitModelV210)
    {
        return new LinesOfCode
        {
            CommentedLines = compilationUnitModelV210.CommentedLines,
            EmptyLines = compilationUnitModelV210.EmptyLines,
            SourceLines = compilationUnitModelV210.SourceLines,
        };
    }


    private static IAccessorMethodType AdaptAccessor(MethodModel_V210 methodModelV210)
    {
        return new CSharpAccessorMethodModel
        {
            Name = methodModelV210.Name,
            Loc = Adapt(methodModelV210.Loc),
            Attributes = methodModelV210.Attributes.Select(Adapt).ToList(),
            AccessModifier = methodModelV210.AccessModifier,
            Modifier = methodModelV210.Modifier,
            Metrics = methodModelV210.Metrics.Select(Adapt).ToList(),
            AccessedFields = methodModelV210.AccessedFields.Select(Adapt).ToList(),
            CalledMethods = methodModelV210.CalledMethods.Select(Adapt).ToList(),
            CyclomaticComplexity = methodModelV210.CyclomaticComplexity,
            LocalFunctions = methodModelV210.LocalFunctions.Select(Adapt).ToList(),
            ParameterTypes = methodModelV210.ParameterTypes.Select(Adapt).ToList(),
            ReturnValue = Adapt(methodModelV210.ReturnValue),
            LocalVariableTypes = methodModelV210.LocalVariableTypes.Select(Adapt).ToList(),
        };
    }

    private static IMethodType AdaptMethod(MethodModel_V210 methodModelV210)
    {
        return Adapt(methodModelV210);
    }

    private static IClassType AdaptClass(IClassType_V210 classTypeV210)
    {
        var classModelV210 = (ClassModel_V210)classTypeV210;

        return new CSharpClassModel
        {
            ClassType = classModelV210.ClassType,
            Name = classModelV210.Name,
            FilePath = classModelV210.FilePath,
            Loc = Adapt(classModelV210.Loc),
            Modifier = classModelV210.Modifier,
            AccessModifier = classModelV210.AccessModifier,
            Imports = classModelV210.Imports.Select(Adapt).ToList(),
            Metrics = classModelV210.Metrics.Select(Adapt).ToList(),
            ContainingNamespaceName = ExtractNamespace(classModelV210.ContainingTypeName),
            ContainingClassName = classModelV210.ContainingTypeName,
            Attributes = classModelV210.Attributes.Select(Adapt).ToList(),
            BaseTypes = classModelV210.BaseTypes.Select(Adapt).ToList(),
            GenericParameters = classModelV210.GenericParameters.Select(Adapt).ToList(),
            Constructors = classModelV210.Constructors.Select(Adapt).ToList(),
            Methods = classModelV210.Methods.Select(AdaptMethod).ToList(),
            Destructor = Adapt(classModelV210.Destructor),
            Fields = classModelV210.Fields.Select(Adapt).ToList(),
            Properties = classModelV210.Properties.Select(Adapt).ToList(),
        };
    }

    private static IClassType AdaptEnum(IClassType_V210 classTypeV210)
    {
        var classModelV210 = (ClassModel_V210)classTypeV210;

        return new CSharpEnumModel
        {
            ClassType = classModelV210.ClassType,
            Name = classModelV210.Name,
            FilePath = classModelV210.FilePath,
            Loc = Adapt(classModelV210.Loc),
            Modifier = classModelV210.Modifier,
            AccessModifier = classModelV210.AccessModifier,
            Imports = classModelV210.Imports.Select(Adapt).ToList(),
            Metrics = classModelV210.Metrics.Select(Adapt).ToList(),
            ContainingNamespaceName = ExtractNamespace(classModelV210.ContainingTypeName),
            ContainingClassName = classModelV210.ContainingTypeName,
            Attributes = classModelV210.Attributes.Select(Adapt).ToList(),
            BaseTypes = classModelV210.BaseTypes.Select(Adapt).ToList(),
            Type = "int",
            Labels = new List<IEnumLabelType>()
        };
    }

    private static IClassType AdaptDelegate(IClassType_V210 classTypeV210)
    {
        var delegateModelV210 = (DelegateModel_V210)classTypeV210;

        return new CSharpDelegateModel
        {
            ClassType = delegateModelV210.ClassType,
            Name = delegateModelV210.Name,
            FilePath = delegateModelV210.FilePath,
            Loc = new LinesOfCode { SourceLines = 1 },
            Modifier = delegateModelV210.Modifier,
            AccessModifier = delegateModelV210.AccessModifier,
            Imports = delegateModelV210.Imports.Select(Adapt).ToList(),
            Metrics = delegateModelV210.Metrics.Select(Adapt).ToList(),
            ContainingNamespaceName = ExtractNamespace(delegateModelV210.ContainingTypeName),
            ContainingClassName = delegateModelV210.ContainingTypeName,
            Attributes = delegateModelV210.Attributes.Select(Adapt).ToList(),
            ParameterTypes = delegateModelV210.ParameterTypes.Select(Adapt).ToList(),
            GenericParameters = delegateModelV210.GenericParameters.Select(Adapt).ToList(),
            BaseTypes = delegateModelV210.BaseTypes.Select(Adapt).ToList(),
            ReturnValue = Adapt(delegateModelV210.ReturnValue),
        };
    }

    private static string ExtractNamespace(string name)
    {
        var lastIndexOf = name.LastIndexOf('.');
        return lastIndexOf == -1 ? "" : name[..lastIndexOf];
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using HoneydewModels.Exporters;
using HoneydewModels.Reference;

namespace Honeydew.Scripts;

/// <summary>
/// Requires the following arguments:
/// <list type="bullet">
///     <item>
///         <description>outputPath</description>
///     </item>
///     <item>
///         <description>testingStuffOutputName</description>
///     </item>
///     <item>
///         <description>referenceRepositoryModel</description>
///     </item>
/// </list>
/// </summary>
public class TestingStuffExportScript : Script
{
    private readonly JsonModelExporter _repositoryExporter;

    public TestingStuffExportScript(JsonModelExporter repositoryExporter)
    {
        _repositoryExporter = repositoryExporter;
    }

    public override void Run(Dictionary<string, object> arguments)
    {
        var outputPath = VerifyArgument<string>(arguments, "outputPath");
        var outputName = VerifyArgument<string>(arguments, "testingStuffOutputName");
        var repositoryModel = VerifyArgument<RepositoryModel>(arguments, "referenceRepositoryModel");

        var solutions =
            repositoryModel.Solutions.Select(solution =>
                new Solution(solution.FilePath, solution.Projects.Select(project => project.FilePath).ToHashSet()));

        var projects = repositoryModel.Projects.Select(project => new Project(
            name: project.Name, path: project.FilePath,
            externalReferences: project.ExternalProjectReferences,
            projectReferences: project.ProjectReferences.Select(@ref => @ref.Name).ToHashSet(),
            files: project.Files.Select(file => new File(ExtractFileName(file.FilePath), file.FilePath,
                    file.Classes.GroupBy(@class => @class.Namespace.FullName)
                        .Select(grouping => new Namespace(grouping.Key,
                            grouping.Select(@class => new Class(
                                    name: @class.Name, usingStatements: @class.Imports.Select(i => i.Name).ToHashSet(),
                                    attributes: @class.Attributes.Select(a => a.Name).ToHashSet(),
                                    usedClasses: GetUsedClasses(@class),
                                    methods: @class.Methods.Select(method => new Method(name: method.Name,
                                            type: method.MethodType,
                                            attributes: method.Attributes.Select(a => a.Name).ToHashSet(),
                                            modifiers: method.Modifier.Split(' ')
                                                .Concat(new[] { method.AccessModifier })
                                                .ToHashSet(),
                                            callers: GetCallers(method),
                                            calledMethods: method.CalledMethods.Select(GenerateMethodId).ToHashSet()))
                                        .Concat(@class.Properties.SelectMany(prop => prop.Accessors.Select(accessor =>
                                            new Method(name: accessor.Name, type: GetAccessorType(accessor),
                                                attributes: accessor.Attributes.Select(a => a.Name).ToHashSet(),
                                                modifiers: accessor.Modifier.Split(' ')
                                                    .Concat(new[] { accessor.AccessModifier })
                                                    .ToHashSet(),
                                                callers: GetCallers(accessor),
                                                calledMethods: accessor.CalledMethods.Select(GenerateMethodId)
                                                    .ToHashSet()
                                            ))))
                                        .ToHashSet()))
                                .ToHashSet()))
                        .ToHashSet()))
                .ToHashSet()
        ));

        var root = new { solutions, projects };
        _repositoryExporter.Export(Path.Combine(outputPath, outputName), root);
    }

    private static ISet<string> GetCallers(MethodModel methodModel)
    {
        return new HashSet<string>();
    }

    private static string GetAccessorType(MethodModel accessor)
    {
        return accessor.Name;
    }

    private static string GenerateMethodId(MethodModel methodModel)
    {
        return
            $"{methodModel.Class?.FilePath}->{methodModel.Class?.Name}@{methodModel.Name}#{string.Join(',', methodModel.Parameters.Select(p => p.Type.Name))}";
    }

    private static ISet<string> GetUsedClasses(ClassModel classModel)
    {
        return classModel.Fields.SelectMany(field => GetGenericNames(field.Class.Type))
            .Concat(classModel.BaseTypes.SelectMany(baseType => GetGenericNames(baseType.Type)))
            .Concat(classModel.Attributes.Select(attribute => attribute.Name))
            .Concat(classModel.Methods.SelectMany(method =>
                method.Parameters.SelectMany(param => GetGenericNames(param.Type))
                    .Concat(method.ReturnValue != null
                        ? GetGenericNames(method.ReturnValue.Type)
                        : new List<string>())
                    .Concat(method.LocalVariables.SelectMany(v => GetGenericNames(v.Type)))
                    .Concat(method.LocalFunctions.SelectMany(GetUsedClassesFromLocalFunctions))
            ))
            .ToHashSet();
    }

    private static IEnumerable<string> GetUsedClassesFromLocalFunctions(MethodModel methodModel)
    {
        return methodModel.Parameters.SelectMany(param => GetGenericNames(param.Type))
            .Concat(methodModel.ReturnValue != null
                ? GetGenericNames(methodModel.ReturnValue.Type)
                : new List<string>())
            .Concat(methodModel.LocalVariables.SelectMany(v => GetGenericNames(v.Type)))
            .Concat(methodModel.LocalFunctions.SelectMany(GetUsedClassesFromLocalFunctions));
    }

    private static IEnumerable<string> GetGenericNames(EntityType entityType)
    {
        return entityType == null ? new List<string>() : GetGenericNames(entityType.FullType);
    }

    private static IEnumerable<string> GetGenericNames(GenericType genericType)
    {
        return genericType.ContainedTypes.SelectMany(GetGenericNames)
            .Concat(GetRefName(genericType.Reference));

        IEnumerable<string> GetRefName(ReferenceEntity referenceEntity)
        {
            return referenceEntity switch
            {
                ClassModel classModel => new List<string> { classModel.Name },
                DelegateModel delegateModel => new List<string> { delegateModel.Name },
                _ => new List<string>()
            };
        }
    }

    private static string ExtractFileName(string filePath)
    {
        return Path.GetFileName(filePath);
    }
}

internal record Solution(string path, ISet<string> projects);

internal record Project(string name, string path, ISet<File> files, ISet<string> projectReferences,
    ISet<string> externalReferences);

internal record File(string name, string path, ISet<Namespace> namespaces);

internal record Namespace(string name, ISet<Class> classes);

internal record Class(string name, ISet<string> usingStatements, ISet<string> attributes, ISet<string> usedClasses,
    ISet<Method> methods);

internal record Method(string name, string type, ISet<string> attributes, ISet<string> modifiers, ISet<string> callers,
    ISet<string> calledMethods);

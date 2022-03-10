using System.Collections.Generic;
using System.IO;
using System.Linq;
using HoneydewModels.Exporters;
using HoneydewScriptBeePlugin.Models;

namespace Honeydew.Scripts;

/// <summary>
/// Requires the following arguments:
/// <list type="bullet">
///     <item>
///         <description>outputPath</description>
///     </item>
///     <item>
///         <description>spektrumOutputName</description>
///     </item>
///     <item>
///         <description>referenceRepositoryModel</description>
///     </item>
/// </list>
/// </summary>
public class SpektrumExportScript : Script
{
    private readonly JsonModelExporter _repositoryExporter;

    public SpektrumExportScript(JsonModelExporter repositoryExporter)
    {
        _repositoryExporter = repositoryExporter;
    }

    public override void Run(Dictionary<string, object> arguments)
    {
        var outputPath = VerifyArgument<string>(arguments, "outputPath");
        var outputName = VerifyArgument<string>(arguments, "spektrumOutputName");
        var repositoryModel = VerifyArgument<RepositoryModel>(arguments, "referenceRepositoryModel");

        var solutions =
            repositoryModel.Solutions.Select(solution =>
                new Solution(solution.FilePath, solution.Projects.Select(project => project.FilePath).ToHashSet()));

        var callersDictionary = new Dictionary<string, ISet<string>>();

        foreach (var methodModel in repositoryModel.Projects.SelectMany(project =>
                     project.Files.SelectMany(file =>
                         file.Entities.SelectMany(entityModel =>
                         {
                             return entityModel switch
                             {
                                 ClassModel classModel => classModel.Methods
                                     .Concat(classModel.Properties.SelectMany(p => p.Accessors))
                                     .Concat(classModel.Constructors)
                                     .Concat(classModel.Destructor == null
                                         ? new List<MethodModel>()
                                         : new List<MethodModel>
                                         {
                                             classModel.Destructor
                                         }),
                                 InterfaceModel interfaceModel => interfaceModel.Methods.Concat(
                                     interfaceModel.Properties.SelectMany(p => p.Accessors)),
                                 _ => new List<MethodModel>()
                             };
                         }))))
        {
            var methodId = GenerateMethodId(methodModel);

            foreach (var calledMethod in methodModel.OutgoingCalls)
            {
                var calledMethodId = GenerateMethodId(calledMethod);

                if (callersDictionary.TryGetValue(calledMethodId, out var list))
                {
                    list.Add(methodId);
                }
                else
                {
                    callersDictionary.Add(calledMethodId, new HashSet<string>() { methodId });
                }
            }
        }

        var projects = repositoryModel.Projects.Select(project => new Project(
            name: project.Name, path: project.FilePath,
            externalReferences: project.ExternalProjectReferences,
            projectReferences: project.ProjectReferences.Select(@ref => @ref.Name).ToHashSet(),
            files: project.Files.Select(file => new File(ExtractFileName(file.FilePath), file.FilePath,
                    file.Entities.GroupBy(@class => @class.Namespace.FullName)
                        .Select(grouping => new Namespace(grouping.Key,
                            grouping.Where(entityModel => entityModel is ClassModel or InterfaceModel).Select(
                                    entityModel =>
                                    {
                                        var classType = "";
                                        IList<MethodModel> methods = new List<MethodModel>();
                                        IList<PropertyModel> properties = new List<PropertyModel>();

                                        if (entityModel is ClassModel classModel)
                                        {
                                            classType = classModel.Type.ToString();
                                            methods = classModel.Methods
                                                .Concat(classModel.Constructors).ToList();
                                            if (classModel.Destructor != null)
                                            {
                                                methods.Add(classModel.Destructor);
                                            }

                                            properties = classModel.Properties;
                                        }
                                        else if (entityModel is InterfaceModel interfaceModel)
                                        {
                                            classType = "interface";
                                            methods = interfaceModel.Methods;
                                            properties = interfaceModel.Properties;
                                        }


                                        return new Class(
                                            name: entityModel.Name,
                                            usingStatements: entityModel.Imports.Select(i =>
                                                i.Entity == null ? i.Namespace?.Name : i.Entity.Name).ToHashSet(),
                                            type: classType,
                                            attributes: entityModel.Attributes.Select(a => a.Type.Entity.Name)
                                                .ToHashSet(),
                                            usedClasses: GetUsedClasses(entityModel),
                                            methods: methods.Select(method =>
                                                    new Method(name: GenerateMethodName(method),
                                                        type: method.Type.ToString(),
                                                        attributes: method.Attributes.Select(a => a.Type.Entity.Name)
                                                            .ToHashSet(),
                                                        modifiers: method.Modifier.Split(' ')
                                                            .Concat(new[] { method.AccessModifier.ToString() })
                                                            .Where(modifier => !string.IsNullOrWhiteSpace(modifier))
                                                            .ToHashSet(),
                                                        callers: method.IncomingCalls.Select(GenerateMethodId)
                                                            .ToHashSet(), //GetCallers(callersDictionary, method),
                                                        calledMethods: method.OutgoingCalls.Select(GenerateMethodId)
                                                            .ToHashSet()))
                                                .Concat(properties.SelectMany(prop =>
                                                    prop.Accessors.Select(accessor =>
                                                        new Method(name: GenerateMethodName(accessor),
                                                            type: GetAccessorType(accessor),
                                                            attributes: accessor.Attributes
                                                                .Select(a => a.Type.Entity.Name)
                                                                .ToHashSet(),
                                                            modifiers: accessor.Modifier.Split(' ')
                                                                .Concat(new[] { accessor.AccessModifier.ToString() })
                                                                .ToHashSet(),
                                                            callers: accessor.IncomingCalls.Select(GenerateMethodId)
                                                                .ToHashSet(), // GetCallers(callersDictionary, accessor),
                                                            calledMethods: accessor.OutgoingCalls
                                                                .Select(GenerateMethodId)
                                                                .ToHashSet()
                                                        ))))
                                                .ToHashSet());
                                    })
                                .ToHashSet()))
                        .ToHashSet()))
                .ToHashSet()
        ));

        var root = new { solutions, projects };
        _repositoryExporter.Export(Path.Combine(outputPath, outputName), root);
    }

    private static string GenerateMethodName(MethodModel methodModel)
    {
        return $"{methodModel.Name}#{string.Join(',', methodModel.Parameters.Select(p => p.Type.Name))}";
    }

    private static ISet<string> GetCallers(IDictionary<string, ISet<string>> callersDictionary,
        MethodModel methodModel)
    {
        return !callersDictionary.TryGetValue(GenerateMethodId(methodModel), out var callers)
            ? new HashSet<string>()
            : callers;
    }

    private static string GetAccessorType(MethodModel accessor)
    {
        return accessor.Name;
    }

    private static string GenerateMethodId(MethodCall methodCall)
    {
        return GenerateMethodId(methodCall.Caller);
    }

    private static string GenerateMethodId(MethodModel methodModel)
    {
        return
            $"{methodModel.Entity.FilePath}->{methodModel.Entity.Name}@{methodModel.Name}#{string.Join(',', methodModel.Parameters.Select(p => p.Type.Name))}";
    }

    private static ISet<string> GetUsedClasses(EntityModel entityModel)
    {
        return entityModel switch
        {
            ClassModel classModel => classModel.Fields.SelectMany(field => GetGenericNames(field.Type))
                .Concat(classModel.Properties.SelectMany(property => GetGenericNames(property.Type)))
                .Concat(classModel.BaseTypes.SelectMany(GetGenericNames))
                .Concat(classModel.Attributes.SelectMany(attribute => GetGenericNames(attribute.Type)))
                .Concat(classModel.Methods.SelectMany(method =>
                    method.Parameters.SelectMany(param => GetGenericNames(param.Type))
                        .Concat(method.ReturnValue != null
                            ? GetGenericNames(method.ReturnValue.Type)
                            : new List<string>())
                        .Concat(method.FieldAccesses.SelectMany(field => GetGenericNames(field.Field.Type)))
                        .Concat(method.LocalVariables.SelectMany(v => GetGenericNames(v.Type)))
                        .Concat(method.LocalFunctions.SelectMany(GetUsedClassesFromLocalFunctions))))
                .ToHashSet(),
            InterfaceModel interfaceModel => interfaceModel.BaseTypes
                .SelectMany(GetGenericNames)
                .Concat(interfaceModel.Attributes.SelectMany(attribute => GetGenericNames(attribute.Type)))
                .Concat(interfaceModel.Properties.SelectMany(property => GetGenericNames(property.Type)))
                .Concat(interfaceModel.Methods.SelectMany(method =>
                    method.Parameters.SelectMany(param => GetGenericNames(param.Type))
                        .Concat(method.ReturnValue != null
                            ? GetGenericNames(method.ReturnValue.Type)
                            : new List<string>())
                        .Concat(method.FieldAccesses.SelectMany(field => GetGenericNames(field.Field.Type)))
                        .Concat(method.LocalVariables.SelectMany(v => GetGenericNames(v.Type)))
                        .Concat(method.LocalFunctions.SelectMany(GetUsedClassesFromLocalFunctions))))
                .ToHashSet(),
            _ => new HashSet<string>()
        };
    }

    private static IEnumerable<string> GetUsedClassesFromLocalFunctions(MethodModel method)
    {
        return method.Parameters.SelectMany(param => GetGenericNames(param.Type))
            .Concat(method.ReturnValue != null
                ? GetGenericNames(method.ReturnValue.Type)
                : new List<string>())
            .Concat(method.FieldAccesses.SelectMany(field => GetGenericNames(field.Field.Type)))
            .Concat(method.LocalVariables.SelectMany(v => GetGenericNames(v.Type)))
            .Concat(method.LocalFunctions.SelectMany(GetUsedClassesFromLocalFunctions));
    }


    private static IEnumerable<string> GetGenericNames(EntityType genericType)
    {
        if (genericType == null)
        {
            return new List<string>();
        }

        return genericType.GenericTypes.SelectMany(GetGenericNames)
            .Concat(GetRefName(genericType.Entity));

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

internal record Class(string name, string type, ISet<string> usingStatements, ISet<string> attributes,
    ISet<string> usedClasses, ISet<Method> methods);

internal record Method(string name, string type, ISet<string> attributes, ISet<string> modifiers, ISet<string> callers,
    ISet<string> calledMethods);

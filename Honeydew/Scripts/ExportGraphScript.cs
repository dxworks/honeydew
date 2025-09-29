using System.Text.Json;
using Honeydew.Logging;
using Honeydew.IO.Writers;
using DxWorks.ScriptBee.Plugins.Honeydew.Models;
using static System.IO.Directory;

namespace Honeydew.Scripts;

/// <summary>
/// Exports a minimal graph artifact structure (folders + manifest) as specified in docs/graph-export-spec.md.
/// Arguments expected:
/// - outputRoot: string (e.g., results/graph_v1)
/// - referenceRepositoryModel: DxWorks.ScriptBee.Plugins.Honeydew.Models.RepositoryModel
/// - includeAttributes: bool
/// - includeImports: bool
/// - includeLocals: bool
/// </summary>
public class ExportGraphScript(ILogger logger, IProgressLogger progress) : Script
{
    // Central helpers to remove duplication when computing type references
    private record TypeRef(string language, string fullName, string typeHomeFilePath);

    private static string BuildFullName(EntityModel e)
    {
        var ns = e.Namespace?.FullName ?? string.Empty;
        return string.IsNullOrEmpty(ns) ? e.Name : $"{ns}.{e.Name}";
    }

    private static string ResolveTypeHome(
        string lang,
        string fullName,
        Dictionary<(string language, string fullName), (string name, SortedSet<string> files, bool isExternal, bool isPrimitive, string nsFullName)> typeMap,
        EntityModel? e)
    {
        if (typeMap.TryGetValue((lang, fullName), out var entry))
        {
            if (entry.files.Count > 0)
            {
                return entry.files.First();
            }

            return entry.isPrimitive ? $"primitive://{lang}" : "external://";
        }

        // Unknown in map: decide by entity primitive flag
        return (e?.IsPrimitive ?? false) ? $"primitive://{lang}" : "external://";
    }

    private static TypeRef ToTypeRef(
        string lang,
        EntityModel e,
        Dictionary<(string language, string fullName), (string name, SortedSet<string> files, bool isExternal, bool isPrimitive, string nsFullName)> typeMap)
    {
        var full = BuildFullName(e);
        var home = ResolveTypeHome(lang, full, typeMap, e);
        return new TypeRef(lang, full, home);
    }

    private static (string fullName, (string name, SortedSet<string> files, bool isExternal, bool isPrimitive, string nsFullName) entry)
        EnsureTypeInMap(
            string lang,
            EntityModel e,
            Dictionary<(string language, string fullName), (string name, SortedSet<string> files, bool isExternal, bool isPrimitive, string nsFullName)> typeMap)
    {
        var ns = e.Namespace?.FullName ?? string.Empty;
        var full = string.IsNullOrEmpty(ns) ? e.Name : $"{ns}.{e.Name}";
        if (!typeMap.TryGetValue((lang, full), out var entry))
        {
            var files = new SortedSet<string>(StringComparer.Ordinal);
            if (!string.IsNullOrEmpty(e.FilePath)) files.Add(e.FilePath);
            entry = (e.Name, files, e.IsExternal || e.IsPrimitive, e.IsPrimitive, ns);
            typeMap[(lang, full)] = entry;
        }
        return (full, entry);
    }

    public override void Run(Dictionary<string, object?> arguments)
    {
        var outputRoot = VerifyArgument<string>(arguments, "outputRoot")!;
        var referenceRepositoryModel = VerifyArgument<RepositoryModel>(arguments, "referenceRepositoryModel");
        var includeAttributes = VerifyArgument<bool>(arguments, "includeAttributes");
        var includeImports = VerifyArgument<bool>(arguments, "includeImports");
        var includeLocals = VerifyArgument<bool>(arguments, "includeLocals");

        CreateDirectory(outputRoot);
        var nodesDir = Path.Combine(outputRoot, "nodes");
        var relationsDir = Path.Combine(outputRoot, "rels");
        CreateDirectory(nodesDir);
        CreateDirectory(relationsDir);

        // Build namespaces and types (internal only in v1 step)
        var nsKeySet = new HashSet<(string language, string fullName)>();
        var nsParentEdges = new HashSet<((string lang, string parent), (string lang, string child))>();
        var typeMap =
            new Dictionary<(string language, string fullName), (string name, SortedSet<string> files, bool isExternal,
                bool isPrimitive, string nsFullName)>();
        var fileDeclares = new List<(string filePath, (string language, string fullName) typeKey)>();

        if (referenceRepositoryModel != null)
        {
            foreach (var project in referenceRepositoryModel.Projects)
            {
                var lang = project.Language;
                foreach (var file in project.Files)
                {
                    foreach (var entity in file.Entities)
                    {
                        var nsFullName = entity.Namespace?.FullName ?? string.Empty;
                        var typeFullName = string.IsNullOrEmpty(nsFullName)
                            ? entity.Name
                            : $"{nsFullName}.{entity.Name}";

                        // Track namespace and its parents for this language
                        if (!string.IsNullOrEmpty(nsFullName))
                        {
                            nsKeySet.Add((lang, nsFullName));
                            var parts = nsFullName.Split('.', StringSplitOptions.RemoveEmptyEntries);
                            for (int i = 1; i < parts.Length; i++)
                            {
                                var parent = string.Join('.', parts.Take(i));
                                var child = string.Join('.', parts.Take(i + 1));
                                nsKeySet.Add((lang, parent));
                                nsKeySet.Add((lang, child));
                                nsParentEdges.Add(((lang, parent), (lang, child)));
                            }
                        }

                        var key = (lang, typeFullName);
                        if (!typeMap.TryGetValue(key, out var entry))
                        {
                            entry = (entity.Name, new SortedSet<string>(StringComparer.Ordinal), entity.IsExternal,
                                entity.IsPrimitive, nsFullName);
                            typeMap[key] = entry;
                        }

                        entry.files.Add(file.FilePath);
                        // Merge flags
                        var mergedExternal = entry.isExternal || entity.IsExternal;
                        var mergedPrimitive = entry.isPrimitive || entity.IsPrimitive;
                        typeMap[key] = (entry.name, entry.files, mergedExternal, mergedPrimitive, nsFullName);

                        fileDeclares.Add((file.FilePath, key));
                    }
                }
            }
        }

        // Emit namespaces.jsonl
        var nsLines = nsKeySet
            .OrderBy(k => k.language).ThenBy(k => k.fullName)
            .Select(k => JsonSerializer.Serialize(new
            {
                k.language,
                k.fullName,
                name = string.IsNullOrEmpty(k.fullName) ? string.Empty : k.fullName.Split('.').Last(),
            }));
        WriteJsonl(Path.Combine(nodesDir, "namespaces.jsonl"), nsLines);

        // Ensure referenced types (fields/properties/methods/parameters/returns[/locals]) are present before writing types
        if (referenceRepositoryModel != null)
        {
            foreach (var project in referenceRepositoryModel.Projects)
            {
                var lang = project.Language;
                foreach (var file in project.Files)
                {
                    foreach (var entity in file.Entities)
                    {
                        if (entity is ClassModel cm)
                        {
                            foreach (var bt in cm.BaseTypes) AddReferencedType(bt, lang);
                            foreach (var f in cm.Fields) AddReferencedType(f.Type, lang);
                            foreach (var p in cm.Properties)
                            {
                                AddReferencedType(p.Type, lang);
                                foreach (var acc in p.Accessors)
                                {
                                    foreach (var prm in acc.Parameters) AddReferencedType(prm.Type, lang);
                                    AddReferencedType(acc.ReturnValue?.Type, lang);
                                    if (includeLocals)
                                    {
                                        foreach (var lv in acc.LocalVariables) AddReferencedType(lv.Type, lang);
                                    }
                                }
                            }

                            foreach (var m in cm.Methods)
                            {
                                foreach (var prm in m.Parameters) AddReferencedType(prm.Type, lang);
                                AddReferencedType(m.ReturnValue?.Type, lang);
                                if (includeLocals)
                                {
                                    foreach (var lv in m.LocalVariables) AddReferencedType(lv.Type, lang);
                                }
                            }

                            foreach (var c in cm.Constructors)
                            {
                                foreach (var prm in c.Parameters) AddReferencedType(prm.Type, lang);
                                if (includeLocals)
                                {
                                    foreach (var lv in c.LocalVariables) AddReferencedType(lv.Type, lang);
                                }
                            }

                            if (cm.Destructor != null)
                            {
                                foreach (var prm in cm.Destructor.Parameters) AddReferencedType(prm.Type, lang);
                                if (includeLocals)
                                {
                                    foreach (var lv in cm.Destructor.LocalVariables) AddReferencedType(lv.Type, lang);
                                }
                            }
                        }
                        else if (entity is InterfaceModel im)
                        {
                            foreach (var bt in im.BaseTypes) AddReferencedType(bt, lang);
                            foreach (var p in im.Properties)
                            {
                                AddReferencedType(p.Type, lang);
                                foreach (var acc in p.Accessors)
                                {
                                    foreach (var prm in acc.Parameters) AddReferencedType(prm.Type, lang);
                                    AddReferencedType(acc.ReturnValue?.Type, lang);
                                    if (includeLocals)
                                    {
                                        foreach (var lv in acc.LocalVariables) AddReferencedType(lv.Type, lang);
                                    }
                                }
                            }

                            foreach (var m in im.Methods)
                            {
                                foreach (var prm in m.Parameters) AddReferencedType(prm.Type, lang);
                                AddReferencedType(m.ReturnValue?.Type, lang);
                                if (includeLocals)
                                {
                                    foreach (var lv in m.LocalVariables) AddReferencedType(lv.Type, lang);
                                }
                            }
                        }
                    }
                }
            }
        }

        // Emit types.jsonl with typeHomeFilePath and declaredInFiles
        var typeLines = typeMap
            .OrderBy(kv => kv.Key.language).ThenBy(kv => kv.Key.fullName)
            .Select(kv =>
            {
                var lang = kv.Key.language;
                var fullName = kv.Key.fullName;
                var (name, files, isExternal, isPrimitive, _) = kv.Value;
                var home = ResolveTypeHome(lang, fullName, typeMap, e: null);
                return JsonSerializer.Serialize(new
                {
                    language = lang,
                    fullName,
                    name,
                    typeHomeFilePath = home,
                    isExternal,
                    isPrimitive,
                    declaredInFiles = files.ToArray(),
                });
            });
        WriteJsonl(Path.Combine(nodesDir, "types.jsonl"), typeLines);

        // Emit namespace_parent.jsonl
        var nsParentLines = nsParentEdges
            .OrderBy(e => e.Item1.lang).ThenBy(e => e.Item1.parent).ThenBy(e => e.Item2.child)
            .Select(e => JsonSerializer.Serialize(new
            {
                parent = new { language = e.Item1.lang, fullName = e.Item1.parent },
                child = new { language = e.Item2.lang, fullName = e.Item2.child },
            }));
        WriteJsonl(Path.Combine(relationsDir, "namespace_parent.jsonl"), nsParentLines);

        // Emit namespace_contains.jsonl
        var nsContainsLines = typeMap.Select(kv =>
            JsonSerializer.Serialize(new
            {
                @namespace = new { kv.Key.language, fullName = kv.Value.nsFullName },
                type = new
                {
                    kv.Key.language,
                    kv.Key.fullName,
                    typeHomeFilePath = ResolveTypeHome(kv.Key.language, kv.Key.fullName, typeMap, e: null)
                },
            }));
        WriteJsonl(Path.Combine(relationsDir, "namespace_contains.jsonl"), nsContainsLines);

        // Emit file_declares.jsonl
        var fileDeclaresLines = fileDeclares
            .OrderBy(x => x.filePath).ThenBy(x => x.typeKey.language).ThenBy(x => x.typeKey.fullName)
            .Select(x =>
                JsonSerializer.Serialize(new
                {
                    x.filePath,
                    type = new
                    {
                        x.typeKey.language,
                        x.typeKey.fullName,
                        typeHomeFilePath = typeMap[x.typeKey].files.First()
                    },
                }));
        WriteJsonl(Path.Combine(relationsDir, "file_declares.jsonl"), fileDeclaresLines);

        var namespacesCount = nsKeySet.Count;
        var typesCount = typeMap.Count;

        // Emit methods and has_method (streaming)
        var methodPath = Path.Combine(nodesDir, "methods.jsonl");
        var hasMethodPath = Path.Combine(relationsDir, "has_method.jsonl");
        using var methodWriter = new JsonlWriter(methodPath);
        using var hasMethodWriter = new JsonlWriter(hasMethodPath);
        int methodsCount = 0;
        int hasMethodCount = 0;

        // Streaming writers for parameters and returns
        var parametersPath = Path.Combine(nodesDir, "parameters.jsonl");
        var hasParameterPath = Path.Combine(relationsDir, "has_parameter.jsonl");
        var parameterTypePath = Path.Combine(relationsDir, "parameter_type.jsonl");
        var returnsPath = Path.Combine(relationsDir, "returns.jsonl");
        using var parameterWriter = new JsonlWriter(parametersPath);
        using var hasParameterWriter = new JsonlWriter(hasParameterPath);
        using var parameterTypeWriter = new JsonlWriter(parameterTypePath);
        using var returnsWriter = new JsonlWriter(returnsPath);
        int parametersCount = 0;
        int hasParameterCount = 0;
        int parameterTypeCount = 0;
        int returnsCount = 0;
        // Streaming writers for calls and accesses
        var callsPath = Path.Combine(relationsDir, "calls.jsonl");
        var accessesPath = Path.Combine(relationsDir, "accesses.jsonl");
        using var callsWriter = new JsonlWriter(callsPath);
        using var accessesWriter = new JsonlWriter(accessesPath);
        int callsCount = 0;
        int accessesCount = 0;

        if (referenceRepositoryModel != null)
        {
            foreach (var project in referenceRepositoryModel.Projects)
            {
                var lang = project.Language;
                foreach (var file in project.Files)
                {
                    foreach (var entity in file.Entities)
                    {
                        var typeFullName = BuildFullName(entity);
                        var typeKey = (lang, typeFullName);
                        if (!typeMap.TryGetValue(typeKey, out var typeEntry))
                        {
                            continue;
                        }

                        var typeHome = ResolveTypeHome(lang, typeFullName, typeMap, entity);

                        IEnumerable<MethodModel> EnumerateMethods(EntityModel e)
                        {
                            switch (e)
                            {
                                case ClassModel cm:
                                {
                                    foreach (var m in cm.Methods) yield return m;
                                    foreach (var c in cm.Constructors) yield return c;
                                    if (cm.Destructor != null) yield return cm.Destructor;
                                    foreach (var p in cm.Properties)
                                    {
                                        foreach (var acc in p.Accessors) yield return acc;
                                    }

                                    break;
                                }
                                case InterfaceModel im:
                                {
                                    foreach (var m in im.Methods) yield return m;
                                    foreach (var p in im.Properties)
                                    {
                                        foreach (var acc in p.Accessors) yield return acc;
                                    }

                                    break;
                                }
                            }
                        }

                        foreach (var method in EnumerateMethods(entity))
                        {
                            // signature: Name(Type1,Type2,...)
                            string SigTypeName(ParameterModel p) => p.Type.Name;
                            var paramTypeNames = method.Parameters.Select(SigTypeName);
                            var signature = $"{method.Name}({string.Join(',', paramTypeNames)})";

                            bool isAccessor = method.Type == MethodType.Accessor || method.ContainingProperty != null;
                            string? accessorKind = null;
                            if (isAccessor)
                            {
                                if (method.Name.StartsWith("get_")) accessorKind = "get";
                                else if (method.Name.StartsWith("set_")) accessorKind = "set";
                                else if (method.Name.StartsWith("init_")) accessorKind = "init";
                                else if (method.Name.StartsWith("add_")) accessorKind = "add";
                                else if (method.Name.StartsWith("remove_")) accessorKind = "remove";
                            }

                            var methodObj = new
                            {
                                language = lang,
                                declaringTypeFullName = typeFullName,
                                declaringTypeHomeFilePath = typeHome,
                                name = method.Name,
                                signature,
                                declaredInFile = entity.FilePath,
                                isPropertyAccessor = isAccessor,
                                accessorKind,
                                propertyName = method.ContainingProperty?.Name,
                            };
                            methodWriter.Write(methodObj);

                            var hasMethodObj = new
                            {
                                type = new { language = lang, fullName = typeFullName, typeHomeFilePath = typeHome },
                                method = new
                                {
                                    language = lang, declaringTypeFullName = typeFullName,
                                    declaringTypeHomeFilePath = typeHome, signature
                                },
                            };
                            hasMethodWriter.Write(hasMethodObj);
                            methodsCount++;
                            hasMethodCount++;

                            // parameters
                            for (int i = 0; i < method.Parameters.Count; i++)
                            {
                                var prm = method.Parameters[i];
                                var prmObj = new
                                {
                                    language = lang,
                                    methodDeclaringTypeFullName = typeFullName,
                                    methodDeclaringTypeHomeFilePath = typeHome,
                                    methodSignature = signature,
                                    position = i,
                                    name = prm.TypeName,
                                    modifier = prm.Modifier.ToString(),
                                    defaultValue = prm.DefaultValue,
                                    isNullable = prm.IsNullable,
                                };
                                parameterWriter.Write(prmObj);

                                var prmTypeRef = ToTypeRef(lang, prm.Type.Entity, typeMap);

                                var prmTypeRel = new
                                    {
                                        parameter = new
                                        {
                                            language = lang, methodDeclaringTypeFullName = typeFullName,
                                            methodDeclaringTypeHomeFilePath = typeHome, methodSignature = signature,
                                            position = i
                                        },
                                        type = prmTypeRef,
                                        isNullable = prm.IsNullable,
                                        isGeneric = prm.Type.IsGeneric,
                                    };
                                parameterTypeWriter.Write(prmTypeRel);
                                parameterTypeCount++;
                                hasParameterWriter.Write(new
                                {
                                    method = new
                                    {
                                        language = lang, declaringTypeFullName = typeFullName,
                                        declaringTypeHomeFilePath = typeHome, signature
                                    },
                                    parameter = new
                                    {
                                        language = lang, methodDeclaringTypeFullName = typeFullName,
                                        methodDeclaringTypeHomeFilePath = typeHome, methodSignature = signature,
                                        position = i
                                    },
                                });
                                hasParameterCount++;
                                parametersCount++;
                            }

                            // returns
                            if (method.ReturnValue?.Type != null)
                            {
                                var rType = method.ReturnValue.Type;
                                var rEnt = rType.Entity;
                                if (rEnt == null) continue;
                                var rRef = ToTypeRef(lang, rEnt, typeMap);
                                returnsWriter.Write(new
                                {
                                    method = new
                                    {
                                        language = lang, declaringTypeFullName = typeFullName,
                                        declaringTypeHomeFilePath = typeHome, signature
                                    },
                                    type = rRef,
                                    isNullable = rType.IsNullable,
                                    isGeneric = rType.IsGeneric,
                                });
                                returnsCount++;
                            }

                            // calls
                            foreach (var call in method.OutgoingCalls)
                            {
                                // caller is current method
                                string CalledSignature(MethodModel m)
                                {
                                    return $"{m.Name}({string.Join(',', m.Parameters.Select(p => p.Type.Name))})";
                                }

                                var called = call.Called;
                                var calledEnt = call.Called.Entity;
                                var (calledTypeFull, calledTypeEntry) = EnsureTypeInMap(lang, calledEnt, typeMap);

                                var calledTypeHome = ResolveTypeHome(lang, calledTypeFull, typeMap, calledEnt);
                                var calledSig = CalledSignature(called);

                                var gpTypes = call.GenericParameters
                                    .Select(gt =>
                                    {
                                        // ensure generic parameter type is present in the map for consistency
                                        EnsureTypeInMap(lang, gt.Entity, typeMap);
                                        return ToTypeRef(lang, gt.Entity, typeMap);
                                    })
                                    .ToArray();
                                var cpTypes = call.ConcreteParameters
                                    .Select(gt => ToTypeRef(lang, gt.Entity, typeMap))
                                    .ToArray();

                                callsWriter.Write(new
                                {
                                    caller = new
                                    {
                                        language = lang, declaringTypeFullName = typeFullName,
                                        declaringTypeHomeFilePath = typeHome, signature
                                    },
                                    called = new
                                    {
                                        language = lang, declaringTypeFullName = calledTypeFull,
                                        declaringTypeHomeFilePath = calledTypeHome, signature = calledSig
                                    },
                                    genericParamTypes = gpTypes,
                                    concreteParamTypes = cpTypes,
                                });
                                callsCount++;
                            }

                            // accesses
                            foreach (var acc in method.FieldAccesses)
                            {
                                var fEnt = acc.Field.Entity;
                                var _ = EnsureTypeInMap(lang, fEnt, typeMap);
                                var fRef = ToTypeRef(lang, fEnt, typeMap);

                                accessesWriter.Write(new
                                {
                                    method = new
                                    {
                                        language = lang, declaringTypeFullName = typeFullName,
                                        declaringTypeHomeFilePath = typeHome, signature
                                    },
                                    field = new
                                    {
                                        language = fRef.language, declaringTypeFullName = fRef.fullName,
                                        declaringTypeHomeFilePath = fRef.typeHomeFilePath, name = acc.Field.Name
                                    },
                                    accessKind = acc.AccessKind.ToString(),
                                });
                                accessesCount++;
                            }
                        }
                    }
                }
            }
        }

        // methods.jsonl and has_method.jsonl already written via streaming above

        // Emit fields and has_field (streaming)
        var fieldsPath = Path.Combine(nodesDir, "fields.jsonl");
        var hasFieldPath = Path.Combine(relationsDir, "has_field.jsonl");
        var fieldTypePath = Path.Combine(relationsDir, "field_type.jsonl");
        using var fieldsWriter = new JsonlWriter(fieldsPath);
        using var hasFieldWriter = new JsonlWriter(hasFieldPath);
        using var fieldTypeWriter = new JsonlWriter(fieldTypePath);
        int fieldsCount = 0;
        int hasFieldCount = 0;
        int fieldTypeCount = 0;

        if (referenceRepositoryModel != null)
        {
            foreach (var project in referenceRepositoryModel.Projects)
            {
                var lang = project.Language;
                foreach (var file in project.Files)
                {
                    foreach (var entity in file.Entities)
                    {
                        if (entity is not ClassModel cm) continue;
                        var typeFullName = BuildFullName(entity);
                        var typeKey = (lang, typeFullName);
                        if (!typeMap.TryGetValue(typeKey, out var typeEntry))
                        {
                            continue;
                        }

                        var typeHome = ResolveTypeHome(lang, typeFullName, typeMap, entity);

                        foreach (var field in cm.Fields)
                        {
                            var fieldObj = new
                            {
                                language = lang,
                                declaringTypeFullName = typeFullName,
                                declaringTypeHomeFilePath = typeHome,
                                name = field.Name,
                                declaredInFile = entity.FilePath,
                                isEvent = field.IsEvent,
                                isNullable = field.IsNullable,
                            };
                            fieldsWriter.Write(fieldObj);

                            var hasFieldObj = new
                            {
                                type = new { language = lang, fullName = typeFullName, typeHomeFilePath = typeHome },
                                field = new
                                {
                                    language = lang, declaringTypeFullName = typeFullName,
                                    declaringTypeHomeFilePath = typeHome, name = field.Name
                                },
                            };
                            hasFieldWriter.Write(hasFieldObj);
                            fieldsCount++;
                            hasFieldCount++;

                            // field type
                            if (field.Type == null) continue;
                            var fEnt = field.Type.Entity;
                            var fNs = fEnt.Namespace?.FullName ?? string.Empty;
                            var fFull = string.IsNullOrEmpty(fNs) ? fEnt.Name : $"{fNs}.{fEnt.Name}";
                            var fRef = ToTypeRef(lang, fEnt, typeMap);
                            fieldTypeWriter.Write(new
                            {
                                field = new
                                {
                                    language = lang, declaringTypeFullName = typeFullName,
                                    declaringTypeHomeFilePath = typeHome, name = field.Name
                                },
                                type = fRef,
                                isNullable = field.IsNullable,
                                isGeneric = field.Type.IsGeneric,
                            });
                            fieldTypeCount++;
                        }
                    }
                }
            }
        }

        // fields.jsonl, has_field.jsonl, and field_type.jsonl were streamed above
        // parameters/has_parameter/parameter_type/returns were streamed above
        // calls.jsonl and accesses.jsonl were streamed above

        // Locals (optional) streaming
        using var localVarsWriter = includeLocals
            ? new JsonlWriter(Path.Combine(nodesDir, "local_variables.jsonl"))
            : null;
        using var declaresLocalWriter =
            includeLocals ? new JsonlWriter(Path.Combine(relationsDir, "declares_local.jsonl")) : null;
        using var localTypeWriter =
            includeLocals ? new JsonlWriter(Path.Combine(relationsDir, "local_type.jsonl")) : null;
        var localVarCount = 0;
        var declaresLocalCount = 0;
        var localTypeCount = 0;
        if (includeLocals && referenceRepositoryModel != null)
        {
            foreach (var project in referenceRepositoryModel.Projects)
            {
                var lang = project.Language;
                foreach (var file in project.Files)
                {
                    foreach (var entity in file.Entities)
                    {
                        var typeFullName = BuildFullName(entity);
                        if (!typeMap.TryGetValue((lang, typeFullName), out var typeEntry)) continue;
                        var typeHome = ResolveTypeHome(lang, typeFullName, typeMap, entity);

                        IEnumerable<MethodModel> EnumerateMethods(EntityModel e)
                        {
                            switch (e)
                            {
                                case ClassModel cm:
                                {
                                    foreach (var m in cm.Methods) yield return m;
                                    foreach (var c in cm.Constructors) yield return c;
                                    if (cm.Destructor != null) yield return cm.Destructor;
                                    foreach (var p in cm.Properties)
                                    foreach (var acc in p.Accessors)
                                        yield return acc;
                                    break;
                                }
                                case InterfaceModel im:
                                {
                                    foreach (var m in im.Methods) yield return m;
                                    foreach (var p in im.Properties)
                                    foreach (var acc in p.Accessors)
                                        yield return acc;
                                    break;
                                }
                            }
                        }

                        foreach (var method in EnumerateMethods(entity))
                        {
                            string SigTypeName(ParameterModel p) => p.Type.Name;
                            var signature = $"{method.Name}({string.Join(',', method.Parameters.Select(SigTypeName))})";

                            for (int i = 0; i < method.LocalVariables.Count; i++)
                            {
                                var lv = method.LocalVariables[i];
                                localVarsWriter!.Write(new
                                {
                                    language = lang,
                                    methodDeclaringTypeFullName = typeFullName,
                                    methodDeclaringTypeHomeFilePath = typeHome,
                                    methodSignature = signature,
                                    name = lv.Name,
                                    ordinal = i,
                                    declaredInFile = entity.FilePath,
                                    isNullable = lv.IsNullable,
                                    modifier = lv.Modifier,
                                });
                                localVarCount++;

                                // declares_local
                                declaresLocalWriter!.Write(new
                                {
                                    method = new
                                    {
                                        language = lang, declaringTypeFullName = typeFullName,
                                        declaringTypeHomeFilePath = typeHome, signature
                                    },
                                    local = new
                                    {
                                        language = lang, methodDeclaringTypeFullName = typeFullName,
                                        methodDeclaringTypeHomeFilePath = typeHome, methodSignature = signature,
                                        name = lv.Name, ordinal = i
                                    },
                                });
                                declaresLocalCount++;

                                // local_type
                                var lEnt = lv.Type.Entity;
                                var lRef = ToTypeRef(lang, lEnt, typeMap);
                                localTypeWriter!.Write(new
                                {
                                    local = new
                                    {
                                        language = lang, methodDeclaringTypeFullName = typeFullName,
                                        methodDeclaringTypeHomeFilePath = typeHome, methodSignature = signature,
                                        name = lv.Name, ordinal = i
                                    },
                                    type = lRef,
                                    isNullable = lv.IsNullable,
                                    isGeneric = lv.Type.IsGeneric,
                                });
                                localTypeCount++;
                            }
                        }
                    }
                }
            }
        }
        // locals were streamed above when enabled

        // Inheritance and implementation
        var extendsRecords = new List<string>();
        var implementsRecords = new List<string>();
        if (referenceRepositoryModel != null)
        {
            foreach (var project in referenceRepositoryModel.Projects)
            {
                var lang = project.Language;
                foreach (var file in project.Files)
                {
                    foreach (var entity in file.Entities)
                    {
                        var childFull = BuildFullName(entity);
                        if (!typeMap.TryGetValue((lang, childFull), out var childEntry)) continue;
                        var childHome = ResolveTypeHome(lang, childFull, typeMap, entity);

                        if (entity is ClassModel cm)
                        {
                            foreach (var bt in cm.BaseTypes)
                            {
                                var be = bt.Entity;
                                var (baseFull, baseEntry) = EnsureTypeInMap(lang, be, typeMap);

                                var baseHome = ResolveTypeHome(lang, baseFull, typeMap, be);

                                if (be is ClassModel)
                                {
                                    extendsRecords.Add(JsonSerializer.Serialize(new
                                    {
                                        subtype = new
                                            { language = lang, fullName = childFull, typeHomeFilePath = childHome },
                                        supertype = new
                                            { language = lang, fullName = baseFull, typeHomeFilePath = baseHome },
                                    }));
                                }
                                else if (be is InterfaceModel)
                                {
                                    implementsRecords.Add(JsonSerializer.Serialize(new
                                    {
                                        type = new
                                        {
                                            language = lang, fullName = childFull, typeHomeFilePath = childHome
                                        },
                                        @interface = new
                                            { language = lang, fullName = baseFull, typeHomeFilePath = baseHome },
                                    }));
                                }
                            }
                        }
                        else if (entity is InterfaceModel im)
                        {
                            foreach (var bt in im.BaseTypes)
                            {
                                var be = bt.Entity;
                                var (baseFull, baseEntry) = EnsureTypeInMap(lang, be, typeMap);

                                var baseHome = ResolveTypeHome(lang, baseFull, typeMap, be);

                                extendsRecords.Add(JsonSerializer.Serialize(new
                                {
                                    subtype = new
                                        { language = lang, fullName = childFull, typeHomeFilePath = childHome },
                                    supertype = new
                                        { language = lang, fullName = baseFull, typeHomeFilePath = baseHome },
                                }));
                            }
                        }
                    }
                }
            }
        }

        extendsRecords = extendsRecords.OrderBy(s => s, StringComparer.Ordinal).ToList();
        implementsRecords = implementsRecords.OrderBy(s => s, StringComparer.Ordinal).ToList();
        WriteJsonl(Path.Combine(relationsDir, "extends.jsonl"), extendsRecords);
        WriteJsonl(Path.Combine(relationsDir, "implements.jsonl"), implementsRecords);

        // Imports (file-level) and Attributes streaming
        using var importsNsWriter = includeImports
            ? new JsonlWriter(Path.Combine(relationsDir, "imports_namespace.jsonl"))
            : null;
        using var importsEntWriter =
            includeImports ? new JsonlWriter(Path.Combine(relationsDir, "imports_entity.jsonl")) : null;
        int importsNamespaceCount = 0;
        int importsEntityCount = 0;
        using var attributesWriter = includeAttributes
            ? new JsonlWriter(Path.Combine(nodesDir, "attributes.jsonl"))
            : null;
        int attributesCount = 0;

        if (referenceRepositoryModel != null)
        {
            foreach (var project in referenceRepositoryModel.Projects)
            {
                var lang = project.Language;
                foreach (var file in project.Files)
                {
                    // imports: file.Imports only (entity-level imports excluded by v1)
                    if (includeImports)
                    {
                        foreach (var imp in file.Imports)
                        {
                            var aliasTypeStr = imp.AliasType.ToString();
                            if (imp.Namespace != null)
                            {
                                importsNsWriter!.Write(new
                                {
                                    filePath = file.FilePath,
                                    @namespace = new { language = lang, fullName = imp.Namespace?.FullName ?? string.Empty },
                                    alias = imp.Alias,
                                    aliasType = aliasTypeStr,
                                    isStatic = imp.IsStatic,
                                });
                                importsNamespaceCount++;
                            }

                            if (imp.Entity != null)
                            {
                                var (full, entry) = EnsureTypeInMap(lang, imp.Entity, typeMap);
                                var home = ResolveTypeHome(lang, full, typeMap, imp.Entity);
                                importsEntWriter!.Write(new
                                {
                                    filePath = file.FilePath,
                                    entity = new { language = lang, fullName = full, typeHomeFilePath = home },
                                    alias = imp.Alias,
                                    aliasType = aliasTypeStr,
                                    isStatic = imp.IsStatic,
                                });
                                importsEntityCount++;
                            }
                        }
                    }

                    if (includeAttributes)
                    {
                        // helper to emit attribute node
                        string SerializeAttrArgs(IList<ParameterModel> pars)
                        {
                            var list = pars.Select(p => new
                            {
                                typeName = p.TypeName,
                                isNullable = p.IsNullable,
                                modifier = p.Modifier.ToString(),
                                defaultValue = p.DefaultValue,
                            });
                            return JsonSerializer.Serialize(list);
                        }

                        // type-level attributes
                        foreach (var entity in file.Entities)
                        {
                            var nsFullName = entity.Namespace?.FullName ?? string.Empty;
                            var typeFullName = string.IsNullOrEmpty(nsFullName)
                                ? entity.Name
                                : $"{nsFullName}.{entity.Name}";
                            var typeHome = ResolveTypeHome(lang, typeFullName, typeMap, entity);

                            for (int ai = 0; ai < entity.Attributes.Count; ai++)
                            {
                                var attr = entity.Attributes[ai];
                                var aEnt = attr.Type.Entity;
                                var aNs = aEnt.Namespace?.FullName ?? string.Empty;
                                var aFull = string.IsNullOrEmpty(aNs) ? aEnt.Name : $"{aNs}.{aEnt.Name}";
                                attributesWriter!.Write(new
                                {
                                    language = lang,
                                    ownerKind = "Type",
                                    ownerDeclaringTypeFullName = (string?)null,
                                    ownerDeclaringTypeHomeFilePath = (string?)null,
                                    ownerSignatureOrName = typeFullName,
                                    orderIndex = ai,
                                    typeFullName = aFull,
                                    target = attr.Target.ToString(),
                                    argsJson = SerializeAttrArgs(attr.Parameters),
                                });
                                attributesCount++;
                            }

                            // field-level attributes
                            if (entity is ClassModel cm)
                            {
                                foreach (var field in cm.Fields)
                                {
                                    for (int ai = 0; ai < field.Attributes.Count; ai++)
                                    {
                                        var attr = field.Attributes[ai];
                                        var aEnt = attr.Type.Entity;
                                        var aNs = aEnt.Namespace?.FullName ?? string.Empty;
                                        var aFull = string.IsNullOrEmpty(aNs) ? aEnt.Name : $"{aNs}.{aEnt.Name}";
                                        attributesWriter!.Write(new
                                        {
                                            language = lang,
                                            ownerKind = "Field",
                                            ownerDeclaringTypeFullName = typeFullName,
                                            ownerDeclaringTypeHomeFilePath = typeHome,
                                            ownerSignatureOrName = field.Name,
                                            orderIndex = ai,
                                            typeFullName = aFull,
                                            target = attr.Target.ToString(),
                                            argsJson = SerializeAttrArgs(attr.Parameters),
                                        });
                                        attributesCount++;
                                    }
                                }

                                // method/accessor-level attributes
                                IEnumerable<MethodModel> EnumerateMethodsForAttr(ClassModel c)
                                {
                                    foreach (var m in c.Methods) yield return m;
                                    foreach (var cstr in c.Constructors) yield return cstr;
                                    if (c.Destructor != null) yield return c.Destructor;
                                    foreach (var p in c.Properties)
                                    foreach (var acc in p.Accessors)
                                        yield return acc;
                                }

                                foreach (var m in EnumerateMethodsForAttr(cm))
                                {
                                    string SigTypeName(ParameterModel p) => p.Type.Name;
                                    var signature = $"{m.Name}({string.Join(',', m.Parameters.Select(SigTypeName))})";

                                    for (int ai = 0; ai < m.Attributes.Count; ai++)
                                    {
                                        var attr = m.Attributes[ai];
                                        var aEnt = attr.Type.Entity;
                                        var aNs = aEnt?.Namespace?.FullName ?? string.Empty;
                                        var aFull = string.IsNullOrEmpty(aNs) ? aEnt?.Name : $"{aNs}.{aEnt?.Name}";
                                        attributesWriter!.Write(new
                                        {
                                            language = lang,
                                            ownerKind = "Method",
                                            ownerDeclaringTypeFullName = typeFullName,
                                            ownerDeclaringTypeHomeFilePath = typeHome,
                                            ownerSignatureOrName = signature,
                                            orderIndex = ai,
                                            typeFullName = aFull,
                                            target = attr.Target.ToString(),
                                            argsJson = SerializeAttrArgs(attr.Parameters),
                                        });
                                        attributesCount++;
                                    }

                                    // parameter-level attributes
                                    foreach (var prm in m.Parameters)
                                    {
                                        for (var ai = 0; ai < prm.Attributes.Count; ai++)
                                        {
                                            var attr = prm.Attributes[ai];
                                            var aEnt = attr.Type.Entity;
                                            var aNs = aEnt?.Namespace?.FullName ?? string.Empty;
                                            var aFull = string.IsNullOrEmpty(aNs) ? aEnt?.Name : $"{aNs}.{aEnt?.Name}";
                                            attributesWriter!.Write(new
                                            {
                                                language = lang,
                                                ownerKind = "Parameter",
                                                ownerDeclaringTypeFullName = typeFullName,
                                                ownerDeclaringTypeHomeFilePath = typeHome,
                                                ownerSignatureOrName = signature,
                                                orderIndex = ai,
                                                typeFullName = aFull,
                                                target = attr.Target.ToString(),
                                                argsJson = SerializeAttrArgs(attr.Parameters),
                                            });
                                            attributesCount++;
                                        }
                                    }
                                }
                            }
                            else if (entity is InterfaceModel im)
                            {
                                foreach (var m in im.Methods)
                                {
                                    string SigTypeName(ParameterModel p) => p.Type.Name;
                                    var signature = $"{m.Name}({string.Join(',', m.Parameters.Select(SigTypeName))})";
                                    for (int ai = 0; ai < m.Attributes.Count; ai++)
                                    {
                                        var attr = m.Attributes[ai];
                                        var aEnt = attr.Type.Entity;
                                        var aNs = aEnt.Namespace?.FullName ?? string.Empty;
                                        var aFull = string.IsNullOrEmpty(aNs) ? aEnt.Name : $"{aNs}.{aEnt.Name}";
                                        attributesWriter!.Write(new
                                        {
                                            language = lang,
                                            ownerKind = "Method",
                                            ownerDeclaringTypeFullName = typeFullName,
                                            ownerDeclaringTypeHomeFilePath = typeHome,
                                            ownerSignatureOrName = signature,
                                            orderIndex = ai,
                                            typeFullName = aFull,
                                            target = attr.Target.ToString(),
                                            argsJson = SerializeAttrArgs(attr.Parameters),
                                        });
                                        attributesCount++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        // imports and attributes were streamed above when enabled

        // Minimal v1 manifest with options and placeholder counts
        var manifestPath = Path.Combine(outputRoot, "manifest.json");
        var manifest = new
        {
            schemaVersion = "v1",
            generator = "Honeydew ExportGraphScript",
            options = new
            {
                includeAttributes,
                includeImports,
                includeLocals,
            },
            files = new
            {
                nodes = new[]
                {
                    "namespaces.jsonl",
                    "types.jsonl",
                    "methods.jsonl",
                    "fields.jsonl",
                    "parameters.jsonl",
                    "attributes.jsonl",
                    // local_variables.jsonl intentionally omitted by default
                },
                rels = new[]
                {
                    "namespace_parent.jsonl",
                    "namespace_contains.jsonl",
                    "file_declares.jsonl",
                    "has_method.jsonl",
                    "has_field.jsonl",
                    "extends.jsonl",
                    "implements.jsonl",
                    "calls.jsonl",
                    "accesses.jsonl",
                    "returns.jsonl",
                    "has_parameter.jsonl",
                    "parameter_type.jsonl",
                    "field_type.jsonl",
                    "imports_namespace.jsonl",
                    "imports_entity.jsonl",
                    // declares_local.jsonl, local_type.jsonl optional
                }
            },
            counts = new
            {
                namespaces = namespacesCount,
                types = typesCount,
                methods = methodsCount,
                fields = fieldsCount,
                parameters = parametersCount,
                attributes = attributesCount,
                rels = nsParentEdges.Count
                       + typeMap.Count
                       + fileDeclares.Count
                       + hasMethodCount
                       + hasFieldCount
                       + hasParameterCount
                       + parameterTypeCount
                       + returnsCount
                       + fieldTypeCount
                       + callsCount
                       + accessesCount
                       + extendsRecords.Count
                       + implementsRecords.Count
                       + (includeImports ? (importsNamespaceCount + importsEntityCount) : 0)
                       + (includeLocals ? (declaresLocalCount + localTypeCount) : 0)
            }
        };

        var json = JsonSerializer.Serialize(manifest, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        System.IO.File.WriteAllText(manifestPath, json);

        logger.Log($"Graph export written to {Path.GetFullPath(outputRoot)}");
        progress.Log($"Graph export written to {Path.GetFullPath(outputRoot)}");
        return;

        // Helper to ensure referenced types exist in typeMap (including external/primitives)
        void AddReferencedType(EntityType? et, string lang)
        {
            if (et == null) return;
            var entity = et.Entity;
            if (entity == null) return;
            var nsFullName = entity.Namespace?.FullName ?? string.Empty;
            var fullName = string.IsNullOrEmpty(nsFullName) ? entity.Name : $"{nsFullName}.{entity.Name}";
            var key = (lang, fullName);
            if (!typeMap.ContainsKey(key))
            {
                typeMap[key] = (entity.Name,
                    new SortedSet<string>(StringComparer.Ordinal),
                    entity.IsExternal || entity.IsPrimitive,
                    entity.IsPrimitive,
                    nsFullName);
            }

            // Recurse into generic arguments
            if (et.GenericTypes != null)
            {
                foreach (var gt in et.GenericTypes)
                {
                    AddReferencedType(gt, lang);
                }
            }
        }

        // Local helper to write JSONL lines
        void WriteJsonl(string filePath, IEnumerable<string> lines)
        {
            using var sw = new StreamWriter(filePath, append: false);
            foreach (var line in lines)
            {
                sw.WriteLine(line);
            }
        }
    }
}
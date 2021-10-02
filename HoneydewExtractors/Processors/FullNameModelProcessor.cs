using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HoneydewCore.Logging;
using HoneydewCore.ModelRepresentations;
using HoneydewCore.Processors;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels.CSharp;
using HoneydewModels.Types;

namespace HoneydewExtractors.Processors
{
    public class FullNameModelProcessor : IProcessorFunction<RepositoryModel, RepositoryModel>
    {
        private readonly ILogger _logger;
        private readonly ILogger _ambiguousClassLogger;

        private readonly IProgressLogger _progressLogger;
        private readonly bool _disableLocalVariablesBinding;


        private Dictionary<string, List<IClassType>> _classNames;
        private readonly Dictionary<string, List<Tuple<ProjectModel, IClassType>>> _classFqns = new();

        private readonly ConcurrentDictionary<string, NamespaceTree> _namespacesDictionary = new();

        private readonly ConcurrentDictionary<string, byte> _notFoundClassNames = new();

        private int _classCount;

        public FullNameModelProcessor(ILogger logger, ILogger ambiguousClassLogger, IProgressLogger progressLogger,
            bool disableLocalVariablesBinding)
        {
            _logger = logger;
            _progressLogger = progressLogger;
            _disableLocalVariablesBinding = disableLocalVariablesBinding;
            _ambiguousClassLogger = ambiguousClassLogger;
        }

        public RepositoryModel Process(RepositoryModel repositoryModel)
        {
            _logger.Log("Resolving Class Names");
            _progressLogger.Log();
            _progressLogger.Log("Resolving Fully Qualified Class Names");

            var allClasses = new List<IClassType>();

            foreach (var projectModel in repositoryModel.Projects)
            {
                foreach (var namespaceModel in projectModel.CompilationUnits)
                {
                    foreach (var classModel in namespaceModel.ClassTypes)
                    {
                        allClasses.Add(classModel);

                        AddClassModelToNamespaceGraph(classModel.Name, classModel.FilePath,
                            classModel.ContainingTypeName);

                        var tuple = Tuple.Create(projectModel, classModel);
                        if (_classFqns.TryGetValue(classModel.Name, out var list))
                        {
                            list.Add(tuple);
                        }
                        else
                        {
                            _classFqns.Add(classModel.Name, new List<Tuple<ProjectModel, IClassType>>
                            {
                                tuple
                            });
                        }
                    }
                }
            }

            _classCount = allClasses.Count;

            _progressLogger.Log();
            _progressLogger.Log("Resolving Class Names");
            _classNames = allClasses.GroupBy(it => it.Name[(it.Name.LastIndexOf('.') + 1)..])
                .ToDictionary(it => it.Key, it => it.ToList());

            _logger.Log("Resolving Using Statements for Each Class");
            _progressLogger.Log();
            _progressLogger.Log("Resolving Using Statements for Each Class");
            SetFullNameForUsings(repositoryModel);

            _logger.Log("Resolving Class Elements (Fields, Methods, Properties,...)");
            _logger.Log();
            _progressLogger.Log();
            _progressLogger.Log("Resolving Class Elements (Fields, Methods, Properties,...)");
            SetFullNameForClassModelComponents(repositoryModel);

            return repositoryModel;
        }

        private string FindClassFullName(string className, string namespaceModelName, IList<IImportType> usingModels,
            ConcurrentDictionary<string, string> classLevelCache,
            string classFilePath = "", int genericParameterCount = 0)
        {
            if (string.IsNullOrEmpty(className))
            {
                return className;
            }

            if (_notFoundClassNames.ContainsKey(className))
            {
                return className;
            }

            if (CSharpConstants.IsPrimitive(className))
            {
                return className;
            }

            var classNameNonNullable = className;
            var nullableString = "";
            if (className.EndsWith("?"))
            {
                nullableString = "?";
                classNameNonNullable = classNameNonNullable.Replace("?", "");
            }

            if (classLevelCache.TryGetValue(classNameNonNullable, out var cachedName))
            {
                var parameterCount = GetGenericParameterCount(cachedName, out _);
                if (parameterCount == genericParameterCount)
                {
                    return cachedName + nullableString;
                }
            }

            if (_classFqns.TryGetValue(classNameNonNullable, out _))
            {
                return classLevelCache.GetOrAdd(className, classNameNonNullable + nullableString);
            }

            if (_classFqns.TryGetValue($"{namespaceModelName}.{classNameNonNullable}", out _))
            {
                return classLevelCache.GetOrAdd(className,
                    $"{namespaceModelName}.{classNameNonNullable}{nullableString}");
            }

            var aliasesForClass = GetAliasesForClass(classNameNonNullable, usingModels);
            if (!string.IsNullOrEmpty(aliasesForClass))
            {
                return classLevelCache.GetOrAdd(className, aliasesForClass + nullableString);
            }

            List<IClassType> possibleClasses;
            if (genericParameterCount == 0)
            {
                if (!_classNames.TryGetValue(classNameNonNullable, out possibleClasses))
                {
                    _notFoundClassNames.TryAdd(classNameNonNullable, 0);
                    return classNameNonNullable + nullableString;
                }
            }
            else
            {
                possibleClasses = _classNames.Where(pair =>
                {
                    var (key, _) = pair;
                    var parameterCount = GetGenericParameterCount(key, out var nameWithoutParameters);
                    return parameterCount == genericParameterCount &&
                           nameWithoutParameters.EndsWith(classNameNonNullable);
                }).SelectMany(pair => pair.Value).ToList();
            }

            foreach (var possibleClass in possibleClasses)
            {
                var imports = usingModels.Where(@using => @using.Name == possibleClass.ContainingTypeName).ToList();

                switch (imports.Count)
                {
                    case 0:
                    {
                        // try to search in parent namespace
                        var lastIndexOf = namespaceModelName.LastIndexOf('.');
                        if (lastIndexOf >= 0)
                        {
                            return FindClassFullName(className, namespaceModelName[..lastIndexOf], imports,
                                classLevelCache, classFilePath, genericParameterCount);
                        }

                        _notFoundClassNames.TryAdd(className, 0);
                        return className;
                    }
                    case 1:

                        var combinedName = $"{possibleClass.Name}{nullableString}";

                        return classLevelCache.AddOrUpdate(className, combinedName, (key, oldValue) =>
                        {
                            var parameterCount = GetGenericParameterCount(key, out _);

                            return parameterCount != genericParameterCount ? combinedName : oldValue;
                        });
                    default:
                    {
                        _ambiguousClassLogger.Log($"{className} found in multiple imports in {classFilePath}:");
                        foreach (var import in imports)
                        {
                            _ambiguousClassLogger.Log($"{import.Name}.{className}{nullableString}");
                        }

                        _ambiguousClassLogger.Log();
                        _ambiguousClassLogger.Log();

                        return classLevelCache.GetOrAdd(className,
                            $"{possibleClass.Name}{nullableString}");
                    }
                }
            }

            _notFoundClassNames.TryAdd(classNameNonNullable, 0);
            return classNameNonNullable + nullableString;
        }

        private int GetGenericParameterCount(string className, out string nameWithoutParameters)
        {
            nameWithoutParameters = className;
            var indexOfStartBracket = className.IndexOf('<');
            if (indexOfStartBracket < 0)
            {
                return 0;
            }

            nameWithoutParameters = className[..indexOfStartBracket];

            var commaCount = className.Count(c => c == ',');

            return commaCount + 1;
        }

        private string GetAliasesForClass(string className, IList<IImportType> importTypes)
        {
            foreach (var importType in importTypes)
            {
                if (importType.AliasType == nameof(EAliasType.Class) && className == importType.Alias)
                {
                    return importType.Name;
                }

                if (importType.AliasType == nameof(EAliasType.Namespace) && className.StartsWith(importType.Alias))
                {
                    return className.Replace(importType.Alias, importType.Name);
                }
            }

            return null;
        }

        private void SetFullNameForUsings(RepositoryModel repositoryModel)
        {
            var progressBar =
                _progressLogger.CreateProgressLogger(_classCount, "Resolving Using Statements for Each Class");
            progressBar.Start();

            foreach (var projectModel in repositoryModel.Projects)
            {
                foreach (var compilationUnitType in projectModel.CompilationUnits)
                {
                    foreach (var classType in compilationUnitType.ClassTypes)
                    {
                        progressBar.Step($"{classType.Name} from {classType.FilePath}");

                        foreach (var usingModel in classType.Imports)
                        {
                            if (usingModel.AliasType == nameof(EAliasType.NotDetermined))
                            {
                                if (classType is ClassModel classModel)
                                {
                                    usingModel.AliasType = DetermineUsingType(usingModel, classModel);
                                }
                            }

                            // var beforeName = usingModel.Name;
                            usingModel.Name = FindNamespaceFullName(usingModel.Name, classType.ContainingTypeName,
                                classType.Imports);
                            // if (usingModel.Name == beforeName)
                            // {
                            //     usingModel.Name = FindClassFullName(usingModel.Name, namespaceModel,
                            //         projectModel, solutionModel, repositoryModel, classModel.Usings, classModel.FilePath);
                            // }
                        }
                    }
                }
            }

            progressBar.Stop();
        }

        private static string DetermineUsingType(IImportType usingModel, IPropertyMembersClassType classModel)
        {
            var namespaceAccess = $"{usingModel.Alias}.";

            if (classModel.Fields.Any(fieldModel => fieldModel.Type.Name.StartsWith(namespaceAccess)))
            {
                return nameof(EAliasType.Namespace);
            }

            foreach (var propertyModel in classModel.Properties)
            {
                if (propertyModel.Type.Name.StartsWith(namespaceAccess))
                {
                    return nameof(EAliasType.Namespace);
                }

                foreach (var accessor in propertyModel.Accessors)
                {
                    if (IsAliasNamespaceSearchInCalledMethods(accessor.CalledMethods))
                    {
                        return nameof(EAliasType.Namespace);
                    }
                }
            }

            if (IsAliasNamespaceSearchInMethodModels(classModel.Methods))
            {
                return nameof(EAliasType.Namespace);
            }

            if (IsAliasNamespaceSearchInConstructorModels(classModel.Constructors))
            {
                return nameof(EAliasType.Namespace);
            }

            return nameof(EAliasType.Class);

            bool IsAliasNamespaceSearchInMethodModels(IEnumerable<IMethodType> methodTypes)
            {
                foreach (var methodModel in methodTypes)
                {
                    if (methodModel.ReturnValue.Type != null &&
                        !string.IsNullOrEmpty(methodModel.ReturnValue.Type.Name) &&
                        methodModel.ReturnValue.Type.Name.StartsWith(namespaceAccess))
                    {
                        return true;
                    }

                    if (methodModel.ParameterTypes.Any(parameterModel =>
                        parameterModel.Type != null && parameterModel.Type.Name.StartsWith(namespaceAccess)))
                    {
                        return true;
                    }

                    if (IsAliasNamespaceSearchInCalledMethods(methodModel.CalledMethods))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool IsAliasNamespaceSearchInConstructorModels(IEnumerable<IConstructorType> constructorTypes)
            {
                foreach (var methodModel in constructorTypes)
                {
                    if (methodModel.ParameterTypes.Any(parameterModel =>
                        parameterModel.Type.Name.StartsWith(namespaceAccess)))
                    {
                        return true;
                    }

                    if (IsAliasNamespaceSearchInCalledMethods(methodModel.CalledMethods))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool IsAliasNamespaceSearchInCalledMethods(IEnumerable<IMethodSignatureType> calledMethods)
            {
                foreach (var calledMethod in calledMethods)
                {
                    if (!string.IsNullOrEmpty(calledMethod.ContainingTypeName) &&
                        calledMethod.ContainingTypeName.StartsWith(namespaceAccess))
                    {
                        return true;
                    }

                    if (calledMethod.ParameterTypes.Any(parameterModel =>
                        parameterModel.Type.Name.StartsWith(namespaceAccess)))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private void SetFullNameForClassModelComponents(RepositoryModel repositoryModel)
        {
            var progressBar =
                _progressLogger.CreateProgressLogger(_classCount,
                    "Resolving Class Elements (Fields, Methods, Properties,...)");
            progressBar.Start();

            var currentClassCount = 1;


            Parallel.ForEach(repositoryModel.Projects, projectModel =>
                // foreach (var projectModel in solutionModel.Projects)
            {
                foreach (var compilationUnitType in projectModel.CompilationUnits)
                {
                    Parallel.ForEach(compilationUnitType.ClassTypes, classType =>
                        // foreach (var classType in namespaceModel.ClassModels)
                    {
                        var classLevelCache = new ConcurrentDictionary<string, string>();

                        var start = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                        foreach (var baseType in classType.BaseTypes)
                        {
                            SetEntityTypeFullName(classType.ContainingTypeName, classType.Imports,
                                classType.FilePath,
                                baseType.Type, classLevelCache);
                        }

                        SetEntityAttributesFullName(repositoryModel, classType, classType.ContainingTypeName,
                            projectModel,
                            classType.Imports, classType.FilePath, classLevelCache);

                        SetGenericParametersFullName(repositoryModel, classType, classType.ContainingTypeName,
                            projectModel, classType.Imports, classType.FilePath, classLevelCache);

                        if (classType is IPropertyMembersClassType classModel)
                        {
                            var namespaceName = classModel.ContainingTypeName;

                            if (string.IsNullOrEmpty(namespaceName))
                            {
                                namespaceName = projectModel.Namespaces
                                    .FirstOrDefault(n => n.ClassNames.Contains(classModel.Name))?.Name;
                            }

                            if (string.IsNullOrEmpty(namespaceName))
                            {
                                namespaceName = projectModel.Namespaces.SelectMany(n => n.ClassNames)
                                    .FirstOrDefault(n => n.Contains(classModel.Name)) ?? "";
                            }

                            foreach (var fieldModel in classModel.Fields)
                            {
                                SetEntityTypeFullName(namespaceName, classModel.Imports,
                                    classModel.FilePath,
                                    fieldModel.Type, classLevelCache);

                                SetEntityAttributesFullName(repositoryModel, fieldModel, namespaceName,
                                    projectModel, classType.Imports, classType.FilePath, classLevelCache);
                            }

                            foreach (var propertyModel in classModel.Properties)
                            {
                                SetEntityTypeFullName(namespaceName, classModel.Imports,
                                    classModel.FilePath, propertyModel.Type, classLevelCache);

                                SetEntityAttributesFullName(repositoryModel, propertyModel,
                                    namespaceName, projectModel,
                                    classType.Imports, classType.FilePath, classLevelCache);

                                foreach (var accessor in propertyModel.Accessors)
                                {
                                    SetMethodSkeletonFullName(classModel.FilePath, accessor,
                                        namespaceName, projectModel, repositoryModel,
                                        classModel.Imports, classLevelCache);
                                }
                            }

                            foreach (var methodModel in classModel.Methods)
                            {
                                if (methodModel.ReturnValue != null)
                                {
                                    SetEntityTypeFullName(namespaceName, classModel.Imports,
                                        classModel.FilePath, methodModel.ReturnValue.Type, classLevelCache);


                                    SetEntityAttributesFullName(repositoryModel, methodModel.ReturnValue,
                                        namespaceName, projectModel,
                                        classType.Imports, classType.FilePath, classLevelCache);
                                }

                                SetMethodSkeletonFullName(classModel.FilePath, methodModel,
                                    namespaceName, projectModel, repositoryModel, classModel.Imports,
                                    classLevelCache);
                            }

                            foreach (var constructorType in classModel.Constructors)
                            {
                                SetMethodSkeletonFullName(classModel.FilePath, constructorType,
                                    namespaceName, projectModel, repositoryModel, classModel.Imports,
                                    classLevelCache);
                            }

                            ChangeDependencyMetricFullName(classModel, namespaceName, classLevelCache);
                        }

                        var end = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                        _logger.Log(
                            $"Resolving Elements for {classType.Name} from {classType.FilePath} ({currentClassCount}/{_classCount})");
                        progressBar.Step(
                            $"\r{classType.Name} from {classType.FilePath} ({currentClassCount}/{_classCount}) - {(end - start) / 1000} s");
                        currentClassCount++;
                    });
                    // }
                }

                _logger.Log();
            });
            // }

            progressBar.Stop();
        }

        private void SetMethodSkeletonFullName(string classFilePath, IMethodSkeletonType methodSkeletonType,
            string namespaceName, ProjectModel projectModel, RepositoryModel repositoryModel,
            IList<IImportType> importTypes, ConcurrentDictionary<string, string> classLevelCache)
        {
            methodSkeletonType.ContainingTypeName = FindClassFullName(methodSkeletonType.ContainingTypeName,
                namespaceName, importTypes, classLevelCache, classFilePath);

            foreach (var parameterModel in methodSkeletonType.ParameterTypes)
            {
                SetParameterFullName(repositoryModel, namespaceName, projectModel, importTypes,
                    classFilePath, parameterModel, classLevelCache);
            }

            SetEntityAttributesFullName(repositoryModel, methodSkeletonType, namespaceName, projectModel,
                importTypes, classFilePath, classLevelCache);

            SetFullNameForCalledMethods(classFilePath, methodSkeletonType.CalledMethods, namespaceName,
                projectModel, repositoryModel, importTypes, classLevelCache);

            if (!_disableLocalVariablesBinding)
            {
                SetLocalVariablesFullName(namespaceName, importTypes,
                    classFilePath, methodSkeletonType, classLevelCache);
            }
        }

        private void SetGenericParametersFullName(RepositoryModel repositoryModel, ITypeWithGenericParameters classType,
            string namespaceName, ProjectModel projectModel, IList<IImportType> importTypes, string filePath,
            ConcurrentDictionary<string, string> classLevelCache)
        {
            foreach (var genericParameter in classType.GenericParameters)
            {
                SetEntityAttributesFullName(repositoryModel, genericParameter, namespaceName, projectModel, importTypes,
                    filePath, classLevelCache);

                foreach (var constraint in genericParameter.Constraints)
                {
                    SetEntityTypeFullName(namespaceName, importTypes, filePath, constraint,
                        classLevelCache);
                }
            }
        }

        private void SetEntityAttributesFullName(RepositoryModel repositoryModel, ITypeWithAttributes classType,
            string namespaceName, ProjectModel projectModel, IList<IImportType> importTypes, string filePath,
            ConcurrentDictionary<string, string> classLevelCache)
        {
            foreach (var attributeType in classType.Attributes)
            {
                SetAttributeFullName(repositoryModel, namespaceName, projectModel, importTypes,
                    filePath, attributeType, classLevelCache);
            }
        }

        private void SetAttributeFullName(RepositoryModel repositoryModel, string namespaceName,
            ProjectModel projectModel, IList<IImportType> importTypes, string filePath,
            IMethodSignatureType attributeType, ConcurrentDictionary<string, string> classLevelCache)
        {
            var suffix = "";
            if (!attributeType.Name.EndsWith("Attribute"))
            {
                suffix = "Attribute";
            }

            attributeType.Name = FindClassFullName(attributeType.Name + suffix, namespaceName, importTypes,
                classLevelCache, filePath);

            attributeType.ContainingTypeName = FindClassFullName(attributeType.ContainingTypeName, namespaceName,
                importTypes, classLevelCache, filePath);

            foreach (var parameterType in attributeType.ParameterTypes)
            {
                SetParameterFullName(repositoryModel, namespaceName, projectModel, importTypes, filePath, parameterType,
                    classLevelCache);
            }
        }

        private void SetParameterFullName(RepositoryModel repositoryModel, string namespaceName,
            ProjectModel projectModel, IList<IImportType> importTypes, string filePath,
            IParameterType parameterType, ConcurrentDictionary<string, string> classLevelCache)
        {
            SetEntityTypeFullName(namespaceName, importTypes, filePath, parameterType.Type,
                classLevelCache);

            foreach (var attributeType in parameterType.Attributes)
            {
                SetAttributeFullName(repositoryModel, namespaceName, projectModel, importTypes, filePath, attributeType,
                    classLevelCache);
            }
        }

        private void SetEntityTypeFullName(string namespaceName, IList<IImportType> importTypes, string filePath,
            IEntityType entityType, ConcurrentDictionary<string, string> classLevelCache)
        {
            SetGenericTypeFullName(namespaceName, importTypes, filePath, entityType.FullType,
                classLevelCache);

            if (entityType.FullType != null)
            {
                entityType.Name = ReconstructFullName(entityType.FullType);
            }
            else
            {
                entityType.Name =
                    FindClassFullName(entityType.Name, namespaceName, importTypes, classLevelCache, filePath);
            }

            string ReconstructFullName(GenericType genericType)
            {
                if (genericType == null)
                {
                    return "";
                }

                var stringBuilder = new StringBuilder();
                var name = genericType.Name;

                name = FindClassFullName(name, namespaceName, importTypes, classLevelCache, filePath,
                    genericType.ContainedTypes.Count);

                var startBracketIndex = name.IndexOf('<');
                if (startBracketIndex >= 0)
                {
                    name = name[..startBracketIndex];
                }

                stringBuilder.Append(name);

                if (genericType.ContainedTypes.Count <= 0)
                {
                    return stringBuilder.ToString();
                }

                stringBuilder.Append('<');
                for (var i = 0; i < genericType.ContainedTypes.Count; i++)
                {
                    var containedType = genericType.ContainedTypes[i];
                    stringBuilder.Append(ReconstructFullName(containedType));
                    if (i != genericType.ContainedTypes.Count - 1)
                    {
                        stringBuilder.Append(',');
                    }
                }

                stringBuilder.Append('>');

                return stringBuilder.ToString();
            }
        }

        private void SetGenericTypeFullName(string namespaceName,
            IList<IImportType> importTypes,
            string filePath, GenericType genericType, ConcurrentDictionary<string, string> classLevelCache)
        {
            if (genericType == null || string.IsNullOrEmpty(genericType.Name))
            {
                return;
            }

            genericType.Name = FindClassFullName(genericType.Name, namespaceName, importTypes, classLevelCache,
                filePath, genericType.ContainedTypes.Count);

            foreach (var type in genericType.ContainedTypes)
            {
                SetGenericTypeFullName(namespaceName, importTypes, filePath, type, classLevelCache);
            }
        }

        private void SetLocalVariablesFullName(string namespaceName, IList<IImportType> importTypes, string filePath,
            ITypeWithLocalVariables typeWithLocalVariables, ConcurrentDictionary<string, string> classLevelCache)
        {
            foreach (var localVariableType in typeWithLocalVariables.LocalVariableTypes)
            {
                SetEntityTypeFullName(namespaceName, importTypes, filePath, localVariableType.Type,
                    classLevelCache);
            }
        }

        private void SetFullNameForCalledMethods(string classFilePath,
            IEnumerable<IMethodSignatureType> methodModelCalledMethods, string namespaceName, ProjectModel projectModel,
            RepositoryModel repositoryModel, IList<IImportType> importTypes,
            ConcurrentDictionary<string, string> classLevelCache)
        {
            foreach (var calledMethod in methodModelCalledMethods)
            {
                foreach (var parameterModel in calledMethod.ParameterTypes)
                {
                    SetParameterFullName(repositoryModel, namespaceName, projectModel, importTypes,
                        classFilePath, parameterModel, classLevelCache);
                }

                calledMethod.ContainingTypeName = FindClassFullName(calledMethod.ContainingTypeName, namespaceName,
                    importTypes, classLevelCache, classFilePath);

                var classModel = GetClassModelFullyQualified(calledMethod.ContainingTypeName, projectModel,
                    repositoryModel);

                // search for static import of a class
                if (classModel == null)
                {
                    calledMethod.ContainingTypeName = SearchForMethodNameInStaticImports(
                        calledMethod.ContainingTypeName, calledMethod.Name, calledMethod.ParameterTypes,
                        projectModel,
                        repositoryModel, importTypes);
                }
            }
        }

        private string SearchForMethodNameInStaticImports(string containingClassName, string methodName,
            IList<IParameterType> methodParameters, ProjectModel projectModel, RepositoryModel repositoryModel,
            IList<IImportType> importTypes)
        {
            if (!string.IsNullOrEmpty(containingClassName) && containingClassName.StartsWith("System."))
            {
                return containingClassName;
            }

            if (importTypes == null)
            {
                return "";
            }

            foreach (var importType in importTypes)
            {
                if (importType is UsingModel { IsStatic: false })
                {
                    continue;
                }

                var staticImportedClassType =
                    GetClassModelFullyQualified(importType.Name, projectModel, repositoryModel);

                if (staticImportedClassType == null)
                {
                    continue;
                }

                if (staticImportedClassType is not ClassModel staticImportedClass)
                {
                    continue;
                }

                foreach (var fieldModel in staticImportedClass.Fields)
                {
                    if (fieldModel.Name == containingClassName)
                    {
                        return fieldModel.Type.Name;
                    }
                }

                foreach (var propertyModel in staticImportedClass.Properties)
                {
                    if (propertyModel.Name == containingClassName)
                    {
                        return propertyModel.Type.Name;
                    }
                }

                foreach (var methodModel in staticImportedClass.Methods)
                {
                    var hasTheSameSignature = MethodsHaveTheSameSignature(methodModel.Name,
                        methodModel.ParameterTypes,
                        methodName, methodParameters);
                    if (hasTheSameSignature)
                    {
                        return staticImportedClass.Name;
                    }
                }
            }

            return containingClassName;
        }

        private static bool MethodsHaveTheSameSignature(string firstMethodName,
            IList<IParameterType> firstMethodParameters,
            string secondMethodName, IList<IParameterType> secondMethodParameters)
        {
            if (firstMethodName != secondMethodName)
            {
                return false;
            }

            if (firstMethodParameters.Count != secondMethodParameters.Count)
            {
                return false;
            }

            for (var i = 0; i < firstMethodParameters.Count; i++)
            {
                if (CSharpConstants.ConvertPrimitiveTypeToSystemType(firstMethodParameters[i].Type.Name) !=
                    CSharpConstants.ConvertPrimitiveTypeToSystemType(secondMethodParameters[i].Type.Name))
                {
                    return false;
                }

                if (firstMethodParameters[i] is ParameterModel firstMethodParameter &&
                    secondMethodParameters[i] is ParameterModel secondMethodParameter)
                {
                    if (string.IsNullOrEmpty(firstMethodParameter.Modifier) &&
                        !string.IsNullOrEmpty(secondMethodParameter.Modifier))
                    {
                        return false;
                    }

                    if (!string.IsNullOrEmpty(firstMethodParameter.Modifier) &&
                        string.IsNullOrEmpty(secondMethodParameter.Modifier))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void ChangeDependencyMetricFullName(IClassType classModel, string namespaceName,
            ConcurrentDictionary<string, string> classLevelCache)
        {
            var dependenciesMetrics = classModel.Metrics.Where(metric =>
                typeof(IRelationVisitor).IsAssignableFrom(Type.GetType(metric.ExtractorName)));

            foreach (var metric in dependenciesMetrics)
            {
                var dependencies = metric.Value as IDictionary<string, int>;
                if (dependencies == null)
                {
                    continue;
                }

                IDictionary<string, int> fullNameDependencies = new Dictionary<string, int>();
                foreach (var (dependencyName, appearanceCount) in dependencies)
                {
                    var wasSet = false;


                    var fullClassName = FindClassFullName(dependencyName, namespaceName, classModel.Imports,
                        classLevelCache, classModel.FilePath);

                    if (fullNameDependencies.ContainsKey(fullClassName))
                    {
                        continue;
                    }

                    if (fullClassName != dependencyName)
                    {
                        fullNameDependencies.Add(fullClassName, appearanceCount);
                        wasSet = true;
                    }

                    if (!wasSet)
                    {
                        fullNameDependencies.Add(dependencyName, appearanceCount);
                    }
                }

                dependencies = fullNameDependencies;
                metric.Value = dependencies;
            }
        }

        private string FindNamespaceFullName(string usingName, string namespaceName, IList<IImportType> usingModels)
        {
            if (string.IsNullOrEmpty(usingName))
            {
                return usingName;
            }

            if (IsNameFullyQualified(usingName))
            {
                return usingName;
            }

            var combinedName = $"{namespaceName}.{usingName}";

            if (IsNameFullyQualified(combinedName))
            {
                return combinedName;
            }

            if (usingModels != null)
            {
                // try searching for the aliases
                // if one class alias is found that matched, return the name
                // if one namespace alias is found, replace the alias with the real name of the namespace
                foreach (var usingModel in usingModels)
                {
                    if (usingModel.AliasType == nameof(EAliasType.Class) && usingName == usingModel.Alias)
                    {
                        return usingModel.Name;
                    }

                    var tempName = usingName;
                    if (usingModel.AliasType == nameof(EAliasType.Namespace) &&
                        usingName.StartsWith(usingModel.Alias))
                    {
                        tempName = usingName.Replace(usingModel.Alias, usingModel.Name);
                    }

                    var fullNamePossibilities = GetPossibilitiesFromNamespace(usingModel.Name, tempName, 0);
                    var firstOrDefault = fullNamePossibilities.FirstOrDefault();
                    if (firstOrDefault != null)
                    {
                        return firstOrDefault;
                    }
                }
            }

            return usingName;
        }


        private IEnumerable<string> GetPossibilitiesFromNamespace(string namespaceName, string className,
            int genericParametersCount)
        {
            var nameParts = namespaceName.Split('.');
            if (!_namespacesDictionary.TryGetValue(nameParts[0], out var fullNameNamespace))
            {
                return new List<string>();
            }

            var childNamespace = fullNameNamespace.GetChild(nameParts);
            if (childNamespace == null)
            {
                return new List<string>();
            }

            var childClassNamespace = GetClassInNamespaceAmongChildrenNamespaces(childNamespace, className);
            if (childClassNamespace != null)
            {
                return new List<string>
                {
                    childClassNamespace.GetFullName()
                };
            }

            if (childNamespace.Parent != null)
            {
                var brotherClassNamespace =
                    GetClassInNamespaceAmongChildrenNamespaces(childNamespace.Parent, className);
                if (brotherClassNamespace != null)
                {
                    return new List<string>
                    {
                        brotherClassNamespace.GetFullName()
                    };
                }
            }

            return childNamespace.GetPossibleChildren(className, genericParametersCount);
        }

        private NamespaceTree GetClassInNamespaceAmongChildrenNamespaces(NamespaceTree namespaceTree,
            string className)
        {
            foreach (var (childName, childNode) in namespaceTree.Children)
            {
                if (className == childName && childNode.Children.Count == 0)
                {
                    return childNode;
                }
            }

            return null;
        }

        private bool IsNameFullyQualified(string name)
        {
            var nameParts = name.Split(".");
            if (nameParts.Length > 0)
            {
                if (_namespacesDictionary.TryGetValue(nameParts[0], out var fullNameNamespace))
                {
                    if (fullNameNamespace.ContainsNameParts(nameParts))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        private static IClassType GetClassModelFullyQualified(string classNameToSearch,
            ProjectModel projectModelToStartSearchFrom, RepositoryModel repositoryModel)
        {
            if (string.IsNullOrEmpty(classNameToSearch))
            {
                return null;
            }

            IReadOnlyList<string> parts = classNameToSearch.Split(".");

            // search for fully name in provided ProjectModel
            var classModel = GetClassModelFullyQualified(parts, projectModelToStartSearchFrom);
            if (classModel != null)
            {
                return classModel;
            }

            // search for fully name in provided SolutionModel
            classModel = repositoryModel.Projects
                .Select(project => GetClassModelFullyQualified(parts, project))
                .FirstOrDefault();
            if (classModel != null)
            {
                return classModel;
            }

            // search for fully name in all solutions

            classModel = repositoryModel.Projects
                .Where(projectModel => projectModel != projectModelToStartSearchFrom)
                .Select(projectModel => GetClassModelFullyQualified(parts, projectModel))
                .FirstOrDefault();
            return classModel;
        }

// parts contains the class name split in parts
// a fully qualified name is generated and compared with the namespaces found in the provided ProjectModel

        private static IClassType GetClassModelFullyQualified(IReadOnlyList<string> classNameParts,
            ProjectModel projectModel)
        {
            var namespaceName = new StringBuilder();
            var namespaceIndex = 0;
            while (namespaceIndex < classNameParts.Count)
            {
                namespaceName.Append(classNameParts[namespaceIndex]);

                var namespaceModel =
                    projectModel.Namespaces.FirstOrDefault(model => model.Name == namespaceName.ToString());
                if (namespaceModel != null)
                {
                    var className = new StringBuilder();
                    var classIndex = namespaceIndex + 1;
                    while (classIndex < classNameParts.Count)
                    {
                        className.Append(classNameParts[classIndex]);
                        className.Append('.');
                        classIndex++;
                    }

                    var classModel =
                        projectModel.CompilationUnits.SelectMany(c => c.ClassTypes).FirstOrDefault(classType =>
                        {
                            var fullyQualifiedName = $"{namespaceName}.{className}";
                            fullyQualifiedName = fullyQualifiedName.Remove(fullyQualifiedName.Length - 1);

                            return classType.Name == fullyQualifiedName;
                        });
                    if (classModel != null)
                    {
                        return classModel;
                    }
                }

                namespaceIndex++;
                namespaceName.Append('.');
            }

            return null;
        }

        private void AddClassModelToNamespaceGraph(string className, string classFilePath, string namespaceName)
        {
            var namespaceNameParts = namespaceName.Split('.');

            NamespaceTree rootNamespaceTree;

            if (_namespacesDictionary.TryGetValue(namespaceNameParts[0], out var fullNameNamespace))
            {
                rootNamespaceTree = fullNameNamespace;
            }
            else
            {
                var nameNamespace = new NamespaceTree
                {
                    Name = namespaceNameParts[0]
                };
                rootNamespaceTree = nameNamespace;

                _namespacesDictionary.TryAdd(namespaceNameParts[0], rootNamespaceTree);
            }

            if (namespaceNameParts.Length > 1) // create sub graph if namespaceNames is not trivial
            {
                rootNamespaceTree.AddNamespaceChild(namespaceName, namespaceName);
            }

            var classModelFullName = rootNamespaceTree.AddNamespaceChild(className, namespaceName);

            if (classModelFullName == null)
            {
                return;
            }

            classModelFullName.FilePath = classFilePath;

            classModelFullName.GetFullName();
        }
    }
}

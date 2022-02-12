using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.ModelRepresentations;
using HoneydewCore.Processors;
using HoneydewCore.Utils;
using HoneydewModels.Reference;

namespace HoneydewExtractors.Processors;

public class RepositoryModelToFileRelationsProcessor : IProcessorFunction<RepositoryModel, RelationsRepresentation>
{
    private readonly IRelationsMetricChooseStrategy _metricChooseStrategy;

    public RepositoryModelToFileRelationsProcessor(IRelationsMetricChooseStrategy metricChooseStrategy)
    {
        _metricChooseStrategy = metricChooseStrategy;
    }

    public RelationsRepresentation Process(RepositoryModel repositoryModel)
    {
        if (repositoryModel == null)
        {
            return new RelationsRepresentation();
        }

        var fileRelationsRepresentation = new RelationsRepresentation();

        var classFilePaths = new Dictionary<string, List<string>>();

        foreach (var classOption in repositoryModel.GetEnumerable())
        {
            var name = "";
            var compilationUnitTypeFilePath = "";

            switch (classOption)
            {
                case ClassOption.Class(var classModel):
                {
                    name = classModel.Name;
                    compilationUnitTypeFilePath = classModel.File?.FilePath;
                    break;
                }
                case ClassOption.Delegate(var delegateModel):
                {
                    name = delegateModel.Name;
                    compilationUnitTypeFilePath = delegateModel.File?.FilePath;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(name) && string.IsNullOrEmpty(compilationUnitTypeFilePath))
            {
                if (classFilePaths.TryGetValue(name, out var dictionary))
                {
                    dictionary.Add(compilationUnitTypeFilePath);
                }
                else
                {
                    classFilePaths.Add(name, new List<string>
                    {
                        compilationUnitTypeFilePath
                    });
                }
            }
        }

        foreach (var projectModel in repositoryModel.Projects)
        {
            foreach (var compilationUnitType in projectModel.Files)
            {
                var groupBy = compilationUnitType.ClassOptions.GroupBy(option =>
                {
                    return option switch
                    {
                        ClassOption.Class(var classModel) => classModel.FilePath,
                        ClassOption.Delegate(var delegateModel) => delegateModel.FilePath,
                        _ => ""
                    };
                });

                foreach (var grouping in groupBy)
                {
                    var dependencyDictionary = new Dictionary<string, Dictionary<string, int>>();

                    foreach (var option in grouping)
                    {
                        var metricModels = option switch
                        {
                            ClassOption.Class(var classModel) => classModel.Metrics,
                            ClassOption.Delegate(var delegateModel) => delegateModel.Metrics,
                            _ => new List<MetricModel>()
                        };
                        foreach (var metricModel in metricModels)
                        {
                            try
                            {
                                var type = Type.GetType(metricModel.ExtractorName);
                                if (type == null)
                                {
                                    continue;
                                }

                                if (!_metricChooseStrategy.Choose(type))
                                {
                                    continue;
                                }

                                var instance = Activator.CreateInstance(type);
                                if (instance is IRelationVisitor relationVisitor)
                                {
                                    var dictionary = (Dictionary<string, int>)metricModel.Value;
                                    foreach (var (targetName, count) in dictionary)
                                    {
                                        if (CSharpConstants.IsPrimitive(targetName))
                                        {
                                            continue;
                                        }

                                        if (!classFilePaths.TryGetValue(targetName, out var possibleClasses))
                                        {
                                            continue;
                                        }


                                        if (possibleClasses.Count == 1)
                                        {
                                            AddToDependencyDictionary(possibleClasses[0]);
                                        }
                                        else
                                        {
                                            var added = false;
                                            foreach (var possibleClass in possibleClasses)
                                            {
                                                if (possibleClass.Contains(projectModel.FilePath))
                                                {
                                                    AddToDependencyDictionary(possibleClass);
                                                    added = true;
                                                    break;
                                                }
                                            }

                                            if (added)
                                            {
                                                continue;
                                            }

                                            AddToDependencyDictionary(possibleClasses[0]);
                                        }

                                        void AddToDependencyDictionary(string filePath)
                                        {
                                            if (dependencyDictionary.TryGetValue(relationVisitor.PrettyPrint(),
                                                    out var dependency))
                                            {
                                                if (dependency.ContainsKey(filePath))
                                                {
                                                    dependency[filePath] += count;
                                                }
                                                else
                                                {
                                                    dependency.Add(filePath, count);
                                                }
                                            }
                                            else
                                            {
                                                dependencyDictionary.Add(relationVisitor.PrettyPrint(),
                                                    new Dictionary<string, int>
                                                    {
                                                        { filePath, count }
                                                    });
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                // 
                            }
                        }
                    }

                    foreach (var (relationType, dictionary) in dependencyDictionary)
                    {
                        foreach (var (filePath, count) in dictionary)
                        {
                            if (grouping.Key != filePath)
                            {
                                fileRelationsRepresentation.Add(grouping.Key, filePath, relationType, count);
                            }
                        }
                    }
                }
            }
        }

        return fileRelationsRepresentation;
    }
}

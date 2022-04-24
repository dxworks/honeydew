using System;
using System.Collections.Generic;
using System.Linq;
using Honeydew.ModelRepresentations;
using Honeydew.Models.CSharp;
using Honeydew.ScriptBeePlugin.Models;
using ClassModel = Honeydew.ScriptBeePlugin.Models.ClassModel;
using DelegateModel = Honeydew.ScriptBeePlugin.Models.DelegateModel;

namespace Honeydew.Processors;

public class RepositoryModelToFileRelationsProcessor
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

        foreach (var entityModel in repositoryModel.GetEnumerable())
        {
            var name = "";
            var compilationUnitTypeFilePath = "";

            switch (entityModel)
            {
                case ClassModel classModel:
                    name = classModel.Name;
                    compilationUnitTypeFilePath = classModel.File.FilePath;
                    break;
                case DelegateModel delegateModel:
                    name = delegateModel.Name;
                    compilationUnitTypeFilePath = delegateModel.File.FilePath;
                    break;
                case InterfaceModel interfaceModel:
                    name = interfaceModel.Name;
                    compilationUnitTypeFilePath = interfaceModel.File.FilePath;
                    break;
            }

            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(compilationUnitTypeFilePath))
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
                var groupBy = compilationUnitType.Entities.GroupBy(entityModel => entityModel.FilePath);

                foreach (var grouping in groupBy)
                {
                    var dependencyDictionary = new Dictionary<string, Dictionary<string, int>>();

                    foreach (var entityModel in grouping)
                    {
                        foreach (var (metricName, value) in entityModel.GetProperties())
                        {
                            if (value is not Dictionary<string, int> dictionary)
                            {
                                continue;
                            }

                            try
                            {
                                if (!_metricChooseStrategy.Choose(metricName))
                                {
                                    continue;
                                }

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
                                        if (dependencyDictionary.TryGetValue(metricName, out var dependency))
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
                                            dependencyDictionary.Add(metricName, new Dictionary<string, int>
                                            {
                                                { filePath, count }
                                            });
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

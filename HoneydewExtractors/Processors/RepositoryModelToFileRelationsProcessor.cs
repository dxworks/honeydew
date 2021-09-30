using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.ModelRepresentations;
using HoneydewCore.Processors;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels.CSharp;

namespace HoneydewExtractors.Processors
{
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

            var classFilePaths = new Dictionary<Tuple<string, string>, string>();
            
            foreach (var projectModel in repositoryModel.Projects)
            {
                foreach (var compilationUnitType in projectModel.CompilationUnits)
                {
                    foreach (var classType in compilationUnitType.ClassTypes)
                    {
                        classFilePaths.Add(new Tuple<string, string>(classType.Name, projectModel.FilePath),
                            classType.FilePath);
                    }
                }
            }

            foreach (var projectModel in repositoryModel.Projects)
            {
                foreach (var compilationUnitType in projectModel.CompilationUnits)
                {
                    var groupBy = compilationUnitType.ClassTypes.GroupBy(classModel => classModel.FilePath);

                    foreach (var grouping in groupBy)
                    {
                        var dependencyDictionary = new Dictionary<string, Dictionary<string, int>>();

                        foreach (var classType in grouping)
                        {
                            foreach (var metricModel in classType.Metrics)
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

                                            var possibleClasses = classFilePaths
                                                .Where(pair => pair.Key.Item1 == targetName).ToList();

                                            if (possibleClasses.Count == 1)
                                            {
                                                var (_, classFilePath) = possibleClasses[0];

                                                AddToDependencyDictionary(classFilePath);
                                            }
                                            else
                                            {
                                                var projectPaths = new List<string>
                                                {
                                                    projectModel.FilePath,
                                                };
                                                projectPaths.AddRange(projectModel.ProjectReferences);

                                                var (_, classFilePath) = possibleClasses.FirstOrDefault(pair =>
                                                    projectPaths.Contains(pair.Key.Item2));

                                                AddToDependencyDictionary(classFilePath);
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
                                fileRelationsRepresentation.Add(grouping.Key, filePath, relationType, count);
                            }
                        }
                    }
                }
            }

            return fileRelationsRepresentation;
        }
    }
}

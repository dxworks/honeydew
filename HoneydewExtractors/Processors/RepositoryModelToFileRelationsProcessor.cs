using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.ModelRepresentations;
using HoneydewCore.Processors;
using HoneydewCore.Utils;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;
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
            Dictionary<string, string> extractorDict = new Dictionary<string, string>
            {
                {nameof(DeclarationRelationVisitor), new DeclarationRelationVisitor().PrettyPrint()},
                {nameof(ExceptionsThrownRelationVisitor), new ExceptionsThrownRelationVisitor().PrettyPrint()},
                {nameof(ExternCallsRelationVisitor), new ExternCallsRelationVisitor().PrettyPrint()},
                {nameof(ExternDataRelationVisitor), new ExternDataRelationVisitor().PrettyPrint()},
                {nameof(FieldsRelationVisitor), new FieldsRelationVisitor().PrettyPrint()},
                {nameof(HierarchyRelationVisitor), new HierarchyRelationVisitor().PrettyPrint()},
                {nameof(LocalVariablesRelationVisitor), new LocalVariablesRelationVisitor().PrettyPrint()},
                {nameof(ObjectCreationRelationVisitor), new ObjectCreationRelationVisitor().PrettyPrint()},
                {nameof(ParameterRelationVisitor), new ParameterRelationVisitor().PrettyPrint()},
                {nameof(PropertiesRelationVisitor), new PropertiesRelationVisitor().PrettyPrint()},
                {nameof(ReturnValueRelationVisitor), new ReturnValueRelationVisitor().PrettyPrint()},
            };
            
            if (repositoryModel == null)
            {
                return new RelationsRepresentation();
            }

            var fileRelationsRepresentation = new RelationsRepresentation();

            var classFilePaths = new Dictionary<string, List<string>>();

            foreach (var projectModel in repositoryModel.Projects)
            {
                foreach (var compilationUnitType in projectModel.CompilationUnits)
                {
                    foreach (var classType in compilationUnitType.ClassTypes)
                    {
                        if (classFilePaths.TryGetValue(classType.Name, out var dictionary))
                        {
                            dictionary.Add(compilationUnitType.FilePath);
                        }
                        else
                        {
                            classFilePaths.Add(classType.Name, new List<string>
                            {
                                compilationUnitType.FilePath
                            });
                        }
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
                                    if (!_metricChooseStrategy.Choose(metricModel.ExtractorName))
                                    {
                                        continue;
                                    }

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
                                                if (dependencyDictionary.TryGetValue(extractorDict[metricModel.ExtractorName],
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
                                                    dependencyDictionary.Add(extractorDict[metricModel.ExtractorName],
                                                        new Dictionary<string, int>
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
}

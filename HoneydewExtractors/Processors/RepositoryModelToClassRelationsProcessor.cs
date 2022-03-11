using System;
using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewCore.Processors;
using HoneydewCore.Utils;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;
using HoneydewModels.CSharp;

namespace HoneydewExtractors.Processors
{
    public class RepositoryModelToClassRelationsProcessor : IProcessorFunction<RepositoryModel, RelationsRepresentation>
    {
        private readonly IRelationsMetricChooseStrategy _metricChooseStrategy;

        public RepositoryModelToClassRelationsProcessor(IRelationsMetricChooseStrategy metricChooseStrategy)
        {
            _metricChooseStrategy = metricChooseStrategy;
        }

        public RelationsRepresentation Process(RepositoryModel repositoryModel)
        {
            Dictionary<string, string> extractorDict = new Dictionary<string, string>
            {
                { typeof(DeclarationRelationVisitor).FullName, new DeclarationRelationVisitor().PrettyPrint() },
                {
                    typeof(ExceptionsThrownRelationVisitor).FullName,
                    new ExceptionsThrownRelationVisitor().PrettyPrint()
                },
                { typeof(ExternCallsRelationVisitor).FullName, new ExternCallsRelationVisitor().PrettyPrint() },
                { typeof(ExternDataRelationVisitor).FullName, new ExternDataRelationVisitor().PrettyPrint() },
                { typeof(FieldsRelationVisitor).FullName, new FieldsRelationVisitor().PrettyPrint() },
                { typeof(HierarchyRelationVisitor).FullName, new HierarchyRelationVisitor().PrettyPrint() },
                { typeof(LocalVariablesRelationVisitor).FullName, new LocalVariablesRelationVisitor().PrettyPrint() },
                { typeof(ObjectCreationRelationVisitor).FullName, new ObjectCreationRelationVisitor().PrettyPrint() },
                { typeof(ParameterRelationVisitor).FullName, new ParameterRelationVisitor().PrettyPrint() },
                { typeof(PropertiesRelationVisitor).FullName, new PropertiesRelationVisitor().PrettyPrint() },
                { typeof(ReturnValueRelationVisitor).FullName, new ReturnValueRelationVisitor().PrettyPrint() },
            };

            if (repositoryModel == null)
            {
                return new RelationsRepresentation();
            }

            var classRelationsRepresentation = new RelationsRepresentation();

            foreach (var classType in repositoryModel.GetEnumerable())
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

                            classRelationsRepresentation.Add(classType.Name, targetName,
                                extractorDict[metricModel.ExtractorName],
                                count);
                        }
                    }
                    catch (Exception)
                    {
                        // 
                    }
                }
            }

            return classRelationsRepresentation;
        }
    }
}

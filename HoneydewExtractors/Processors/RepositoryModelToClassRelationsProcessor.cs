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

                        var dictionary = (Dictionary<string, int>) metricModel.Value;
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
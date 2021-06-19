using System;
using HoneydewCore.Extractors.Metrics;
using HoneydewCore.Models;
using HoneydewCore.Models.Representations;

namespace HoneydewCore.Processors
{
    public class SolutionModelToFileRelationsProcessor : IProcessorFunction<SolutionModel, FileRelationsRepresentation>
    {
        public Func<Processable<SolutionModel>, Processable<FileRelationsRepresentation>> GetFunction()
        {
            return processable =>
            {
                var solutionModel = processable.Value;

                var fileRelationsRepresentation = new FileRelationsRepresentation();

                if (solutionModel == null)
                    return new Processable<FileRelationsRepresentation>(fileRelationsRepresentation);

                foreach (var classModel in solutionModel.GetEnumerable())
                {
                    if (classModel.Metrics.Count == 0)
                    {
                        fileRelationsRepresentation.Add(classModel.FullName);
                    }
                    else
                    {
                        foreach (var classMetric in classModel.Metrics)
                        {
                            var extractorType = Type.GetType(classMetric.ExtractorName);
                            if (extractorType == null)
                            {
                                continue;
                            }

                            if (!typeof(IRelationMetric).IsAssignableFrom(extractorType)) continue;

                            var relationMetric = (IRelationMetric) Activator.CreateInstance(extractorType);

                            if (relationMetric == null) continue;

                            var relations = relationMetric.GetRelations(classMetric.Value);
                            foreach (var relation in relations)
                            {
                                fileRelationsRepresentation.Add(classModel.FullName, relation.FileTarget,
                                    relation.RelationType, relation.RelationCount);
                            }
                        }
                    }
                }

                return new Processable<FileRelationsRepresentation>(fileRelationsRepresentation);
            };
        }
    }
}
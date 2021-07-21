using System;
using HoneydewCore.Extractors.Metrics;
using HoneydewCore.Models;
using HoneydewCore.Models.Representations;
using HoneydewModels;

namespace HoneydewCore.Processors
{
    public class
        RepositoryModelToClassRelationsProcessor : IProcessorFunction<RepositoryModel, ClassRelationsRepresentation>
    {
        public Func<Processable<RepositoryModel>, Processable<ClassRelationsRepresentation>> GetFunction()
        {
            return processable =>
            {
                var repositoryModel = processable.Value;

                var classRelationsRepresentation = new ClassRelationsRepresentation();

                if (repositoryModel == null)
                    return new Processable<ClassRelationsRepresentation>(classRelationsRepresentation);

                foreach (var classModel in repositoryModel.GetEnumerable())
                {
                    if (classModel.Metrics.Count == 0)
                    {
                        classRelationsRepresentation.Add(classModel.FullName);
                    }
                    else
                    {
                        foreach (var classMetric in classModel.Metrics)
                        {
                            var extractorType = Type.GetType(classMetric.ExtractorName);
                            if (extractorType == null || !typeof(IRelationMetric).IsAssignableFrom(extractorType))
                            {
                                continue;
                            }

                            var relationMetric = (IRelationMetric) Activator.CreateInstance(extractorType);

                            if (relationMetric == null) continue;

                            var relations = relationMetric.GetRelations(classMetric.Value);
                            foreach (var relation in relations)
                            {
                                classRelationsRepresentation.Add(classModel.FullName, relation.FileTarget,
                                    relation.RelationType, relation.RelationCount);
                            }
                        }
                    }
                }

                return new Processable<ClassRelationsRepresentation>(classRelationsRepresentation);
            };
        }
    }
}

using HoneydewCore.ModelRepresentations;
using HoneydewModels.CSharp;

namespace HoneydewCore.Processors
{
    public class
        RepositoryModelToClassRelationsProcessor : IProcessorFunction<RepositoryModel, ClassRelationsRepresentation>
    {
        private readonly IMetricRelationsProvider _metricRelationsProvider;
        private readonly IMetricPrettier _metricPrettier;
        private readonly bool _usePrettyPrint;

        public RepositoryModelToClassRelationsProcessor(IMetricRelationsProvider metricRelationsProvider,
            IMetricPrettier metricPrettier,
            bool usePrettyPrint)
        {
            _metricRelationsProvider = metricRelationsProvider;
            _metricPrettier = metricPrettier;
            _usePrettyPrint = usePrettyPrint;
        }

        public ClassRelationsRepresentation Process(RepositoryModel repositoryModel)
        {
            var classRelationsRepresentation = new ClassRelationsRepresentation(_metricPrettier)
            {
                UsePrettyPrint = _usePrettyPrint
            };

            if (repositoryModel == null)
                return classRelationsRepresentation;

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
                        var relations = _metricRelationsProvider.GetFileRelations(classMetric);
                        foreach (var relation in relations)
                        {
                            classRelationsRepresentation.Add(classModel.FullName, relation.FileTarget,
                                relation.RelationType, relation.RelationCount);
                        }
                    }
                }
            }

            return classRelationsRepresentation;
        }
    }
}

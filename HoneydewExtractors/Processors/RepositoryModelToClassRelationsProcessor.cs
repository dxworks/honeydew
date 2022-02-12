using System;
using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewCore.Processors;
using HoneydewCore.Utils;
using HoneydewModels.Reference;

namespace HoneydewExtractors.Processors;

public class RepositoryModelToClassRelationsProcessor : IProcessorFunction<RepositoryModel, RelationsRepresentation>
{
    private readonly IRelationsMetricChooseStrategy _metricChooseStrategy;

    public RepositoryModelToClassRelationsProcessor(IRelationsMetricChooseStrategy metricChooseStrategy)
    {
        _metricChooseStrategy = metricChooseStrategy;
    }

    public RelationsRepresentation Process(RepositoryModel repositoryModel)
    {
        if (repositoryModel == null)
        {
            return new RelationsRepresentation();
        }

        var classRelationsRepresentation = new RelationsRepresentation();

        foreach (var classOption in repositoryModel.GetEnumerable())
        {
            var name = "";
            IList<MetricModel> metricModels = new List<MetricModel>();
            switch (classOption)
            {
                case ClassOption.Class(var classModel):
                {
                    metricModels = classModel.Metrics;
                    name = classModel.Name;
                    break;
                }
                case ClassOption.Delegate(var delegateModel):
                {
                    metricModels = delegateModel.Metrics;
                    name = delegateModel.Name;
                    break;
                }
            }

            if (string.IsNullOrEmpty(name))
            {
                continue;
            }

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

                            classRelationsRepresentation.Add(name, targetName, relationVisitor.PrettyPrint(),
                                count);
                        }
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

using Honeydew.ModelRepresentations;
using Honeydew.Models.CSharp;
using DxWorks.ScriptBee.Plugins.Honeydew.Models;

namespace Honeydew.Processors;

public class RepositoryModelToClassRelationsProcessor
{
    private readonly IRelationsMetricChooseStrategy _metricChooseStrategy;

    public RepositoryModelToClassRelationsProcessor(IRelationsMetricChooseStrategy metricChooseStrategy)
    {
        _metricChooseStrategy = metricChooseStrategy;
    }

    public RelationsRepresentation Process(RepositoryModel? repositoryModel)
    {
        if (repositoryModel == null)
        {
            return new RelationsRepresentation();
        }

        var classRelationsRepresentation = new RelationsRepresentation();

        foreach (var entityModel in repositoryModel.GetEnumerable())
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

                        classRelationsRepresentation.Add(entityModel.Name, targetName, metricName, count);
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

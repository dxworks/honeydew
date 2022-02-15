using System;
using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewCore.Processors;
using HoneydewCore.Utils;
using HoneydewModels.Reference;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;
using HoneydewModels.CSharp;

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
        Dictionary<string, string> extractorDict = new Dictionary<string, string>
        {
            { nameof(DeclarationRelationVisitor), new DeclarationRelationVisitor().PrettyPrint() },
            { nameof(ExceptionsThrownRelationVisitor), new ExceptionsThrownRelationVisitor().PrettyPrint() },
            { nameof(ExternCallsRelationVisitor), new ExternCallsRelationVisitor().PrettyPrint() },
            { nameof(ExternDataRelationVisitor), new ExternDataRelationVisitor().PrettyPrint() },
            { nameof(FieldsRelationVisitor), new FieldsRelationVisitor().PrettyPrint() },
            { nameof(HierarchyRelationVisitor), new HierarchyRelationVisitor().PrettyPrint() },
            { nameof(LocalVariablesRelationVisitor), new LocalVariablesRelationVisitor().PrettyPrint() },
            { nameof(ObjectCreationRelationVisitor), new ObjectCreationRelationVisitor().PrettyPrint() },
            { nameof(ParameterRelationVisitor), new ParameterRelationVisitor().PrettyPrint() },
            { nameof(PropertiesRelationVisitor), new PropertiesRelationVisitor().PrettyPrint() },
            { nameof(ReturnValueRelationVisitor), new ReturnValueRelationVisitor().PrettyPrint() },
        };

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

        return classRelationsRepresentation;
}

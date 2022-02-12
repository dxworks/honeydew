using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;

public abstract class RelationVisitor : IRelationVisitor, ICSharpClassVisitor
{
    protected readonly IRelationMetricHolder MetricHolder;

    protected RelationVisitor()
    {
    }

    protected RelationVisitor(IRelationMetricHolder metricHolder)
    {
        MetricHolder = metricHolder;
    }

    public abstract string PrettyPrint();

    protected abstract void AddDependencies(string className, BaseTypeDeclarationSyntax syntaxNode,
        SemanticModel semanticModel);


    public void Accept(IVisitor visitor)
    {
    }

    public IList<Relation> GetRelations(
        IDictionary<string, IDictionary<IRelationVisitor, IDictionary<string, int>>> dependencies)
    {
        return MetricHolder.GetRelations();
    }

    public IClassType Visit(BaseTypeDeclarationSyntax syntaxNode, SemanticModel semanticModel, IClassType modelType)
    {
        var className = CSharpExtractionHelperMethods.GetFullName(syntaxNode, semanticModel).Name;

        AddDependencies(className, syntaxNode, semanticModel);

        var metricModel = GetMetricModel(className);
        modelType.Metrics.Add(metricModel);
        return modelType;
    }

    private MetricModel GetMetricModel(string className)
    {
        if (MetricHolder.GetDependencies(className).TryGetValue(PrettyPrint(), out var dictionary))
        {
            return new MetricModel
            {
                ExtractorName = GetType().ToString(),
                Value = dictionary,
                ValueType = dictionary.GetType().ToString()
            };
        }

        var dependencies = new Dictionary<string, int>();
        var metricModel = new MetricModel
        {
            ExtractorName = GetType().ToString(),
            Value = dependencies,
            ValueType = dependencies.GetType().ToString()
        };
        return metricModel;
    }
}

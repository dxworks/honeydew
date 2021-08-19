using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Class.Relations
{
    public abstract class RelationMetricVisitor : ExtractionVisitor<CSharpSyntacticModel, CSharpSemanticModel>,
        IRelationMetric, ICSharpClassVisitor
    {
        protected readonly IRelationMetricHolder MetricHolder;

        protected RelationMetricVisitor(IRelationMetricHolder metricHolder)
        {
            MetricHolder = metricHolder;
        }

        public abstract string PrettyPrint();

        protected abstract void AddDependencies(string className, BaseTypeDeclarationSyntax syntaxNode);

        public IList<FileRelation> GetRelations(IDictionary<string, IDictionary<string, int>> dependencies)
        {
            return MetricHolder.GetRelations(dependencies);
        }

        public IPropertyMembersClassType Visit(BaseTypeDeclarationSyntax syntaxNode,
            IPropertyMembersClassType modelType)
        {
            var className = syntaxNode.Identifier.ToString();

            AddDependencies(className, syntaxNode);

            var metricModel = GetMetricModel(className);
            modelType.Metrics.Add(metricModel);
            return modelType;
        }

        private MetricModel GetMetricModel(string className)
        {
            var dependencies = MetricHolder.GetDependencies(className);

            var metricModel = new MetricModel
            {
                ExtractorName = GetType().ToString(),
                Value = dependencies,
                ValueType = dependencies.GetType().ToString()
            };
            return metricModel;
        }
    }
}

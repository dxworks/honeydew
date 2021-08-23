﻿using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.CSharp.Metrics.Extraction;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Class.Relations
{
    public abstract class RelationMetricVisitor : IRequireCSharpExtractionHelperMethodsVisitor,
        IRelationMetric, ICSharpClassVisitor
    {
        public CSharpExtractionHelperMethods CSharpHelperMethods { get; set; }

        protected readonly IRelationMetricHolder MetricHolder;

        protected RelationMetricVisitor(IRelationMetricHolder metricHolder)
        {
            MetricHolder = metricHolder;
        }

        public abstract string PrettyPrint();

        protected abstract void AddDependencies(string className, BaseTypeDeclarationSyntax syntaxNode);


        public void Accept(IVisitor visitor)
        {
        }

        public IList<FileRelation> GetRelations(
            IDictionary<string, IDictionary<IRelationMetric, IDictionary<string, int>>> dependencies)
        {
            return MetricHolder.GetRelations();
        }

        public IClassType Visit(BaseTypeDeclarationSyntax syntaxNode, IClassType modelType)
        {
            var className = CSharpHelperMethods.GetFullName(syntaxNode);

            AddDependencies(className, syntaxNode);

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

            Dictionary<string, int> dependencies = new Dictionary<string, int>();
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

﻿using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewCore.Extractors.Metrics.SemanticMetrics
{
    public class ParameterDependenciesMetric : CSharpMetricExtractor, ISemanticMetric, ISyntacticMetric
    {
        public DependencyDataMetric DataMetric { get; set; } = new();

        public override IMetric GetMetric()
        {
            return new Metric<DependencyDataMetric>(DataMetric);
        }

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            DataMetric.Usings.Add(node.Name.ToString());
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            foreach (var parameterSyntax in node.ParameterList.Parameters)
            {
                if (parameterSyntax.Type == null) continue;

                var parameterType = parameterSyntax.Type.ToString();
                if (DataMetric.Dependencies.ContainsKey(parameterType))
                {
                    DataMetric.Dependencies[parameterType]++;
                }
                else
                {
                    DataMetric.Dependencies.Add(parameterType, 1);
                }
            }
        }
    }
}
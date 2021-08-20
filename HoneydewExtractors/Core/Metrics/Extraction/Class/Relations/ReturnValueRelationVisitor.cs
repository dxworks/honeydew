﻿using System.Linq;
using HoneydewCore.ModelRepresentations;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Class.Relations
{
    public class ReturnValueRelationVisitor : RelationMetricVisitor
    {
        public ReturnValueRelationVisitor(IRelationMetricHolder metricHolder) : base(metricHolder)
        {
        }

        public override string PrettyPrint()
        {
            return "Return Value Dependency";
        }

        protected override void AddDependencies(string className, BaseTypeDeclarationSyntax syntaxNode)
        {
            foreach (var methodDeclarationSyntax in syntaxNode.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                MetricHolder.Add(className,
                    InheritedSemanticModel.GetFullName(methodDeclarationSyntax.ReturnType), this);
            }
        }
    }
}
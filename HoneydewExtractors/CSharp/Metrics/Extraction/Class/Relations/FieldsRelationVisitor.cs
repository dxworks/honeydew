﻿using System.Linq;
using HoneydewCore.ModelRepresentations;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations
{
    public class FieldsRelationVisitor : RelationVisitor
    {
        public FieldsRelationVisitor()
        {
        }

        public FieldsRelationVisitor(IRelationMetricHolder metricHolder) : base(metricHolder)
        {
        }

        public override string PrettyPrint()
        {
            return "Fields Dependency";
        }

        protected override void AddDependencies(string className, BaseTypeDeclarationSyntax syntaxNode)
        {
            foreach (var fieldDeclarationSyntax in syntaxNode.DescendantNodes().OfType<FieldDeclarationSyntax>())
            {
                MetricHolder.Add(className,
                    CSharpHelperMethods.GetFullName(fieldDeclarationSyntax.Declaration.Type).Name, this);
            }

            foreach (var eventFieldDeclarationSyntax in syntaxNode.DescendantNodes()
                .OfType<EventFieldDeclarationSyntax>())
            {
                MetricHolder.Add(className,
                    CSharpHelperMethods.GetFullName(eventFieldDeclarationSyntax.Declaration.Type).Name, this);
            }
        }
    }
}
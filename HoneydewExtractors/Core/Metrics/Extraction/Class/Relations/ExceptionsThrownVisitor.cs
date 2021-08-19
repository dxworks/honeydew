using System.Collections.Generic;
using System.Linq;
using HoneydewCore.ModelRepresentations;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Class.Relations
{
    public class ExceptionsThrownVisitor : ExtractionVisitor<CSharpSyntacticModel, CSharpSemanticModel>,
        IRelationMetric, ICSharpClassVisitor
    {
        private readonly IRelationMetricHolder _relationMetricHolder;

        public ExceptionsThrownVisitor(IRelationMetricHolder relationMetricHolder)
        {
            _relationMetricHolder = relationMetricHolder;
        }

        public string PrettyPrint()
        {
            return "Exceptions Thrown Dependency";
        }

        public IList<FileRelation> GetRelations(IDictionary<string, IDictionary<string, int>> dependencies)
        {
            return _relationMetricHolder.GetRelations(dependencies);
        }

        public IPropertyMembersClassType Visit(BaseTypeDeclarationSyntax syntaxNode,
            IPropertyMembersClassType modelType)
        {
            var className = syntaxNode.Identifier.ToString();

            foreach (var throwExpressionSyntax in syntaxNode.DescendantNodes().OfType<ThrowExpressionSyntax>())
            {
                _relationMetricHolder.Add(className,
                    InheritedSemanticModel.GetFullName(throwExpressionSyntax.Expression));
            }

            foreach (var throwStatementSyntax in syntaxNode.DescendantNodes().OfType<ThrowStatementSyntax>())
            {
                _relationMetricHolder.Add(className,
                    throwStatementSyntax.Expression == null
                        ? InheritedSemanticModel.GetFullName(throwStatementSyntax)
                        : InheritedSemanticModel.GetFullName(throwStatementSyntax.Expression));
            }

            var dependencies = _relationMetricHolder.GetDependencies(className);

            modelType.Metrics.Add(new MetricModel
            {
                ExtractorName = GetType().ToString(),
                Value = dependencies,
                ValueType = dependencies.GetType().ToString()
            });

            return modelType;
        }
    }
}

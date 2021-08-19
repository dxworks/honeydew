using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Class.Relations
{
    public class ExceptionsThrownVisitor : RelationMetricVisitor
    {
        public ExceptionsThrownVisitor(IRelationMetricHolder metricHolder) : base(metricHolder)
        {
        }

        public override string PrettyPrint()
        {
            return "Exceptions Thrown Dependency";
        }

        protected override void AddDependencies(string className, BaseTypeDeclarationSyntax syntaxNode)
        {
            foreach (var throwExpressionSyntax in syntaxNode.DescendantNodes().OfType<ThrowExpressionSyntax>())
            {
                MetricHolder.Add(className,
                    InheritedSemanticModel.GetFullName(throwExpressionSyntax.Expression));
            }

            foreach (var throwStatementSyntax in syntaxNode.DescendantNodes().OfType<ThrowStatementSyntax>())
            {
                MetricHolder.Add(className,
                    throwStatementSyntax.Expression == null
                        ? InheritedSemanticModel.GetFullName(throwStatementSyntax)
                        : InheritedSemanticModel.GetFullName(throwStatementSyntax.Expression));
            }
        }
    }
}

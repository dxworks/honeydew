using System.Linq;
using HoneydewCore.ModelRepresentations;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations
{
    public class ExceptionsThrownRelationVisitor : RelationVisitor
    {
        public ExceptionsThrownRelationVisitor()
        {
        }

        public ExceptionsThrownRelationVisitor(IRelationMetricHolder metricHolder) : base(metricHolder)
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
                    CSharpHelperMethods.GetFullName(throwExpressionSyntax.Expression).Name, this);
            }

            foreach (var throwStatementSyntax in syntaxNode.DescendantNodes().OfType<ThrowStatementSyntax>())
            {
                MetricHolder.Add(className,
                    throwStatementSyntax.Expression == null
                        ? CSharpHelperMethods.GetFullName(throwStatementSyntax).Name
                        : CSharpHelperMethods.GetFullName(throwStatementSyntax.Expression).Name, this);
            }
        }
    }
}

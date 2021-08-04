using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.RelationMetric
{
    public class CSharpExceptionsThrownRelationMetric : CSharpRelationMetric
    {
        public override string PrettyPrint()
        {
            return "Exceptions Thrown Dependency";
        }

        public override void VisitThrowStatement(ThrowStatementSyntax node)
        {
            if (node.Expression == null)
            {
                AddDependency(HoneydewSemanticModel.GetFullName(node));
            }
            else
            {
                AddDependency(HoneydewSemanticModel.GetFullName(node.Expression));
            }
        }

        public override void VisitThrowExpression(ThrowExpressionSyntax node)
        {
            AddDependency(HoneydewSemanticModel.GetFullName(node.Expression));
        }
    }
}

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.RelationMetric
{
    public class CSharpObjectCreationRelationMetric : CSharpRelationMetric
    {
        public override string PrettyPrint()
        {
            return "Object Creation Dependency";
        }

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            AddDependency(HoneydewSemanticModel.GetFullName(node.Type));
        }

        public override void VisitImplicitObjectCreationExpression(ImplicitObjectCreationExpressionSyntax node)
        {
            AddDependency(HoneydewSemanticModel.GetFullName(node));
        }

        public override void VisitArrayCreationExpression(ArrayCreationExpressionSyntax node)
        {
            AddDependency(HoneydewSemanticModel.GetFullName(node.Type));
        }

        public override void VisitImplicitArrayCreationExpression(ImplicitArrayCreationExpressionSyntax node)
        {
            AddDependency(HoneydewSemanticModel.GetFullName(node));
        }
    }
}

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.RelationMetric
{
    public class CSharpReturnValueRelationMetric : CSharpRelationMetric
    {
        public override string PrettyPrint()
        {
            return "Return Value Dependency";
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            AddDependency(HoneydewSemanticModel.GetFullName(node.ReturnType));
        }
    }
}

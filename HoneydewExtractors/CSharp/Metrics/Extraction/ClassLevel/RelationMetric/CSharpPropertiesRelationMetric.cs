using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.RelationMetric
{
    public class CSharpPropertiesRelationMetric : CSharpRelationMetric
    {
        public override string PrettyPrint()
        {
            return "Properties Dependency";
        }

        public override void VisitEventDeclaration(EventDeclarationSyntax node)
        {
            AddDependency(HoneydewSemanticModel.GetFullName(node.Type));
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            AddDependency(HoneydewSemanticModel.GetFullName(node.Type));
        }
    }
}

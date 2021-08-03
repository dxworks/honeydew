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
            AddPropertyInfo(node, node.Identifier.ToString(), true);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            AddPropertyInfo(node, node.Identifier.ToString(), false);
        }

        private void AddPropertyInfo(BasePropertyDeclarationSyntax node, string name, bool isEvent)
        {
            AddDependency(HoneydewSemanticModel.GetFullName(node.Type));
        }
    }
}

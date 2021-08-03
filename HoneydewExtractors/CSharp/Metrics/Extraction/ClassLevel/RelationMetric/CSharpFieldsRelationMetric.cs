using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.RelationMetric
{
    public class CSharpFieldsRelationMetric : CSharpRelationMetric
    {
        public override string PrettyPrint()
        {
            return "Fields Dependency";
        }

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            AddDependency(HoneydewSemanticModel.GetFullName(node.Declaration.Type));
        }

        public override void VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
        {
            AddDependency(HoneydewSemanticModel.GetFullName(node.Declaration.Type));
        }
    }
}

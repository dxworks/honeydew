using System.Linq;
using HoneydewCore.ModelRepresentations;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations
{
    public class PropertiesRelationVisitor : RelationVisitor
    {
        public PropertiesRelationVisitor()
        {
        }

        public PropertiesRelationVisitor(IRelationMetricHolder metricHolder) : base(metricHolder)
        {
        }

        public override string PrettyPrint()
        {
            return "Properties Dependency";
        }

        protected override void AddDependencies(string className, BaseTypeDeclarationSyntax syntaxNode)
        {
            foreach (var propertyDeclarationSyntax in syntaxNode.DescendantNodes().OfType<BasePropertyDeclarationSyntax>())
            {
                MetricHolder.Add(className,
                    CSharpHelperMethods.GetFullName(propertyDeclarationSyntax.Type).Name, this);
            }
        }
    }
}

using System.Linq;
using HoneydewCore.ModelRepresentations;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations
{
    public class ParameterRelationVisitor : RelationVisitor
    {
        public ParameterRelationVisitor()
        {
        }

        public ParameterRelationVisitor(IRelationMetricHolder metricHolder) : base(metricHolder)
        {
        }

        public override string PrettyPrint()
        {
            return "Parameter Dependency";
        }

        protected override void AddDependencies(string className, BaseTypeDeclarationSyntax syntaxNode)
        {
            foreach (var baseMethodDeclarationSyntax in syntaxNode.DescendantNodes()
                .OfType<BaseMethodDeclarationSyntax>())
            {
                foreach (var parameterSyntax in baseMethodDeclarationSyntax.ParameterList.Parameters)
                {
                    MetricHolder.Add(className, CSharpHelperMethods.GetFullName(parameterSyntax.Type).Name, this);
                }
            }
        }
    }
}

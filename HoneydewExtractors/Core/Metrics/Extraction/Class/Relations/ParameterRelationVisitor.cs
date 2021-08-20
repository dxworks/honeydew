using System.Linq;
using HoneydewCore.ModelRepresentations;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Class.Relations
{
    public class ParameterRelationVisitor : RelationMetricVisitor
    {
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
                    MetricHolder.Add(className, InheritedSemanticModel.GetFullName(parameterSyntax.Type), this);
                }
            }
        }
    }
}

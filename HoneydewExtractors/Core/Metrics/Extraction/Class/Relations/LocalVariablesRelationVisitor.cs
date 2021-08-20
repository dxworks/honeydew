using System.Linq;
using HoneydewExtractors.CSharp.Utils;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Class.Relations
{
    public class LocalVariablesRelationVisitor : RelationMetricVisitor
    {
        public LocalVariablesRelationVisitor(IRelationMetricHolder metricHolder) : base(metricHolder)
        {
        }

        public override string PrettyPrint()
        {
            return "Local Variables Dependency";
        }

        protected override void AddDependencies(string className, BaseTypeDeclarationSyntax syntaxNode)
        {
            foreach (var variableDeclarationSyntax in syntaxNode.DescendantNodes().OfType<BaseMethodDeclarationSyntax>()
                .SelectMany(syntax => syntax.DescendantNodes().OfType<VariableDeclarationSyntax>())
            )
            {
                var fullName = InheritedSemanticModel.GetFullName(variableDeclarationSyntax.Type);

                if (fullName != CSharpConstants.VarIdentifier)
                {
                    MetricHolder.Add(className, fullName);
                }
                else
                {
                    fullName = InheritedSemanticModel.GetFullName(variableDeclarationSyntax);
                    if (fullName != CSharpConstants.VarIdentifier)
                    {
                        MetricHolder.Add(className, fullName);
                    }
                    else
                    {
                        foreach (var declarationVariable in variableDeclarationSyntax.Variables)
                        {
                            if (declarationVariable.Initializer is
                            {
                                Value: ObjectCreationExpressionSyntax
                                objectCreationExpressionSyntax
                            })
                            {
                                MetricHolder.Add(className,
                                    InheritedSemanticModel.GetFullName(objectCreationExpressionSyntax.Type));
                            }
                        }
                    }
                }
            }
        }
    }
}

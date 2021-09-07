using System.Linq;
using HoneydewCore.ModelRepresentations;
using HoneydewExtractors.CSharp.Utils;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations
{
    public class LocalVariablesRelationVisitor : RelationVisitor
    {
        public LocalVariablesRelationVisitor()
        {
        }

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
                var fullName = CSharpHelperMethods.GetFullName(variableDeclarationSyntax.Type).Name;

                if (fullName != CSharpConstants.VarIdentifier)
                {
                    MetricHolder.Add(className, fullName, this);
                }
                else
                {
                    fullName = CSharpHelperMethods.GetFullName(variableDeclarationSyntax).Name;
                    if (fullName != CSharpConstants.VarIdentifier)
                    {
                        MetricHolder.Add(className, fullName, this);
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
                                    CSharpHelperMethods.GetFullName(objectCreationExpressionSyntax.Type).Name, this);
                            }
                            else if (declarationVariable.Initializer != null)
                            {
                                MetricHolder.Add(className,
                                    CSharpHelperMethods.GetFullName(declarationVariable.Initializer.Value).Name, this);
                            }
                        }
                    }
                }
            }
        }
    }
}

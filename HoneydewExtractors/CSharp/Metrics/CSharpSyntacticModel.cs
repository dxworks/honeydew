using System.Linq;
using HoneydewExtractors.Core.Metrics;
using HoneydewExtractors.CSharp.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics
{
    public class CSharpSyntacticModel : ISyntacticModel
    {
        public SyntaxTree Tree { get; set; }
        public CompilationUnitSyntax CompilationUnitSyntax { get; set; }

        public string SetTypeModifier(string typeString, string modifier)
        {
            if (typeString.StartsWith(CSharpConstants.RefReadonlyIdentifier))
            {
                modifier += $" {CSharpConstants.RefReadonlyIdentifier}";
                modifier = modifier.Trim();
            }
            else if (typeString.StartsWith(CSharpConstants.RefIdentifier))
            {
                modifier += $" {CSharpConstants.RefIdentifier}";
                modifier = modifier.Trim();
            }

            return modifier;
        }
        
        public int CalculateCyclomaticComplexity(MemberDeclarationSyntax syntax)
        {
            var count = 1;

            foreach (var descendantNode in syntax.DescendantNodes())
            {
                switch (descendantNode)
                {
                    case WhileStatementSyntax whileStatementSyntax:
                    {
                        count += CalculateCyclomaticComplexity(whileStatementSyntax.Condition);
                    }
                        break;
                    case IfStatementSyntax ifStatementSyntax:
                    {
                        count += CalculateCyclomaticComplexity(ifStatementSyntax.Condition);
                    }
                        break;
                    case ForStatementSyntax forStatementSyntax:
                    {
                        count += CalculateCyclomaticComplexity(forStatementSyntax.Condition);
                    }
                        break;
                    case DoStatementSyntax:
                    case CaseSwitchLabelSyntax:
                    case DefaultSwitchLabelSyntax:
                    case CasePatternSwitchLabelSyntax:
                    case ForEachStatementSyntax:
                    case ConditionalExpressionSyntax:
                    case ConditionalAccessExpressionSyntax:
                        count++;
                        break;
                    default:
                    {
                        switch (descendantNode.Kind())
                        {
                            case SyntaxKind.CoalesceAssignmentExpression:
                            case SyntaxKind.CoalesceExpression:
                                count++;
                                break;
                        }
                    }
                        break;
                }
            }

            return count;
        }

        private int CalculateCyclomaticComplexity(ExpressionSyntax conditionExpressionSyntax)
        {
            if (conditionExpressionSyntax == null)
            {
                return 1;
            }

            var logicalOperatorsCount = conditionExpressionSyntax
                .DescendantTokens()
                .Count(token =>
                {
                    var syntaxKind = token.Kind();
                    return syntaxKind is SyntaxKind.AmpersandAmpersandToken or SyntaxKind.BarBarToken or SyntaxKind
                        .AndKeyword or SyntaxKind.OrKeyword;
                });

            return logicalOperatorsCount + 1;
        }
    }
}

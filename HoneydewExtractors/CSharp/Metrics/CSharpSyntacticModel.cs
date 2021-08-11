using System.Linq;
using HoneydewExtractors.Core.Metrics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics
{
    public class CSharpSyntacticModel : ISyntacticModel
    {
        public SyntaxTree Tree { get; set; }
        public CompilationUnitSyntax CompilationUnitSyntax { get; set; }

        public int CalculateCyclomaticComplexity(MemberDeclarationSyntax syntax)
        {
            var count = 0;

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
                    case CasePatternSwitchLabelSyntax:
                    case ReturnStatementSyntax:
                        // case ElseClauseSyntax:
                        count++;
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

            var binaryExpressionSyntaxCount = conditionExpressionSyntax
                .DescendantTokens()
                .Count(token =>
                {
                    var syntaxKind = token.Kind();
                    return syntaxKind is SyntaxKind.AmpersandAmpersandToken or SyntaxKind.BarBarToken;
                });

            return binaryExpressionSyntaxCount + 1;
        }
    }
}

using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Visitors.LocalVariables;

public interface ICSharpLocalVariablesVisitor : ILocalVariablesVisitor,
    IExtractionVisitor<VariableDeclaratorSyntax, SemanticModel, ILocalVariableType>,
    IExtractionVisitor<DeclarationPatternSyntax, SemanticModel, ILocalVariableType>,
    IExtractionVisitor<ForEachStatementSyntax, SemanticModel, ILocalVariableType>
{
}

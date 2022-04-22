using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors;

public interface ICSharpLocalVariablesVisitor : ILocalVariablesVisitor,
    IExtractionVisitor<VariableDeclaratorSyntax, SemanticModel, ILocalVariableType>,
    IExtractionVisitor<DeclarationPatternSyntax, SemanticModel, ILocalVariableType>,
    IExtractionVisitor<ForEachStatementSyntax, SemanticModel, ILocalVariableType>
{
}

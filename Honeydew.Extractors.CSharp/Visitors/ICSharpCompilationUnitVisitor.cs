using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Honeydew.Extractors.CSharp.Visitors;

public interface
    ICSharpCompilationUnitVisitor : ICSharpExtractionVisitor<CSharpSyntaxNode, SemanticModel, ICompilationUnitType>
{
}

using Honeydew.Extractors.Visitors;
using Microsoft.CodeAnalysis.CSharp;

namespace Honeydew.Extractors.CSharp;

public interface ICSharpExtractionVisitor<in TSyntaxNode, in TSemanticNode, TType> : IExtractionVisitor<TSyntaxNode, TSemanticNode, TType>
    where TSyntaxNode : CSharpSyntaxNode
{
}

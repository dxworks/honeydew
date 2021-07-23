using HoneydewExtractors.Metrics.Extraction;
using Microsoft.CodeAnalysis;

namespace HoneydewExtractors.Metrics.CSharp
{
    public class CSharpSyntaxNode : ISyntaxNode
    {
        public readonly SyntaxNode? Node;

        public CSharpSyntaxNode(SyntaxNode? node)
        {
            Node = node;
        }
    }
}

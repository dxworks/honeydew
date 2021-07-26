using HoneydewExtractors.Core.Metrics;
using Microsoft.CodeAnalysis;

namespace HoneydewExtractors.CSharp.Metrics
{
    public class CSharpSyntaxNode : ISyntaxNode
    {
        public readonly SyntaxNode Node;

        public CSharpSyntaxNode(SyntaxNode node)
        {
            Node = node;
        }
    }
}

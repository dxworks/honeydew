using Microsoft.CodeAnalysis;

namespace Honeydew.Extractors.CSharp.Visitors.Utils;

public static class SyntaxNodeExtensions
{
    public static T? GetParentDeclarationSyntax<T>(this SyntaxNode node) where T : SyntaxNode
    {
        var rootNode = node;
        while (true)
        {
            if (node is T syntax && node != rootNode)
            {
                return syntax;
            }

            if (node?.Parent == null)
            {
                return null;
            }

            node = node.Parent;
        }
    }
}

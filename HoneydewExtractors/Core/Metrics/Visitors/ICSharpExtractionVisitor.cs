using Microsoft.CodeAnalysis.CSharp;

namespace HoneydewExtractors.Core.Metrics.Visitors
{
    public interface ICSharpExtractionVisitor<in TSyntaxNode, TType> : IExtractionVisitor<TSyntaxNode, TType>
        where TSyntaxNode : CSharpSyntaxNode
    {
    }
}

using Microsoft.CodeAnalysis.CSharp;

namespace HoneydewExtractors.Core.Metrics.Visitors
{
    public interface ICSharpExtractionVisitor<in TSyntaxNode, in TSemanticNode, TType> : IExtractionVisitor<TSyntaxNode, TSemanticNode, TType>
        where TSyntaxNode : CSharpSyntaxNode
    {
    }
}

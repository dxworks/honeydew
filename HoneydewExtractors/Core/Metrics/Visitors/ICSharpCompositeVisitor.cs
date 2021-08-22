using Microsoft.CodeAnalysis.CSharp;

namespace HoneydewExtractors.Core.Metrics.Visitors
{
    public interface ICSharpCompositeVisitor<TType> : ICompositeVisitor, IModelVisitor,
        IExtractionVisitor<CSharpSyntaxNode, TType>
    {
    }
}

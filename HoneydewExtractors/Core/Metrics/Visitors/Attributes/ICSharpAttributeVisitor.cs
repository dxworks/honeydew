using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Visitors.Attributes
{
    public interface ICSharpAttributeVisitor : IAttributeVisitor,
        IExtractionVisitor<AttributeSyntax, IAttributeType>
    {
        
    }
}

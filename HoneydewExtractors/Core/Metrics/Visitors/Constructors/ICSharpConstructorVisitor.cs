using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Visitors.Constructors
{
    public interface ICSharpConstructorVisitor : IConstructorVisitor,
        IExtractionVisitor<ConstructorDeclarationSyntax, IConstructorType>
    {
    }
}

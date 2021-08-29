using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Visitors.Methods
{
    public interface ICSharpMethodAccessorVisitor : IMethodVisitor,
        IExtractionVisitor<AccessorDeclarationSyntax, IMethodType>
    {
    }
}

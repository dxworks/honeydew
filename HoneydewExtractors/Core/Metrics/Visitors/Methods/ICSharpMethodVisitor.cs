using HoneydewExtractors.Core.Metrics.Extraction.ModelCreators;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Visitors.Methods
{
    public interface ICSharpMethodVisitor : IMethodVisitor, ICSharpTypeVisitor,
        IVisitorType<MethodDeclarationSyntax, IMethodType>
    {
    }
}

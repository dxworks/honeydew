using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Visitors.Properties
{
    public interface ICSharpPropertyVisitor : IPropertyVisitor, ICSharpTypeVisitor,
        IVisitorType<BasePropertyDeclarationSyntax, IPropertyType>
    {
    }
}

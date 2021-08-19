using HoneydewModels.Types;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace HoneydewExtractors.Core.Metrics.Visitors.Properties
{
    public interface IVisualBasicPropertyVisitor : IPropertyVisitor, IVisualBasicTypeVisitor,
        IVisitorType<PropertyStatementSyntax, IPropertyType>
    {
    }
}

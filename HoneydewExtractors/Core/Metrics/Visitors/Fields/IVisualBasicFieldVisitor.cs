using HoneydewModels.Types;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace HoneydewExtractors.Core.Metrics.Visitors.Fields
{
    public interface IVisualBasicFieldVisitor : IFieldVisitor, IVisualBasicTypeVisitor,
        IVisitorType<FieldDeclarationSyntax, IFieldType>
    {
    }
}

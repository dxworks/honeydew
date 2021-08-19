using HoneydewModels.Types;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace HoneydewExtractors.Core.Metrics.Visitors.Classes
{
    public interface IVisualBasicClassVisitor : IClassVisitor, IVisualBasicTypeVisitor,
        IVisitorType<ClassStatementSyntax, IMembersClassType>
    {
    }
}

using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace HoneydewExtractors.Core.Metrics.Visitors.Modules
{
    public interface IVisualBasicModuleVisitor : IModuleVisitor, IVisualBasicTypeVisitor
    {
        void Visit(ModuleStatementSyntax syntax);
    }
}

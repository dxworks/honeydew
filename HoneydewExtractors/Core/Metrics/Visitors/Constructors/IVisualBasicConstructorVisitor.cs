using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace HoneydewExtractors.Core.Metrics.Visitors.Constructors
{
    public interface IVisualBasicConstructorVisitor : IMethodVisitor, IVisualBasicTypeVisitor,
        IVisitorType<ConstructorBlockSyntax, IConstructorType>
    {
    }
}

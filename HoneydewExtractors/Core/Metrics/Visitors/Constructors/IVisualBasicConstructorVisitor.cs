using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace HoneydewExtractors.Core.Metrics.Visitors.Constructors;

public interface IVisualBasicConstructorVisitor : IMethodVisitor,
    IExtractionVisitor<ConstructorBlockSyntax, SemanticModel, IConstructorType>
{
}

using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Visitors.Classes;

public interface IVisualBasicDelegateVisitor : IDelegateVisitor,
    IExtractionVisitor<DelegateDeclarationSyntax, SemanticModel, IDelegateType>
{
}

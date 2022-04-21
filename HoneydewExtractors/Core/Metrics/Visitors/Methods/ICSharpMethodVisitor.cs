using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Visitors.Methods;

public interface ICSharpMethodVisitor : IMethodVisitor,
    IExtractionVisitor<MethodDeclarationSyntax, SemanticModel, IMethodType>
{
}

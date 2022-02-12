using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Visitors.Method;

public interface ICSharpLocalFunctionVisitor : ILocalFunctionVisitor,
    IExtractionVisitor<LocalFunctionStatementSyntax, SemanticModel, IMethodTypeWithLocalFunctions>
{
}

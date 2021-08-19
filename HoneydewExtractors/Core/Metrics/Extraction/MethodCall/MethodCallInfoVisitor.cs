using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.MethodSignatures;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.MethodCall
{
    public class MethodCallInfoVisitor : ExtractionVisitor<CSharpSyntacticModel, CSharpSemanticModel>,
        ICSharpMethodSignatureVisitor
    {
        public IMethodSignatureType Visit(InvocationExpressionSyntax syntaxNode, IMethodSignatureType modelType)
        {
            var methodCallModel = InheritedSemanticModel.GetMethodCallModel(syntaxNode);
            return methodCallModel;
        }
    }
}

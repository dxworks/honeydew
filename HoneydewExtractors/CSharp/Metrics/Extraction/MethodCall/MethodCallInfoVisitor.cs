using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.MethodSignatures;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.MethodCall
{
    public class MethodCallInfoVisitor : IRequireCSharpExtractionHelperMethodsVisitor,
        ICSharpMethodSignatureVisitor
    {
        public CSharpExtractionHelperMethods CSharpHelperMethods { get; set; }

        public void Accept(IVisitor visitor)
        {
        }

        public IMethodSignatureType Visit(InvocationExpressionSyntax syntaxNode, IMethodSignatureType modelType)
        {
            var methodCallModel = CSharpHelperMethods.GetMethodCallModel(syntaxNode);
            return methodCallModel;
        }
    }
}

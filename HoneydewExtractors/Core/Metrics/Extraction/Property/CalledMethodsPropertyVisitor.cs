using System.Collections.Generic;
using System.Linq;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Property
{
    public class CalledMethodsPropertyVisitor : ExtractionVisitor<CSharpSyntacticModel, CSharpSemanticModel>,
        ICSharpPropertyVisitor
    {
        public IPropertyType Visit(BasePropertyDeclarationSyntax syntaxNode, IPropertyType modelType)
        {
            var containingClass = "";
            if (syntaxNode.Parent is BaseTypeDeclarationSyntax classDeclarationSyntax)
            {
                containingClass = InheritedSemanticModel.GetFullName(classDeclarationSyntax);
            }

            var calledMethods = new List<IMethodSignatureType>();
            foreach (var invocationExpressionSyntax in syntaxNode.DescendantNodes()
                .OfType<InvocationExpressionSyntax>())
            {
                var methodCallModel = InheritedSemanticModel.GetMethodCallModel(invocationExpressionSyntax,
                    containingClass);
                if (methodCallModel != null)
                {
                    calledMethods.Add(methodCallModel);
                }
            }

            modelType.CalledMethods = calledMethods;

            return modelType;
        }
    }
}

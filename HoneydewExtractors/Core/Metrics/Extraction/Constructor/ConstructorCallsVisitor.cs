using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.CSharp.Metrics.Extraction;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Constructor
{
    public class ConstructorCallsVisitor : IRequireCSharpExtractionHelperMethodsVisitor,
        ICSharpConstructorVisitor
    {
        public CSharpExtractionHelperMethods CSharpHelperMethods { get; set; }

        public void Accept(IVisitor visitor)
        {
        }

        public IConstructorType Visit(ConstructorDeclarationSyntax syntaxNode, IConstructorType modelType)
        {
            var methodCall = ExtractInfoAboutConstructorCalls(syntaxNode);

            if (methodCall != null)
            {
                modelType.CalledMethods.Add(methodCall);
            }

            return modelType;
        }

        private IMethodSignatureType ExtractInfoAboutConstructorCalls(ConstructorDeclarationSyntax syntax)
        {
            if (syntax.Initializer == null)
            {
                return null;
            }

            var containingClassName = "";
            var baseName = CSharpConstants.ObjectIdentifier;

            if (syntax.Parent is BaseTypeDeclarationSyntax baseTypeDeclarationSyntax)
            {
                containingClassName = CSharpHelperMethods.GetFullName(baseTypeDeclarationSyntax);
                baseName = CSharpHelperMethods.GetBaseClassName(baseTypeDeclarationSyntax);
            }

            var methodName = syntax.Identifier.ToString();
            if (syntax.Initializer.ThisOrBaseKeyword.ValueText == "base")
            {
                containingClassName = baseName;
                methodName = baseName;
            }

            IList<IParameterType> parameterModels = new List<IParameterType>();

            var methodSymbol = CSharpHelperMethods.GetMethodSymbol(syntax.Initializer);

            if (methodSymbol != null)
            {
                parameterModels = CSharpHelperMethods.GetParameters(methodSymbol);
                methodName = methodSymbol.ContainingType.Name;
            }

            return new MethodCallModel
            {
                Name = methodName,
                ContainingTypeName = containingClassName,
                ParameterTypes = parameterModels
            };
        }
    }
}

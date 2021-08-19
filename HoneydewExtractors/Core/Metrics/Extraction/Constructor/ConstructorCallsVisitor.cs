using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Constructor
{
    public class ConstructorCallsVisitor : ExtractionVisitor<CSharpSyntacticModel, CSharpSemanticModel>,
        ICSharpConstructorVisitor
    {
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
                containingClassName = InheritedSemanticModel.GetFullName(baseTypeDeclarationSyntax);
                baseName = InheritedSemanticModel.GetBaseClassName(baseTypeDeclarationSyntax);
            }

            var methodName = syntax.Identifier.ToString();
            if (syntax.Initializer.ThisOrBaseKeyword.ValueText == "base")
            {
                containingClassName = baseName;
                methodName = baseName;
            }

            IList<IParameterType> parameterModels = new List<IParameterType>();

            var methodSymbol = InheritedSemanticModel.GetMethodSymbol(syntax.Initializer);

            if (methodSymbol != null)
            {
                parameterModels = InheritedSemanticModel.GetParameters(methodSymbol);
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

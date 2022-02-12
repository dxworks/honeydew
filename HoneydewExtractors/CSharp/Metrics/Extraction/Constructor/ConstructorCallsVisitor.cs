using System.Collections.Generic;
using HoneydewCore.Utils;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Constructor;

public class ConstructorCallsVisitor : ICSharpConstructorVisitor
{
    public void Accept(IVisitor visitor)
    {
    }

    public IConstructorType Visit(ConstructorDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IConstructorType modelType)
    {
        var methodCall = ExtractInfoAboutConstructorCalls(syntaxNode, semanticModel);

        if (methodCall != null)
        {
            modelType.CalledMethods.Add(methodCall);
        }

        return modelType;
    }

    private IMethodSignatureType ExtractInfoAboutConstructorCalls(ConstructorDeclarationSyntax syntax,
        SemanticModel semanticModel)
    {
        if (syntax.Initializer == null)
        {
            return null;
        }

        var containingClassName = "";
        var baseName = CSharpConstants.ObjectIdentifier;

        if (syntax.Parent is BaseTypeDeclarationSyntax baseTypeDeclarationSyntax)
        {
            containingClassName =
                CSharpExtractionHelperMethods.GetFullName(baseTypeDeclarationSyntax, semanticModel).Name;
            baseName = CSharpExtractionHelperMethods.GetBaseClassName(baseTypeDeclarationSyntax, semanticModel).Name;
        }

        var methodName = syntax.Identifier.ToString();
        if (syntax.Initializer.ThisOrBaseKeyword.ValueText == "base")
        {
            containingClassName = baseName;
            methodName = baseName;
        }

        IList<IParameterType> parameterModels = new List<IParameterType>();

        var methodSymbol = CSharpExtractionHelperMethods.GetMethodSymbol(syntax.Initializer, semanticModel);

        if (methodSymbol != null)
        {
            parameterModels = CSharpExtractionHelperMethods.GetParameters(methodSymbol);
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

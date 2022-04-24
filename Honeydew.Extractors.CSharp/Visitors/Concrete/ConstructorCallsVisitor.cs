using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Honeydew.Extractors.CSharp.Visitors.Utils.CSharpExtractionHelperMethods;

namespace Honeydew.Extractors.CSharp.Visitors.Concrete;

public class ConstructorCallsVisitor : IExtractionVisitor<ConstructorDeclarationSyntax, SemanticModel, IConstructorType>
{
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

    private static IMethodCallType? ExtractInfoAboutConstructorCalls(ConstructorDeclarationSyntax syntaxNode,
        SemanticModel semanticModel)
    {
        if (syntaxNode.Initializer == null)
        {
            return null;
        }

        var baseName = CSharpConstants.ObjectIdentifier;

        if (syntaxNode.Parent is BaseTypeDeclarationSyntax baseTypeDeclarationSyntax)
        {
            baseName = GetBaseClassName(baseTypeDeclarationSyntax, semanticModel).Name;
        }

        var methodName = syntaxNode.Identifier.ToString();
        if (syntaxNode.Initializer.ThisOrBaseKeyword.ValueText == "base")
        {
            methodName = baseName;
        }

        IList<IParameterType> parameterModels = new List<IParameterType>();

        var methodSymbol = GetMethodSymbol(syntaxNode.Initializer, semanticModel);

        if (methodSymbol != null)
        {
            parameterModels = GetParameters(methodSymbol);
            methodName = methodSymbol.ContainingType.Name;
        }

        return new CSharpMethodCallModel
        {
            Name = methodName,
            DefinitionClassName = GetDefinitionClassName(syntaxNode, semanticModel),
            LocationClassName = GetLocationClassName(syntaxNode, semanticModel),
            ParameterTypes = parameterModels
        };
    }
}

using Honeydew.Extractors.Dotnet;
using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using static Honeydew.Extractors.VisualBasic.Visitors.Utils.VisualBasicExtractionHelperMethods;

namespace Honeydew.Extractors.VisualBasic.Visitors.Concrete;

public class ConstructorCallsVisitor : IExtractionVisitor<ConstructorBlockSyntax, SemanticModel, IConstructorType>
{
    public IConstructorType Visit(ConstructorBlockSyntax syntaxNode, SemanticModel semanticModel,
        IConstructorType modelType)
    {
        var parentTypeSyntax = syntaxNode.GetParentDeclarationSyntax<TypeBlockSyntax>();
        if (parentTypeSyntax is null)
        {
            return modelType;
        }

        var constructorCallSyntax = syntaxNode.Statements
            .OfType<ExpressionStatementSyntax>()
            .Select(e =>
            {
                if (e.Expression is InvocationExpressionSyntax
                    {
                        Expression: MemberAccessExpressionSyntax memberAccessExpressionSyntax
                    })
                {
                    var expressionName = memberAccessExpressionSyntax.Expression.ToString();
                    if (expressionName == "Me" || expressionName == "MyBase" &&
                        memberAccessExpressionSyntax.Name.Identifier.ToString() == "New")
                    {
                        return memberAccessExpressionSyntax;
                    }
                }

                return null;
            })
            .FirstOrDefault();

        if (constructorCallSyntax is null)
        {
            return modelType;
        }

        var constructorCall = new VisualBasicMethodCallModel
        {
            Name = "New",
            DefinitionClassName = GetDefinitionClassName(constructorCallSyntax, semanticModel),
            LocationClassName = GetLocationClassName(constructorCallSyntax, semanticModel),
            ParameterTypes = GetParameters(constructorCallSyntax, semanticModel)
        };

        modelType.CalledMethods.Add(constructorCall);

        return modelType;
    }
}

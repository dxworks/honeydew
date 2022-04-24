using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Honeydew.Extractors.CSharp.Visitors.Utils.CSharpExtractionHelperMethods;

namespace Honeydew.Extractors.CSharp.Visitors.Concrete;

public class ReturnValueInfoVisitor :
    IExtractionVisitor<TypeSyntax, SemanticModel, IReturnValueType>,
    IExtractionVisitor<AccessorReturnValue, SemanticModel, IReturnValueType>
{
    public IReturnValueType Visit(TypeSyntax syntaxNode, SemanticModel semanticModel, IReturnValueType modelType)
    {
        var returnType = GetFullName(syntaxNode, semanticModel, out var isNullable);
        var returnTypeModifier = SetTypeModifier(syntaxNode.ToString(), "");

        modelType.Type = returnType;
        modelType.Modifier = returnTypeModifier;
        modelType.IsNullable = isNullable;

        return modelType;
    }

    public IReturnValueType Visit(AccessorReturnValue syntaxNode, SemanticModel semanticModel,
        IReturnValueType modelType)
    {
        if (syntaxNode.Type != "get")
        {
            modelType.Type = new EntityTypeModel
            {
                Name = "void"
            };
            modelType.IsNullable = false;
            modelType.Modifier = "";

            return modelType;
        }

        var returnType = GetFullName(syntaxNode.ReturnType, semanticModel, out var isNullable);
        var returnTypeModifier = SetTypeModifier(syntaxNode.ToString(), "");

        modelType.Type = returnType;
        modelType.Modifier = returnTypeModifier;
        modelType.IsNullable = isNullable;

        return modelType;
    }
}

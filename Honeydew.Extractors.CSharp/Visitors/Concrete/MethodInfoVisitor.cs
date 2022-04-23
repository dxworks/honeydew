using Honeydew.Extractors.CSharp.Visitors.Utils;
using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Honeydew.Extractors.CSharp.Utils.CSharpConstants;
using static Honeydew.Extractors.CSharp.Visitors.Utils.CSharpExtractionHelperMethods;

namespace Honeydew.Extractors.CSharp.Visitors.Concrete;

public class MethodInfoVisitor :
    IExtractionVisitor<MethodDeclarationSyntax, SemanticModel, IMethodType>,
    IExtractionVisitor<AccessorDeclarationSyntax, SemanticModel, IAccessorMethodType>,
    IExtractionVisitor<ArrowExpressionClauseSyntax, SemanticModel, IAccessorMethodType>
{
    public IMethodType Visit(MethodDeclarationSyntax syntaxNode, SemanticModel semanticModel, IMethodType modelType)
    {
        var isInterface = syntaxNode.GetParentDeclarationSyntax<InterfaceDeclarationSyntax>() != null;
        var accessModifier = isInterface
            ? DefaultInterfaceMethodAccessModifier
            : DefaultClassMethodAccessModifier;
        var modifier = isInterface
            ? DefaultInterfaceMethodModifier
            : syntaxNode.Modifiers.ToString();

        SetModifiers(syntaxNode.Modifiers.ToString(), ref accessModifier, ref modifier);

        var returnType = GetFullName(syntaxNode.ReturnType, semanticModel, out var isNullable);

        var returnTypeModifier = SetTypeModifier(syntaxNode.ReturnType.ToString(), "");


        modelType.Name = syntaxNode.Identifier.ToString();
        modelType.ReturnValue = new ReturnValueModel
        {
            Type = returnType,
            Modifier = returnTypeModifier,
            IsNullable = isNullable
        };
        modelType.Modifier = modifier;
        modelType.AccessModifier = accessModifier;
        modelType.CyclomaticComplexity = CalculateCyclomaticComplexity(syntaxNode);

        return modelType;
    }

    public IAccessorMethodType Visit(AccessorDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IAccessorMethodType modelMethodType)
    {
        var accessModifier = "public";
        var modifier = syntaxNode.Modifiers.ToString();

        SetModifiers(syntaxNode.Modifiers.ToString(), ref accessModifier, ref modifier);
        var keyword = syntaxNode.Keyword.ToString();

        IEntityType returnType = new EntityTypeModel
        {
            Name = "void"
        };
        var isNullable = false;

        if (keyword == "get")
        {
            var basePropertyDeclarationSyntax =
                syntaxNode.GetParentDeclarationSyntax<BasePropertyDeclarationSyntax>();
            if (basePropertyDeclarationSyntax != null)
            {
                returnType =
                    GetFullName(basePropertyDeclarationSyntax.Type, semanticModel,
                        out isNullable);
            }
        }

        modelMethodType.Name = keyword;
        modelMethodType.ReturnValue = new ReturnValueModel
        {
            Type = returnType,
            IsNullable = isNullable
        };
        modelMethodType.Modifier = modifier;
        modelMethodType.AccessModifier = accessModifier;
        modelMethodType.CyclomaticComplexity = CalculateCyclomaticComplexity(syntaxNode);

        return modelMethodType;
    }

    public IAccessorMethodType Visit(ArrowExpressionClauseSyntax syntaxNode, SemanticModel semanticModel,
        IAccessorMethodType modelMethodType)
    {
        IEntityType returnType = new EntityTypeModel
        {
            Name = "void"
        };
        var isNullable = false;
        var basePropertyDeclarationSyntax = syntaxNode.GetParentDeclarationSyntax<BasePropertyDeclarationSyntax>();
        if (basePropertyDeclarationSyntax != null)
        {
            returnType =
                GetFullName(basePropertyDeclarationSyntax.Type, semanticModel,
                    out isNullable);
        }

        modelMethodType.Name = "get";
        modelMethodType.AccessModifier = "public";
        modelMethodType.ReturnValue = new ReturnValueModel
        {
            Type = returnType,
            IsNullable = isNullable
        };
        modelMethodType.CyclomaticComplexity = CalculateCyclomaticComplexity(syntaxNode);

        return modelMethodType;
    }
}

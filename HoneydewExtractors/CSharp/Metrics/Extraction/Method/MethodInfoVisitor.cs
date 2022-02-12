using HoneydewCore.Utils;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Method;

public class MethodInfoVisitor : ICSharpMethodVisitor, ICSharpMethodAccessorVisitor, ICSharpArrowExpressionMethodVisitor
{
    public void Accept(IVisitor visitor)
    {
    }

    public IMethodType Visit(MethodDeclarationSyntax syntaxNode, SemanticModel semanticModel, IMethodType modelType)
    {
        var isInterface = syntaxNode.GetParentDeclarationSyntax<InterfaceDeclarationSyntax>() != null;
        var accessModifier = isInterface
            ? CSharpConstants.DefaultInterfaceMethodAccessModifier
            : CSharpConstants.DefaultClassMethodAccessModifier;
        var modifier = isInterface
            ? CSharpConstants.DefaultInterfaceMethodModifier
            : syntaxNode.Modifiers.ToString();

        CSharpConstants.SetModifiers(syntaxNode.Modifiers.ToString(), ref accessModifier, ref modifier);

        var returnType =
            CSharpExtractionHelperMethods.GetFullName(syntaxNode.ReturnType, semanticModel, out var isNullable);

        var returnTypeModifier =
            CSharpExtractionHelperMethods.SetTypeModifier(syntaxNode.ReturnType.ToString(), "");


        modelType.Name = syntaxNode.Identifier.ToString();
        modelType.ReturnValue = new ReturnValueModel
        {
            Type = returnType,
            Modifier = returnTypeModifier,
            IsNullable = isNullable
        };
        modelType.ContainingTypeName = CSharpExtractionHelperMethods.GetParentDeclaredType(syntaxNode, semanticModel);
        modelType.Modifier = modifier;
        modelType.AccessModifier = accessModifier;
        modelType.CyclomaticComplexity = CSharpExtractionHelperMethods.CalculateCyclomaticComplexity(syntaxNode);

        return modelType;
    }

    public IMethodType Visit(AccessorDeclarationSyntax syntaxNode, SemanticModel semanticModel, IMethodType modelType)
    {
        var accessModifier = "public";
        var modifier = syntaxNode.Modifiers.ToString();

        CSharpConstants.SetModifiers(syntaxNode.Modifiers.ToString(), ref accessModifier, ref modifier);
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
                    CSharpExtractionHelperMethods.GetFullName(basePropertyDeclarationSyntax.Type, semanticModel,
                        out isNullable);
            }
        }

        modelType.Name = keyword;
        modelType.ReturnValue = new ReturnValueModel
        {
            Type = returnType,
            IsNullable = isNullable
        };
        modelType.ContainingTypeName = CSharpExtractionHelperMethods.GetParentDeclaredType(syntaxNode, semanticModel);
        modelType.Modifier = modifier;
        modelType.AccessModifier = accessModifier;
        modelType.CyclomaticComplexity = CSharpExtractionHelperMethods.CalculateCyclomaticComplexity(syntaxNode);

        return modelType;
    }

    public IMethodType Visit(ArrowExpressionClauseSyntax syntaxNode, SemanticModel semanticModel, IMethodType modelType)
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
                CSharpExtractionHelperMethods.GetFullName(basePropertyDeclarationSyntax.Type, semanticModel,
                    out isNullable);
        }

        modelType.Name = "get";
        modelType.AccessModifier = "public";
        modelType.ContainingTypeName = CSharpExtractionHelperMethods.GetParentDeclaredType(syntaxNode, semanticModel);
        modelType.ReturnValue = new ReturnValueModel
        {
            Type = returnType,
            IsNullable = isNullable
        };
        modelType.CyclomaticComplexity = CSharpExtractionHelperMethods.CalculateCyclomaticComplexity(syntaxNode);

        return modelType;
    }
}

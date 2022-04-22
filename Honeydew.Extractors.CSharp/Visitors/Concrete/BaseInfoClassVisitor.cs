using Honeydew.Models.Types;
using HoneydewCore.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Honeydew.Extractors.CSharp.Visitors.Utils.CSharpExtractionHelperMethods;

namespace Honeydew.Extractors.CSharp.Visitors.Concrete;

public class BaseInfoClassVisitor : ICSharpClassVisitor
{
    public IMembersClassType Visit(TypeDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        var accessModifier = CSharpConstants.DefaultClassAccessModifier;
        var modifier = "";
        CSharpConstants.SetModifiers(syntaxNode.Modifiers.ToString(), ref accessModifier,
            ref modifier);

        modelType.Name = GetFullName(syntaxNode, semanticModel).Name;
        modelType.AccessModifier = accessModifier;
        modelType.Modifier = modifier;
        modelType.ClassType = syntaxNode.Kind().ToString().Replace("Declaration", "").ToLower();
        modelType.ContainingNamespaceName = GetContainingNamespaceName(syntaxNode, semanticModel);
        modelType.ContainingClassName = GetContainingClassName(syntaxNode, semanticModel);

        return modelType;
    }
}

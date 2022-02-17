using HoneydewCore.Utils;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static HoneydewExtractors.CSharp.Metrics.Extraction.CSharpExtractionHelperMethods;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class;

public class BaseInfoClassVisitor : ICSharpClassVisitor
{
    public void Accept(IVisitor visitor)
    {
    }

    public IClassType Visit(BaseTypeDeclarationSyntax syntaxNode, SemanticModel semanticModel, IClassType modelType)
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

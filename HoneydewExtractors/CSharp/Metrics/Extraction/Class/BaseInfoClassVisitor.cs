using HoneydewCore.Utils;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

        modelType.Name = CSharpExtractionHelperMethods.GetFullName(syntaxNode, semanticModel).Name;
        modelType.AccessModifier = accessModifier;
        modelType.Modifier = modifier;
        modelType.ClassType = syntaxNode.Kind().ToString().Replace("Declaration", "").ToLower();
        modelType.ContainingTypeName = CSharpExtractionHelperMethods.GetParentDeclaredType(syntaxNode, semanticModel);

        return modelType;
    }
}

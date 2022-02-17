using HoneydewCore.Utils;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static HoneydewExtractors.CSharp.Metrics.Extraction.CSharpExtractionHelperMethods;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Delegate;

public class BaseInfoDelegateVisitor : ICSharpDelegateVisitor
{
    public void Accept(IVisitor visitor)
    {
    }

    public IDelegateType Visit(DelegateDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IDelegateType modelType)
    {
        var accessModifier = CSharpConstants.DefaultClassAccessModifier;
        var modifier = "";
        CSharpConstants.SetModifiers(syntaxNode.Modifiers.ToString(), ref accessModifier,
            ref modifier);

        var returnType = GetFullName(syntaxNode.ReturnType, semanticModel);

        var returnTypeModifier = SetTypeModifier(syntaxNode.ReturnType.ToString(), "");

        var name = GetFullName(syntaxNode, semanticModel).Name;
        modelType.Name = name;
        modelType.AccessModifier = accessModifier;
        modelType.Modifier = modifier;
        modelType.ReturnValue = new ReturnValueModel
        {
            Type = returnType,
            Modifier = returnTypeModifier
        };

        modelType.ClassType = CSharpConstants.DelegateIdentifier;
        modelType.BaseTypes.Add(new BaseTypeModel
        {
            Kind = CSharpConstants.ClassIdentifier,
            Type = new EntityTypeModel
            {
                Name = CSharpConstants.SystemDelegate
            }
        });
        modelType.ContainingClassName = GetContainingClassName(syntaxNode, semanticModel);
        modelType.ContainingNamespaceName = GetContainingNamespaceName(syntaxNode, semanticModel);

        return modelType;
    }
}

using HoneydewCore.Utils;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

        var returnType = CSharpExtractionHelperMethods.GetFullName(syntaxNode.ReturnType, semanticModel);

        var returnTypeModifier = CSharpExtractionHelperMethods.SetTypeModifier(syntaxNode.ReturnType.ToString(), "");

        var name = CSharpExtractionHelperMethods.GetFullName(syntaxNode, semanticModel).Name;
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
        modelType.ContainingTypeName = name
            .Replace(syntaxNode.Identifier.ToString(), "").Trim('.');

        return modelType;
    }
}

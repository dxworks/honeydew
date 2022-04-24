using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Honeydew.Extractors.CSharp.Visitors.Utils.CSharpExtractionHelperMethods;

namespace Honeydew.Extractors.CSharp.Visitors.Concrete;

public class BaseInfoDelegateVisitor : IExtractionVisitor<DelegateDeclarationSyntax, SemanticModel, IDelegateType>
{
    public IDelegateType Visit(DelegateDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IDelegateType modelType)
    {
        var accessModifier = CSharpConstants.DefaultClassAccessModifier;
        var modifier = "";
        CSharpConstants.SetModifiers(syntaxNode.Modifiers.ToString(), ref accessModifier,
            ref modifier);

        var name = GetFullName(syntaxNode, semanticModel).Name;
        modelType.Name = name;
        modelType.AccessModifier = accessModifier;
        modelType.Modifier = modifier;

        modelType.ClassType = CSharpConstants.DelegateIdentifier;
        modelType.BaseTypes.Add(new CSharpBaseTypeModel
        {
            Kind = CSharpConstants.ClassIdentifier,
            Type = new CSharpEntityTypeModel
            {
                Name = CSharpConstants.SystemDelegate
            }
        });
        modelType.ContainingClassName = GetContainingClassName(syntaxNode, semanticModel);
        modelType.ContainingNamespaceName = GetContainingNamespaceName(syntaxNode, semanticModel);

        return modelType;
    }
}

using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Honeydew.Extractors.CSharp.Visitors.Utils.CSharpExtractionHelperMethods;

namespace Honeydew.Extractors.CSharp.Visitors.Concrete;

public class BaseInfoEnumVisitor : IExtractionVisitor<EnumDeclarationSyntax, SemanticModel, IEnumType>
{
    public IEnumType Visit(EnumDeclarationSyntax syntaxNode, SemanticModel semanticModel, IEnumType modelType)
    {
        var accessModifier = CSharpConstants.DefaultClassAccessModifier;
        var modifier = "";
        CSharpConstants.SetModifiers(syntaxNode.Modifiers.ToString(), ref accessModifier, ref modifier);

        modelType.Name = GetFullName(syntaxNode, semanticModel).Name;
        modelType.AccessModifier = accessModifier;
        modelType.Modifier = modifier;
        modelType.ClassType = "enum";
        modelType.ContainingNamespaceName = GetContainingNamespaceName(syntaxNode, semanticModel);
        modelType.ContainingClassName = GetContainingClassName(syntaxNode, semanticModel);
        modelType.Type = "int";
        var baseTypeSyntax = syntaxNode.BaseList?.Types.FirstOrDefault();
        if (baseTypeSyntax != null)
        {
            modelType.Type = baseTypeSyntax.ToString();
        }

        modelType.BaseTypes.Add(new BaseTypeModel
        {
            Type = new EntityTypeModel
            {
                Name = "System.Enum",
                FullType = new GenericType
                {
                    Name = "System.Enum"
                }
            },
            Kind = "class"
        });

        return modelType;
    }
}

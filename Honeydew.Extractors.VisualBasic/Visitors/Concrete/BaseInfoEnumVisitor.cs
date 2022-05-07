using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using static Honeydew.Extractors.VisualBasic.Visitors.Utils.VisualBasicExtractionHelperMethods;

namespace Honeydew.Extractors.VisualBasic.Visitors.Concrete;

public class BaseInfoEnumVisitor : IExtractionVisitor<EnumBlockSyntax, SemanticModel, IEnumType>
{
    public IEnumType Visit(EnumBlockSyntax syntaxNode, SemanticModel semanticModel, IEnumType modelType)
    {
        var enumStatement = syntaxNode.EnumStatement;
        
        var accessModifier = VisualBasicConstants.DefaultClassAccessModifier;
        var modifier = "";
        VisualBasicConstants.SetModifiers(enumStatement.Modifiers.ToString(), ref accessModifier, ref modifier);

        modelType.Name = GetFullName(enumStatement, semanticModel).Name;
        modelType.AccessModifier = accessModifier;
        modelType.Modifier = modifier;
        modelType.ClassType = "enum";
        modelType.ContainingNamespaceName = GetContainingNamespaceName(enumStatement, semanticModel);
        modelType.ContainingClassName = GetContainingClassName(enumStatement, semanticModel);
        modelType.Type = "Int";

        if (enumStatement.UnderlyingType is SimpleAsClauseSyntax simpleAsClauseSyntax)
        {
            modelType.Type = simpleAsClauseSyntax.Type.ToString();
        }

        modelType.BaseTypes.Add(new VisualBasicBaseTypeModel
        {
            Type = new VisualBasicEntityTypeModel
            {
                Name = "System.Enum",
                FullType = new GenericType
                {
                    Name = "System.Enum"
                }
            },
            Kind = "class"
        });

        if (modelType is VisualBasicEnumModel visualBasicEnumModel)
        {
            visualBasicEnumModel.ContainingModuleName = GetContainingModuleName(enumStatement);
        }

        return modelType;
    }
}

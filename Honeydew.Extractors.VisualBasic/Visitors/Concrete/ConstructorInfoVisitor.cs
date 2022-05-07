using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using static Honeydew.Extractors.VisualBasic.Visitors.Utils.VisualBasicExtractionHelperMethods;

namespace Honeydew.Extractors.VisualBasic.Visitors.Concrete;

public class ConstructorInfoVisitor : IExtractionVisitor<ConstructorBlockSyntax, SemanticModel, IConstructorType>
{
    public IConstructorType Visit(ConstructorBlockSyntax syntaxNode, SemanticModel semanticModel,
        IConstructorType modelType)
    {
        var allModifiers = syntaxNode.BlockStatement.Modifiers.ToString();

        var accessModifier = VisualBasicConstants.DefaultClassMethodAccessModifier;
        var modifier = allModifiers;

        VisualBasicConstants.SetModifiers(allModifiers, ref accessModifier, ref modifier);

        if (modifier == "static")
        {
            accessModifier = "";
        }

        modelType.Name = "New";
        modelType.Modifier = modifier;
        modelType.AccessModifier = accessModifier;
        modelType.CyclomaticComplexity = CalculateCyclomaticComplexity(syntaxNode);

        return modelType;
    }
}

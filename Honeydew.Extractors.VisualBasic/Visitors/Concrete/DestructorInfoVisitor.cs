using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using static Honeydew.Extractors.VisualBasic.Visitors.Utils.VisualBasicExtractionHelperMethods;

namespace Honeydew.Extractors.VisualBasic.Visitors.Concrete;

public class DestructorInfoVisitor : IExtractionVisitor<MethodBlockSyntax, SemanticModel, IDestructorType>
{
    public IDestructorType Visit(MethodBlockSyntax syntaxNode, SemanticModel semanticModel,
        IDestructorType modelType)
    {
        modelType.Name = syntaxNode.SubOrFunctionStatement.Identifier.ToString();
        
        var accessModifier = VisualBasicConstants.DefaultClassAccessModifier;
        var modifier = "";
        VisualBasicConstants.SetModifiers(syntaxNode.SubOrFunctionStatement.Modifiers.ToString(), ref accessModifier,
            ref modifier);

        modelType.AccessModifier = accessModifier;
        modelType.Modifier = modifier;

        modelType.CyclomaticComplexity = CalculateCyclomaticComplexity(syntaxNode);

        return modelType;
    }
}

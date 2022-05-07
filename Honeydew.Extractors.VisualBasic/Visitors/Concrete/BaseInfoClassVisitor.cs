using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using static Honeydew.Extractors.VisualBasic.Visitors.Utils.VisualBasicExtractionHelperMethods;

namespace Honeydew.Extractors.VisualBasic.Visitors.Concrete;

public class BaseInfoClassVisitor :
    IExtractionVisitor<ClassBlockSyntax, SemanticModel, IMembersClassType>,
    IExtractionVisitor<InterfaceBlockSyntax, SemanticModel, IMembersClassType>,
    IExtractionVisitor<StructureBlockSyntax, SemanticModel, IMembersClassType>
{
    public IMembersClassType Visit(ClassBlockSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        return UpdateModelType(syntaxNode, semanticModel, modelType);
    }

    public IMembersClassType Visit(InterfaceBlockSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        return UpdateModelType(syntaxNode, semanticModel, modelType);
    }

    public IMembersClassType Visit(StructureBlockSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        return UpdateModelType(syntaxNode, semanticModel, modelType);
    }

    private static IMembersClassType UpdateModelType(TypeBlockSyntax blockSyntax, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        var accessModifier = VisualBasicConstants.DefaultClassAccessModifier;
        var modifier = "";
        VisualBasicConstants.SetModifiers(blockSyntax.BlockStatement.Modifiers.ToString(), ref accessModifier,
            ref modifier);

        modelType.Name = GetFullName(blockSyntax, semanticModel).Name;
        modelType.AccessModifier = accessModifier;
        modelType.Modifier = modifier;
        modelType.ClassType = blockSyntax.Kind()
            .ToString()
            .Replace("Statement", "")
            .Replace("Block", "")
            .ToLower();
        modelType.ContainingNamespaceName = GetContainingNamespaceName(blockSyntax, semanticModel);
        modelType.ContainingClassName = GetContainingClassName(blockSyntax, semanticModel);

        if (modelType is VisualBasicClassModel visualBasicClassModel)
        {
            visualBasicClassModel.ContainingModuleName = GetContainingModuleName(blockSyntax);
        }

        return modelType;
    }
}

using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using static Honeydew.Extractors.VisualBasic.Visitors.Utils.VisualBasicExtractionHelperMethods;

namespace Honeydew.Extractors.VisualBasic.Visitors.Concrete;

public class BaseInfoClassVisitor :
    IExtractionVisitor<ClassStatementSyntax, SemanticModel, IMembersClassType>,
    IExtractionVisitor<InterfaceStatementSyntax, SemanticModel, IMembersClassType>,
    IExtractionVisitor<StructureStatementSyntax, SemanticModel, IMembersClassType>
{
    public IMembersClassType Visit(ClassStatementSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        return UpdateModelType(syntaxNode, semanticModel, modelType);
    }

    public IMembersClassType Visit(InterfaceStatementSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        return UpdateModelType(syntaxNode, semanticModel, modelType);
    }

    public IMembersClassType Visit(StructureStatementSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        return UpdateModelType(syntaxNode, semanticModel, modelType);
    }

    private static IMembersClassType UpdateModelType(TypeStatementSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        var accessModifier = VisualBasicConstants.DefaultClassAccessModifier;
        var modifier = "";
        VisualBasicConstants.SetModifiers(syntaxNode.Modifiers.ToString(), ref accessModifier,
            ref modifier);

        modelType.Name = GetFullName(syntaxNode, semanticModel).Name;
        modelType.AccessModifier = accessModifier;
        modelType.Modifier = modifier;
        modelType.ClassType = syntaxNode.Kind().ToString().Replace("Statement", "").ToLower();
        modelType.ContainingNamespaceName = GetContainingNamespaceName(syntaxNode, semanticModel);
        modelType.ContainingClassName = GetContainingClassName(syntaxNode, semanticModel);

        if (modelType is VisualBasicClassModel visualBasicClassModel)
        {
            visualBasicClassModel.ContainingModuleName = GetContainingModuleName(syntaxNode);
        }

        return modelType;
    }
}

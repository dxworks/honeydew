using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using static Honeydew.Extractors.VisualBasic.Visitors.Utils.VisualBasicExtractionHelperMethods;

namespace Honeydew.Extractors.VisualBasic.Visitors.Concrete;

public class BaseInfoDelegateVisitor : IExtractionVisitor<DelegateStatementSyntax, SemanticModel, IDelegateType>
{
    public IDelegateType Visit(DelegateStatementSyntax syntaxNode, SemanticModel semanticModel,
        IDelegateType modelType)
    {
        var accessModifier = VisualBasicConstants.DefaultClassAccessModifier;
        var modifier = "";
        VisualBasicConstants.SetModifiers(syntaxNode.Modifiers.ToString(), ref accessModifier,
            ref modifier);

        var name = GetFullName(syntaxNode, semanticModel).Name;
        modelType.Name = name;
        modelType.AccessModifier = accessModifier;
        modelType.Modifier = modifier;

        modelType.ClassType = "delegate";
        modelType.BaseTypes.Add(new VisualBasicBaseTypeModel
        {
            Kind = "class",
            Type = new VisualBasicEntityTypeModel
            {
                Name = "System.Delegate",
                FullType = new GenericType
                {
                    Name = "System.Delegate"
                }
            }
        });
        modelType.ContainingClassName = GetContainingClassName(syntaxNode, semanticModel);
        modelType.ContainingNamespaceName = GetContainingNamespaceName(syntaxNode, semanticModel);

        if (modelType is VisualBasicDelegateModel delegateModel)
        {
            delegateModel.ContainingModuleName = GetContainingModuleName(syntaxNode);
        }

        return modelType;
    }
}

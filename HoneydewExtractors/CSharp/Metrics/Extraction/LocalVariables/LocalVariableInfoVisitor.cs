using System.Linq;
using HoneydewCore.Utils;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.LocalVariables;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.LocalVariables;

public class LocalVariableInfoVisitor : ICSharpLocalVariablesVisitor
{
    public void Accept(IVisitor visitor)
    {
    }

    public ILocalVariableType Visit(VariableDeclaratorSyntax syntaxNode, SemanticModel semanticModel,
        ILocalVariableType modelType)
    {
        var variableDeclarationSyntax = syntaxNode.GetParentDeclarationSyntax<VariableDeclarationSyntax>();
        if (variableDeclarationSyntax == null)
        {
            return modelType;
        }

        IEntityType localVariableType =
            CSharpExtractionHelperMethods.GetFullName(variableDeclarationSyntax.Type, semanticModel,
                out var isNullable);
        var fullName = localVariableType.Name;

        var modifier = CSharpExtractionHelperMethods.SetTypeModifier(variableDeclarationSyntax.Type.ToString(), "");
        modelType.Modifier = modifier;

        if (!string.IsNullOrEmpty(modifier) && fullName.Contains(modifier))
        {
            fullName = fullName.Replace(modifier, "").Trim();
        }

        if (fullName == CSharpConstants.VarIdentifier)
        {
            fullName = CSharpExtractionHelperMethods
                .GetFullName(variableDeclarationSyntax, semanticModel, out isNullable).Name;
            if (fullName != CSharpConstants.VarIdentifier)
            {
                localVariableType.Name = fullName;
            }
            else
            {
                foreach (var declarationVariable in variableDeclarationSyntax.Variables)
                {
                    if (declarationVariable.Initializer is
                        {
                            Value: ObjectCreationExpressionSyntax
                            objectCreationExpressionSyntax
                        })
                    {
                        localVariableType =
                            CSharpExtractionHelperMethods.GetFullName(objectCreationExpressionSyntax.Type,
                                semanticModel, out isNullable);
                    }
                    else if (declarationVariable.Initializer != null)
                    {
                        var memberAccessExpressionSyntax = declarationVariable.Initializer.ChildNodes()
                            .OfType<MemberAccessExpressionSyntax>().SingleOrDefault();
                        if (memberAccessExpressionSyntax != null)
                        {
                            localVariableType =
                                CSharpExtractionHelperMethods.GetFullName(memberAccessExpressionSyntax.Name,
                                    semanticModel, out isNullable);
                            if (localVariableType.Name == memberAccessExpressionSyntax.Name.ToString())
                            {
                                localVariableType.Name = "";
                                localVariableType.FullType.Name = "";
                            }
                        }
                        else
                        {
                            localVariableType =
                                CSharpExtractionHelperMethods.GetFullName(declarationVariable.Initializer.Value,
                                    semanticModel, out isNullable);
                        }
                    }
                }
            }
        }

        modelType.Type = localVariableType;
        modelType.IsNullable = isNullable;

        return modelType;
    }

    public ILocalVariableType Visit(DeclarationPatternSyntax syntaxNode, SemanticModel semanticModel,
        ILocalVariableType modelType)
    {
        modelType.Type =
            CSharpExtractionHelperMethods.GetFullName(syntaxNode.Type, semanticModel, out var isNullable);
        modelType.IsNullable = isNullable;
        return modelType;
    }

    public ILocalVariableType Visit(ForEachStatementSyntax syntaxNode, SemanticModel semanticModel,
        ILocalVariableType modelType)
    {
        modelType.Type =
            CSharpExtractionHelperMethods.GetFullName(syntaxNode.Type, semanticModel, out var isNullable);

        if (modelType.Type.Name == CSharpConstants.VarIdentifier)
        {
            var entityType =
                CSharpExtractionHelperMethods.GetFullName(syntaxNode.Expression, semanticModel, out isNullable);
            if (entityType.FullType.ContainedTypes.Count > 0)
            {
                var genericType = entityType.FullType.ContainedTypes[0];
                modelType.Type.Name = genericType.Name;
                modelType.Type.FullType = genericType;
            }
        }

        modelType.IsNullable = isNullable;

        return modelType;
    }
}

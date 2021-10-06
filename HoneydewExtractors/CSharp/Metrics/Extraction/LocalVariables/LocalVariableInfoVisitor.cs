using System.Linq;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.LocalVariables;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.LocalVariables
{
    public class LocalVariableInfoVisitor : IRequireCSharpExtractionHelperMethodsVisitor,
        ICSharpLocalVariablesVisitor
    {
        public CSharpExtractionHelperMethods CSharpHelperMethods { get; set; }

        public void Accept(IVisitor visitor)
        {
        }

        public ILocalVariableType Visit(VariableDeclaratorSyntax syntaxNode, ILocalVariableType modelType)
        {
            var variableDeclarationSyntax = syntaxNode.GetParentDeclarationSyntax<VariableDeclarationSyntax>();
            if (variableDeclarationSyntax == null)
            {
                return modelType;
            }

            IEntityType localVariableType =
                CSharpHelperMethods.GetFullName(variableDeclarationSyntax.Type, out var isNullable);
            var fullName = localVariableType.Name;

            var modifier = CSharpHelperMethods.SetTypeModifier(variableDeclarationSyntax.Type.ToString(), "");
            modelType.Modifier = modifier;

            if (!string.IsNullOrEmpty(modifier) && fullName.Contains(modifier))
            {
                fullName = fullName.Replace(modifier, "").Trim();
            }

            if (fullName == CSharpConstants.VarIdentifier)
            {
                fullName = CSharpHelperMethods.GetFullName(variableDeclarationSyntax, out isNullable).Name;
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
                                CSharpHelperMethods.GetFullName(objectCreationExpressionSyntax.Type, out isNullable);
                        }
                        else if (declarationVariable.Initializer != null)
                        {
                            var memberAccessExpressionSyntax = declarationVariable.Initializer.ChildNodes()
                                .OfType<MemberAccessExpressionSyntax>().SingleOrDefault();
                            if (memberAccessExpressionSyntax != null)
                            {
                                localVariableType = CSharpHelperMethods.GetFullName(memberAccessExpressionSyntax.Name,
                                    out isNullable);
                                if (localVariableType.Name == memberAccessExpressionSyntax.Name.ToString())
                                {
                                    localVariableType.Name = "";
                                    localVariableType.FullType.Name = "";
                                }
                            }
                            else
                            {
                                localVariableType =
                                    CSharpHelperMethods.GetFullName(declarationVariable.Initializer.Value,
                                        out isNullable);
                            }
                        }
                    }
                }
            }

            modelType.Type = localVariableType;
            modelType.IsNullable = isNullable;

            return modelType;
        }

        public ILocalVariableType Visit(DeclarationPatternSyntax syntaxNode, ILocalVariableType modelType)
        {
            modelType.Type = CSharpHelperMethods.GetFullName(syntaxNode.Type, out var isNullable);
            modelType.IsNullable = isNullable;
            return modelType;
        }

        public ILocalVariableType Visit(ForEachStatementSyntax syntaxNode, ILocalVariableType modelType)
        {
            modelType.Type = CSharpHelperMethods.GetFullName(syntaxNode.Type, out var isNullable);

            if (modelType.Type.Name == CSharpConstants.VarIdentifier)
            {
                var entityType = CSharpHelperMethods.GetFullName(syntaxNode.Expression, out isNullable);
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
}

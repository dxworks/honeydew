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

            IEntityType localVariableType = CSharpHelperMethods.GetFullName(variableDeclarationSyntax.Type);
            var fullName = localVariableType.Name;

            if (fullName == CSharpConstants.VarIdentifier)
            {
                fullName = CSharpHelperMethods.GetFullName(variableDeclarationSyntax).Name;
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
                            localVariableType = CSharpHelperMethods.GetFullName(objectCreationExpressionSyntax.Type);
                        }
                        else if (declarationVariable.Initializer != null)
                        {
                            localVariableType = CSharpHelperMethods.GetFullName(declarationVariable.Initializer.Value);
                        }
                    }
                }
            }

            modelType.Type = localVariableType;

            return modelType;
        }

        public ILocalVariableType Visit(DeclarationPatternSyntax syntaxNode, ILocalVariableType modelType)
        {
            modelType.Type = CSharpHelperMethods.GetFullName(syntaxNode.Type);
            return modelType;
        }

        public ILocalVariableType Visit(ForEachStatementSyntax syntaxNode, ILocalVariableType modelType)
        {
            modelType.Type = CSharpHelperMethods.GetFullName(syntaxNode.Type);

            if (modelType.Type.Name == CSharpConstants.VarIdentifier)
            {
                var entityType = CSharpHelperMethods.GetFullName(syntaxNode.Expression);
                if (entityType.FullType.ContainedTypes.Count > 0)
                {
                    var genericType = entityType.FullType.ContainedTypes[0];
                    modelType.Type.Name = genericType.Name;
                    modelType.Type.FullType = genericType;
                }
            }

            return modelType;
        }
    }
}

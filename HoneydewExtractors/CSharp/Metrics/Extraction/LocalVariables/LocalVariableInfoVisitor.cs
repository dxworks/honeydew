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

        public IEntityType Visit(VariableDeclaratorSyntax syntaxNode, IEntityType modelType)
        {
            var variableDeclarationSyntax =
                CSharpHelperMethods.GetParentDeclarationSyntax<VariableDeclarationSyntax>(syntaxNode);
            if (variableDeclarationSyntax == null)
            {
                return modelType;
            }

            var fullName = CSharpHelperMethods.GetFullName(variableDeclarationSyntax.Type).Name;

            if (fullName != CSharpConstants.VarIdentifier)
            {
                modelType.Name = fullName;
            }
            else
            {
                fullName = CSharpHelperMethods.GetFullName(variableDeclarationSyntax).Name;
                if (fullName != CSharpConstants.VarIdentifier)
                {
                    modelType.Name = fullName;
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
                            modelType.Name = CSharpHelperMethods.GetFullName(objectCreationExpressionSyntax.Type).Name;
                        }
                        else if (declarationVariable.Initializer != null)
                        {
                            modelType.Name = CSharpHelperMethods.GetFullName(declarationVariable.Initializer.Value)
                                .Name;
                        }
                    }
                }
            }

            return modelType;
        }
    }
}

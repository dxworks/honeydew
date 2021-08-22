using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Visitors.Method.LocalFunctions
{
    public class LocalFunctionInfoVisitor : ExtractionVisitor<CSharpSyntacticModel, CSharpSemanticModel>,
        ICSharpLocalFunctionVisitor
    {
        public IMethodTypeWithLocalFunctions Visit(LocalFunctionStatementSyntax syntaxNode, IMethodTypeWithLocalFunctions modelType)
        {
            var returnType = InheritedSemanticModel.GetFullName(syntaxNode.ReturnType);
            var returnTypeModifier = InheritedSyntacticModel.SetTypeModifier(syntaxNode.ReturnType.ToString(), "");

            modelType.Name = syntaxNode.Identifier.ToString();
            modelType.ReturnType = new ReturnTypeModel
            {
                Name = returnType,
                Modifier = returnTypeModifier
            };
            modelType.ContainingTypeName = InheritedSemanticModel.GetParentDeclaredType(syntaxNode);
            modelType.Modifier = syntaxNode.Modifiers.ToString();

            modelType.AccessModifier = "";
            modelType.CyclomaticComplexity = InheritedSyntacticModel.CalculateCyclomaticComplexity(syntaxNode);

            foreach (var parameterType in InheritedSemanticModel.ExtractInfoAboutParameters(syntaxNode.ParameterList))
            {
                modelType.ParameterTypes.Add(parameterType);
            }

            return modelType;
        }
    }
}

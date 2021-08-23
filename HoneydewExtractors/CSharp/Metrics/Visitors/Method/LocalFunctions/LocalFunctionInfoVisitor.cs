using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.CSharp.Metrics.Extraction;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Visitors.Method.LocalFunctions
{
    public class LocalFunctionInfoVisitor : IRequireCSharpExtractionHelperMethodsVisitor,
        ICSharpLocalFunctionVisitor
    {
        public CSharpExtractionHelperMethods CSharpHelperMethods { get; set; }

        public void Accept(IVisitor visitor)
        {
        }

        public IMethodTypeWithLocalFunctions Visit(LocalFunctionStatementSyntax syntaxNode,
            IMethodTypeWithLocalFunctions modelType)
        {
            var returnType = CSharpHelperMethods.GetFullName(syntaxNode.ReturnType);
            var returnTypeModifier = CSharpHelperMethods.SetTypeModifier(syntaxNode.ReturnType.ToString(), "");

            modelType.Name = syntaxNode.Identifier.ToString();
            modelType.ReturnType = new ReturnTypeModel
            {
                Name = returnType,
                Modifier = returnTypeModifier
            };
            modelType.ContainingTypeName = CSharpHelperMethods.GetParentDeclaredType(syntaxNode);
            modelType.Modifier = syntaxNode.Modifiers.ToString();

            modelType.AccessModifier = "";
            modelType.CyclomaticComplexity = CSharpHelperMethods.CalculateCyclomaticComplexity(syntaxNode);

            foreach (var parameterType in CSharpHelperMethods.ExtractInfoAboutParameters(syntaxNode.ParameterList))
            {
                modelType.ParameterTypes.Add(parameterType);
            }

            return modelType;
        }
    }
}

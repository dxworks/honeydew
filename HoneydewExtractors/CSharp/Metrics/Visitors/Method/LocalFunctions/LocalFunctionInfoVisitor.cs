using System.Collections.Generic;
using System.Linq;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.CSharp.Metrics.Extraction;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Visitors.Method.LocalFunctions
{
    public class LocalFunctionInfoVisitor : CompositeVisitor, IRequireCSharpExtractionHelperMethodsVisitor,
        ICSharpLocalFunctionVisitor
    {
        public CSharpExtractionHelperMethods CSharpHelperMethods { get; set; }

        public LocalFunctionInfoVisitor(IEnumerable<ILocalFunctionVisitor> visitors) : base(visitors)
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

            if (syntaxNode.Body == null)
            {
                return modelType;
            }

            foreach (var localFunctionStatementSyntax in syntaxNode.Body.ChildNodes()
                .OfType<LocalFunctionStatementSyntax>())
            {
                IMethodTypeWithLocalFunctions localFunction = new MethodModel();
                foreach (var visitor in GetContainedVisitors())
                {
                    if (visitor is ICSharpLocalFunctionVisitor extractionVisitor)
                    {
                        localFunction = extractionVisitor.Visit(localFunctionStatementSyntax, localFunction);
                    }
                }

                localFunction = Visit(localFunctionStatementSyntax, localFunction);

                modelType.LocalFunctions.Add(localFunction);
            }
            
            return modelType;
        }
    }
}

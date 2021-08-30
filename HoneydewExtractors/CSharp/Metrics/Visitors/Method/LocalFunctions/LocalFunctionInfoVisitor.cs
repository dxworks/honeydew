using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Logging;
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
            modelType.ReturnValue = new ReturnValueModel
            {
                Type = returnType,
                Modifier = returnTypeModifier
            };
            modelType.ContainingTypeName = CSharpHelperMethods.GetParentDeclaredType(syntaxNode);
            modelType.Modifier = syntaxNode.Modifiers.ToString();

            modelType.AccessModifier = "";
            modelType.CyclomaticComplexity = CSharpHelperMethods.CalculateCyclomaticComplexity(syntaxNode);

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
                    try
                    {
                        if (visitor is ICSharpLocalFunctionVisitor extractionVisitor)
                        {
                            localFunction = extractionVisitor.Visit(localFunctionStatementSyntax, localFunction);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Log($"Could not extract from Local Function Info Visitor because {e}", LogLevels.Warning);
                    }
                }

                localFunction = Visit(localFunctionStatementSyntax, localFunction);

                modelType.LocalFunctions.Add(localFunction);
            }

            return modelType;
        }
    }
}

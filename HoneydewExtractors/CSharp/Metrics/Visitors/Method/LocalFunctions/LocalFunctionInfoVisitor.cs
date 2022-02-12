using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.CSharp.Metrics.Extraction;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Visitors.Method.LocalFunctions;

public class LocalFunctionInfoVisitor : CompositeVisitor, ICSharpLocalFunctionVisitor
{
    public LocalFunctionInfoVisitor(IEnumerable<ILocalFunctionVisitor> visitors) : base(visitors)
    {
    }

    public IMethodTypeWithLocalFunctions Visit(LocalFunctionStatementSyntax syntaxNode, SemanticModel semanticModel,
        IMethodTypeWithLocalFunctions modelType)
    {
        var returnType =
            CSharpExtractionHelperMethods.GetFullName(syntaxNode.ReturnType, semanticModel, out var isNullable);
        var returnTypeModifier = CSharpExtractionHelperMethods.SetTypeModifier(syntaxNode.ReturnType.ToString(), "");

        modelType.Name = syntaxNode.Identifier.ToString();
        modelType.ReturnValue = new ReturnValueModel
        {
            Type = returnType,
            Modifier = returnTypeModifier,
            IsNullable = isNullable
        };
        modelType.ContainingTypeName = CSharpExtractionHelperMethods.GetParentDeclaredType(syntaxNode, semanticModel);
        modelType.Modifier = syntaxNode.Modifiers.ToString();

        modelType.AccessModifier = "";
        modelType.CyclomaticComplexity = CSharpExtractionHelperMethods.CalculateCyclomaticComplexity(syntaxNode);

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
                        localFunction =
                            extractionVisitor.Visit(localFunctionStatementSyntax, semanticModel, localFunction);
                    }
                }
                catch (Exception e)
                {
                    Logger.Log($"Could not extract from Local Function Info Visitor because {e}", LogLevels.Warning);
                }
            }

            localFunction = Visit(localFunctionStatementSyntax, semanticModel, localFunction);

            modelType.LocalFunctions.Add(localFunction);
        }

        return modelType;
    }
}

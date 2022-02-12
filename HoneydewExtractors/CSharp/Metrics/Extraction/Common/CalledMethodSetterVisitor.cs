using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Destructors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.MethodSignatures;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Common;

public class CalledMethodSetterVisitor : CompositeVisitor, ICSharpMethodVisitor,
    ICSharpConstructorVisitor, ICSharpLocalFunctionVisitor, ICSharpMethodAccessorVisitor,
    ICSharpArrowExpressionMethodVisitor, ICSharpDestructorVisitor
{
    public CalledMethodSetterVisitor(IEnumerable<IMethodSignatureVisitor> visitors) : base(visitors)
    {
    }

    public IMethodType Visit(MethodDeclarationSyntax syntaxNode, SemanticModel semanticModel, IMethodType modelType)
    {
        SetMethodCalls(syntaxNode, semanticModel, modelType);

        return modelType;
    }

    public IConstructorType Visit(ConstructorDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IConstructorType modelType)
    {
        SetMethodCalls(syntaxNode, semanticModel, modelType);

        return modelType;
    }

    public IDestructorType Visit(DestructorDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IDestructorType modelType)
    {
        SetMethodCalls(syntaxNode, semanticModel, modelType);

        return modelType;
    }

    public IMethodType Visit(AccessorDeclarationSyntax syntaxNode, SemanticModel semanticModel, IMethodType modelType)
    {
        SetMethodCalls(syntaxNode, semanticModel, modelType);

        return modelType;
    }

    public IMethodType Visit(ArrowExpressionClauseSyntax syntaxNode, SemanticModel semanticModel, IMethodType modelType)
    {
        SetMethodCalls(syntaxNode, semanticModel, modelType);

        return modelType;
    }

    public IMethodTypeWithLocalFunctions Visit(LocalFunctionStatementSyntax syntaxNode, SemanticModel semanticModel,
        IMethodTypeWithLocalFunctions modelType)
    {
        if (syntaxNode.Body == null)
        {
            return modelType;
        }

        var invocationExpressionSyntaxes =
            syntaxNode.Body.ChildNodes().OfType<InvocationExpressionSyntax>().ToList();

        foreach (var returnStatementSyntax in syntaxNode.Body.ChildNodes().OfType<ReturnStatementSyntax>())
        {
            invocationExpressionSyntaxes.AddRange(returnStatementSyntax.DescendantNodes()
                .OfType<InvocationExpressionSyntax>());
        }

        foreach (var awaitExpressionSyntax in syntaxNode.Body.ChildNodes().OfType<AwaitExpressionSyntax>())
        {
            invocationExpressionSyntaxes.AddRange(awaitExpressionSyntax.DescendantNodes()
                .OfType<InvocationExpressionSyntax>());
        }

        foreach (var awaitExpressionSyntax in
                 syntaxNode.Body.ChildNodes().OfType<LocalDeclarationStatementSyntax>())
        {
            invocationExpressionSyntaxes.AddRange(awaitExpressionSyntax.DescendantNodes()
                .OfType<InvocationExpressionSyntax>());
        }

        foreach (var invocationExpressionSyntax in invocationExpressionSyntaxes)
        {
            IMethodSignatureType methodModel = new MethodModel();

            foreach (var visitor in GetContainedVisitors())
            {
                try
                {
                    if (visitor is ICSharpMethodSignatureVisitor extractionVisitor)
                    {
                        methodModel = extractionVisitor.Visit(invocationExpressionSyntax, semanticModel, methodModel);
                    }
                }
                catch (Exception e)
                {
                    Logger.Log($"Could not extract from Local Function Called Method Visitor because {e}",
                        LogLevels.Warning);
                }
            }

            modelType.CalledMethods.Add(methodModel);
        }

        return modelType;
    }

    private void SetMethodCalls(SyntaxNode syntaxNode, SemanticModel semanticModel,
        ICallingMethodsType callingMethodsType)
    {
        foreach (var invocationExpressionSyntax in
                 syntaxNode.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (invocationExpressionSyntax.GetParentDeclarationSyntax<LocalFunctionStatementSyntax>() != null)
            {
                continue;
            }

            IMethodSignatureType methodModel = new MethodModel();

            foreach (var visitor in GetContainedVisitors())
            {
                try
                {
                    if (visitor is ICSharpMethodSignatureVisitor extractionVisitor)
                    {
                        methodModel = extractionVisitor.Visit(invocationExpressionSyntax, semanticModel, methodModel);
                    }
                }
                catch (Exception e)
                {
                    Logger.Log($"Could not extract from Called Method Visitor because {e}", LogLevels.Warning);
                }
            }

            callingMethodsType.CalledMethods.Add(methodModel);
        }
    }
}

using Honeydew.Extractors.CSharp.Visitors.Utils;
using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using HoneydewCore.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class CalledMethodSetterVisitor : CompositeVisitor, ICSharpMethodVisitor,
    ICSharpConstructorVisitor, ICSharpLocalFunctionVisitor, ICSharpMethodAccessorVisitor,
    ICSharpArrowExpressionMethodVisitor, ICSharpDestructorVisitor
{
    public CalledMethodSetterVisitor(IEnumerable<IMethodCallVisitor> visitors) : base(visitors)
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

    public IAccessorType Visit(AccessorDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IAccessorType modelType)
    {
        SetMethodCalls(syntaxNode, semanticModel, modelType);

        return modelType;
    }

    public IAccessorType Visit(ArrowExpressionClauseSyntax syntaxNode, SemanticModel semanticModel,
        IAccessorType modelType)
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
            IMethodCallType methodCallModel = new MethodCallModel();

            foreach (var visitor in GetContainedVisitors())
            {
                try
                {
                    if (visitor is ICSharpMethodCallVisitor extractionVisitor)
                    {
                        methodCallModel =
                            extractionVisitor.Visit(invocationExpressionSyntax, semanticModel, methodCallModel);
                    }
                }
                catch (Exception e)
                {
                    Logger.Log($"Could not extract from Local Function Called Method Visitor because {e}",
                        LogLevels.Warning);
                }
            }

            modelType.CalledMethods.Add(methodCallModel);
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

            IMethodCallType methodCallModel = new MethodCallModel();

            foreach (var visitor in GetContainedVisitors())
            {
                try
                {
                    if (visitor is ICSharpMethodCallVisitor extractionVisitor)
                    {
                        methodCallModel =
                            extractionVisitor.Visit(invocationExpressionSyntax, semanticModel, methodCallModel);
                    }
                }
                catch (Exception e)
                {
                    Logger.Log($"Could not extract from Called Method Visitor because {e}", LogLevels.Warning);
                }
            }

            callingMethodsType.CalledMethods.Add(methodCallModel);
        }
    }
}

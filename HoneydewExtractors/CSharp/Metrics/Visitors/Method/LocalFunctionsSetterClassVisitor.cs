using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Destructors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Visitors.Method;

public class LocalFunctionsSetterClassVisitor : CompositeVisitor, ICSharpMethodVisitor, ICSharpConstructorVisitor,
    ICSharpLocalFunctionVisitor, ICSharpMethodAccessorVisitor, ICSharpDestructorVisitor
{
    public LocalFunctionsSetterClassVisitor(IEnumerable<ILocalFunctionVisitor> visitors) : base(visitors)
    {
    }

    public IMethodType Visit(MethodDeclarationSyntax syntaxNode, SemanticModel semanticModel, IMethodType modelType)
    {
        if (syntaxNode.Body == null)
        {
            return modelType;
        }

        SetLocalFunctionInfo(syntaxNode.Body, semanticModel, modelType);

        return modelType;
    }

    public IConstructorType Visit(ConstructorDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IConstructorType modelType)
    {
        if (syntaxNode.Body == null)
        {
            return modelType;
        }

        SetLocalFunctionInfo(syntaxNode.Body, semanticModel, modelType);

        return modelType;
    }

    public IDestructorType Visit(DestructorDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IDestructorType modelType)
    {
        if (syntaxNode.Body == null)
        {
            return modelType;
        }

        SetLocalFunctionInfo(syntaxNode.Body, semanticModel, modelType);

        return modelType;
    }

    public IAccessorType Visit(AccessorDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IAccessorType modelType)
    {
        if (syntaxNode.Body == null)
        {
            return modelType;
        }

        SetLocalFunctionInfo(syntaxNode.Body, semanticModel, modelType);

        return modelType;
    }

    public IMethodTypeWithLocalFunctions Visit(LocalFunctionStatementSyntax syntaxNode, SemanticModel semanticModel,
        IMethodTypeWithLocalFunctions modelType)
    {
        if (syntaxNode.Body == null)
        {
            return modelType;
        }

        SetLocalFunctionInfo(syntaxNode.Body, semanticModel, modelType);

        return modelType;
    }

    private void SetLocalFunctionInfo(SyntaxNode syntaxNode, SemanticModel semanticModel,
        ITypeWithLocalFunctions typeWithLocalFunctions)
    {
        foreach (var localFunctionStatementSyntax in
                 syntaxNode.ChildNodes().OfType<LocalFunctionStatementSyntax>())
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
                    Logger.Log($"Could not extract from Local Function Visitor because {e}", LogLevels.Warning);
                }
            }

            typeWithLocalFunctions.LocalFunctions.Add(localFunction);
        }
    }
}

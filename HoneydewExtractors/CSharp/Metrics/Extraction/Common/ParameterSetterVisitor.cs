using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.Parameters;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Common;

public class ParameterSetterVisitor : CompositeVisitor, ICSharpDelegateVisitor, ICSharpMethodVisitor,
    ICSharpConstructorVisitor, ICSharpLocalFunctionVisitor
{
    public ParameterSetterVisitor(IEnumerable<IParameterVisitor> visitors) : base(visitors)
    {
    }

    public IDelegateType Visit(DelegateDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IDelegateType modelType)
    {
        ExtractParameterInfo(syntaxNode, semanticModel, modelType);

        return modelType;
    }

    public IMethodType Visit(MethodDeclarationSyntax syntaxNode, SemanticModel semanticModel, IMethodType modelType)
    {
        ExtractParameterInfo(syntaxNode, semanticModel, modelType);

        return modelType;
    }

    public IConstructorType Visit(ConstructorDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IConstructorType modelType)
    {
        ExtractParameterInfo(syntaxNode, semanticModel, modelType);

        return modelType;
    }

    public IMethodTypeWithLocalFunctions Visit(LocalFunctionStatementSyntax syntaxNode, SemanticModel semanticModel,
        IMethodTypeWithLocalFunctions modelType)
    {
        ExtractParameterInfo(syntaxNode, semanticModel, modelType);

        return modelType;
    }

    private void ExtractParameterInfo(SyntaxNode syntaxNode, SemanticModel semanticModel,
        IMethodSignatureType modelType)
    {
        foreach (var parameterListSyntax in syntaxNode.ChildNodes().OfType<ParameterListSyntax>())
        {
            foreach (var parameterSyntax in parameterListSyntax.Parameters)
            {
                IParameterType parameterModel = new ParameterModel();

                foreach (var visitor in GetContainedVisitors())
                {
                    try
                    {
                        if (visitor is ICSharpParameterVisitor extractionVisitor)
                        {
                            parameterModel = extractionVisitor.Visit(parameterSyntax, semanticModel, parameterModel);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Log($"Could not extract from Parameter Visitor because {e}", LogLevels.Warning);
                    }
                }

                modelType.ParameterTypes.Add(parameterModel);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.MethodSignatures;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Common
{
    public class CalledMethodSetterVisitor : CompositeVisitor, ICSharpPropertyVisitor, ICSharpMethodVisitor,
        ICSharpConstructorVisitor, ICSharpLocalFunctionVisitor
    {
        public CalledMethodSetterVisitor(IEnumerable<IMethodSignatureVisitor> visitors) : base(visitors)
        {
        }

        public IPropertyType Visit(BasePropertyDeclarationSyntax syntaxNode, IPropertyType modelType)
        {
            SetMethodCalls(syntaxNode, modelType);

            return modelType;
        }

        public IMethodType Visit(MethodDeclarationSyntax syntaxNode, IMethodType modelType)
        {
            SetMethodCalls(syntaxNode, modelType);

            return modelType;
        }

        public IConstructorType Visit(ConstructorDeclarationSyntax syntaxNode, IConstructorType modelType)
        {
            SetMethodCalls(syntaxNode, modelType);

            return modelType;
        }

        public IMethodTypeWithLocalFunctions Visit(LocalFunctionStatementSyntax syntaxNode,
            IMethodTypeWithLocalFunctions modelType)
        {
            if (syntaxNode.Body == null)
            {
                return modelType;
            }

            foreach (var invocationExpressionSyntax in
                syntaxNode.Body.ChildNodes().OfType<InvocationExpressionSyntax>())
            {
                IMethodSignatureType methodModel = new MethodModel();

                foreach (var visitor in GetContainedVisitors())
                {
                    try
                    {
                        if (visitor is ICSharpMethodSignatureVisitor extractionVisitor)
                        {
                            methodModel = extractionVisitor.Visit(invocationExpressionSyntax, methodModel);
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

            foreach (var returnStatementSyntax in syntaxNode.Body.ChildNodes().OfType<ReturnStatementSyntax>())
            {
                SetMethodCalls(returnStatementSyntax, modelType);
            }

            return modelType;
        }

        private void SetMethodCalls(SyntaxNode syntaxNode, ICallingMethodsType callingMethodsType)
        {
            foreach (var invocationExpressionSyntax in
                syntaxNode.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                IMethodSignatureType methodModel = new MethodModel();

                foreach (var visitor in GetContainedVisitors())
                {
                    try
                    {
                        if (visitor is ICSharpMethodSignatureVisitor extractionVisitor)
                        {
                            methodModel = extractionVisitor.Visit(invocationExpressionSyntax, methodModel);
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
}

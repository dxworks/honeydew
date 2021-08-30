using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Visitors.Method
{
    public class LocalFunctionsSetterClassVisitor : CompositeVisitor, ICSharpMethodVisitor,
        ICSharpConstructorVisitor, ICSharpLocalFunctionVisitor, ICSharpMethodAccessorVisitor
    {
        public LocalFunctionsSetterClassVisitor(IEnumerable<ILocalFunctionVisitor> visitors) : base(visitors)
        {
        }

        public IMethodType Visit(MethodDeclarationSyntax syntaxNode, IMethodType modelType)
        {
            if (modelType is not MethodModel methodModel)
            {
                return modelType;
            }

            if (syntaxNode.Body == null)
            {
                return methodModel;
            }

            SetLocalFunctionInfo(syntaxNode.Body, methodModel);

            return methodModel;
        }

        public IConstructorType Visit(ConstructorDeclarationSyntax syntaxNode, IConstructorType modelType)
        {
            if (modelType is not ConstructorModel constructorModel)
            {
                return modelType;
            }

            if (syntaxNode.Body == null)
            {
                return constructorModel;
            }

            SetLocalFunctionInfo(syntaxNode.Body, constructorModel);

            return constructorModel;
        }

        public IMethodType Visit(AccessorDeclarationSyntax syntaxNode, IMethodType modelType)
        {
            if (modelType is not MethodModel methodModel)
            {
                return modelType;
            }

            if (syntaxNode.Body == null)
            {
                return methodModel;
            }

            SetLocalFunctionInfo(syntaxNode.Body, methodModel);

            return methodModel;
        }

        public IMethodTypeWithLocalFunctions Visit(LocalFunctionStatementSyntax syntaxNode,
            IMethodTypeWithLocalFunctions modelType)
        {
            if (syntaxNode.Body == null)
            {
                return modelType;
            }

            SetLocalFunctionInfo(syntaxNode.Body, modelType);

            return modelType;
        }

        private void SetLocalFunctionInfo(SyntaxNode syntaxNode, ITypeWithLocalFunctions typeWithLocalFunctions)
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
                            localFunction = extractionVisitor.Visit(localFunctionStatementSyntax, localFunction);
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
}

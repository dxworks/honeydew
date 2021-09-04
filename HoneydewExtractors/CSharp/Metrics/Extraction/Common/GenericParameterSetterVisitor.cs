using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.Parameters;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Common
{
    public class GenericParameterSetterVisitor : CompositeVisitor, ICSharpClassVisitor, ICSharpDelegateVisitor,
        ICSharpMethodVisitor, ICSharpLocalFunctionVisitor
    {
        public GenericParameterSetterVisitor(IEnumerable<IGenericParameterVisitor> visitors) : base(visitors)
        {
        }

        public IClassType Visit(BaseTypeDeclarationSyntax syntaxNode, IClassType modelType)
        {
            ExtractParameterInfo(syntaxNode, modelType);

            return modelType;
        }

        public IDelegateType Visit(DelegateDeclarationSyntax syntaxNode, IDelegateType modelType)
        {
            ExtractParameterInfo(syntaxNode, modelType);

            return modelType;
        }

        public IMethodType Visit(MethodDeclarationSyntax syntaxNode, IMethodType modelType)
        {
            return modelType;
        }

        public IMethodTypeWithLocalFunctions Visit(LocalFunctionStatementSyntax syntaxNode,
            IMethodTypeWithLocalFunctions modelType)
        {
            return modelType;
        }

        private void ExtractParameterInfo(SyntaxNode syntaxNode, ITypeWithGenericParameters modelType)
        {
            foreach (var typeParameterListSyntax in syntaxNode.ChildNodes().OfType<TypeParameterListSyntax>())
            {
                foreach (var typeParameterSyntax in typeParameterListSyntax.Parameters)
                {
                    IGenericParameterType parameterModel = new GenericParameterModel();

                    foreach (var visitor in GetContainedVisitors())
                    {
                        try
                        {
                            if (visitor is ICSharpGenericParameterVisitor extractionVisitor)
                            {
                                parameterModel = extractionVisitor.Visit(typeParameterSyntax, parameterModel);
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Log($"Could not extract from Generic Parameter Visitor because {e}",
                                LogLevels.Warning);
                        }
                    }

                    modelType.GenericParameters.Add(parameterModel);
                }
            }
        }
    }
}

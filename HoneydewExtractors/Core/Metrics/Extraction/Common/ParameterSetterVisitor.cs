using System.Collections.Generic;
using System.Linq;
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

namespace HoneydewExtractors.Core.Metrics.Extraction.Common
{
    public class ParameterSetterVisitor : CompositeVisitor, ICSharpDelegateVisitor, ICSharpMethodVisitor,
        ICSharpConstructorVisitor, ICSharpLocalFunctionVisitor
    {
        public ParameterSetterVisitor(IEnumerable<IParameterVisitor> visitors): base(visitors)
        {
        }
        
        public IDelegateType Visit(DelegateDeclarationSyntax syntaxNode, IDelegateType modelType)
        {
            ExtractParameterInfo(syntaxNode, modelType);

            return modelType;
        }

        public IMethodType Visit(MethodDeclarationSyntax syntaxNode, IMethodType modelType)
        {
            ExtractParameterInfo(syntaxNode, modelType);

            return modelType;
        }

        public IConstructorType Visit(ConstructorDeclarationSyntax syntaxNode, IConstructorType modelType)
        {
            ExtractParameterInfo(syntaxNode, modelType);

            return modelType;
        }

        public IMethodTypeWithLocalFunctions Visit(LocalFunctionStatementSyntax syntaxNode,
            IMethodTypeWithLocalFunctions modelType)
        {
            ExtractParameterInfo(syntaxNode, modelType);

            return modelType;
        }

        private void ExtractParameterInfo(SyntaxNode syntaxNode, IMethodSignatureType modelType)
        {
            foreach (var parameterListSyntax in syntaxNode.ChildNodes().OfType<ParameterListSyntax>())
            {
                foreach (var parameterSyntax in parameterListSyntax.Parameters)
                {
                    IParameterType parameterModel = new ParameterModel();

                    foreach (var visitor in GetContainedVisitors())
                    {
                        if (visitor is ICSharpParameterVisitor extractionVisitor)
                        {
                            parameterModel = extractionVisitor.Visit(parameterSyntax, parameterModel);
                        }
                    }

                    modelType.ParameterTypes.Add(parameterModel);
                }
            }
        }
    }
}

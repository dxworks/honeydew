using System.Linq;
using HoneydewExtractors.Core.Metrics.Extraction.ModelCreators;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Common
{
    public class CalledMethodSetterVisitor : CompositeTypeVisitor, ICSharpPropertyVisitor, ICSharpMethodVisitor,
        ICSharpConstructorVisitor, ICSharpLocalFunctionVisitor
    {
        private readonly CSharpMethodCallModelCreator _cSharpMethodCallModelCreator;

        public CalledMethodSetterVisitor(CSharpMethodCallModelCreator cSharpMethodCallModelCreator)
        {
            _cSharpMethodCallModelCreator = cSharpMethodCallModelCreator;

            foreach (var visitor in _cSharpMethodCallModelCreator.GetVisitors())
            {
                Add(visitor);
            }
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
                modelType.CalledMethods.Add(_cSharpMethodCallModelCreator.Create(invocationExpressionSyntax,
                    new MethodModel()));
            }

            foreach (var returnStatementSyntax in syntaxNode.Body.ChildNodes().OfType<ReturnStatementSyntax>())
            {
                foreach (var invocationExpressionSyntax in
                    returnStatementSyntax.DescendantNodes().OfType<InvocationExpressionSyntax>())
                {
                    modelType.CalledMethods.Add(_cSharpMethodCallModelCreator.Create(invocationExpressionSyntax,
                        new MethodModel()));
                }
            }

            return modelType;
        }

        private void SetMethodCalls(SyntaxNode syntaxNode, ICallingMethodsType callingMethodsType)
        {
            foreach (var invocationExpressionSyntax in
                syntaxNode.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                callingMethodsType.CalledMethods.Add(_cSharpMethodCallModelCreator.Create(invocationExpressionSyntax,
                    new MethodModel()));
            }
        }
    }
}

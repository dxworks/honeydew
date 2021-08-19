using System.Linq;
using HoneydewExtractors.Core.Metrics.Extraction.ModelCreators;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Common
{
    public class CalledMethodSetterVisitor : CompositeTypeVisitor, ICSharpPropertyVisitor, ICSharpMethodVisitor,
        ICSharpConstructorVisitor
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
            foreach (var invocationExpressionSyntax in
                syntaxNode.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                modelType.CalledMethods.Add(_cSharpMethodCallModelCreator.Create(invocationExpressionSyntax,
                    new MethodModel()));
            }

            return modelType;
        }

        public IMethodType Visit(MethodDeclarationSyntax syntaxNode, IMethodType modelType)
        {
            foreach (var invocationExpressionSyntax in
                syntaxNode.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                modelType.CalledMethods.Add(_cSharpMethodCallModelCreator.Create(invocationExpressionSyntax,
                    new MethodModel()));
            }

            return modelType;
        }

        public IConstructorType Visit(ConstructorDeclarationSyntax syntaxNode, IConstructorType modelType)
        {
            foreach (var invocationExpressionSyntax in
                syntaxNode.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                modelType.CalledMethods.Add(_cSharpMethodCallModelCreator.Create(invocationExpressionSyntax,
                    new MethodModel()));
            }

            return modelType;
        }
    }
}

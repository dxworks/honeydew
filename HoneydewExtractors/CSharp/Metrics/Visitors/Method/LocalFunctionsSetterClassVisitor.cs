using System.Linq;
using HoneydewExtractors.Core.Metrics.Extraction.ModelCreators;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Visitors.Method
{
    public class LocalFunctionsSetterClassVisitor : CompositeTypeVisitor, ICSharpMethodVisitor,
        ICSharpConstructorVisitor, ICSharpPropertyVisitor, ICSharpLocalFunctionVisitor
    {
        private readonly CSharpLocalFunctionsModelCreator _cSharpLocalFunctionsModelCreator;

        public LocalFunctionsSetterClassVisitor(CSharpLocalFunctionsModelCreator cSharpLocalFunctionsModelCreator)
        {
            _cSharpLocalFunctionsModelCreator = cSharpLocalFunctionsModelCreator;

            foreach (var visitor in _cSharpLocalFunctionsModelCreator.GetVisitors())
            {
                Add(visitor);
            }
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

            foreach (var localFunctionStatementSyntax in
                syntaxNode.Body.ChildNodes().OfType<LocalFunctionStatementSyntax>())
            {
                methodModel.LocalFunctions.Add(
                    _cSharpLocalFunctionsModelCreator.Create(localFunctionStatementSyntax, new MethodModel()));
            }

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

            foreach (var localFunctionStatementSyntax in
                syntaxNode.Body.ChildNodes().OfType<LocalFunctionStatementSyntax>())
            {
                constructorModel.LocalFunctions.Add(
                    _cSharpLocalFunctionsModelCreator.Create(localFunctionStatementSyntax, new MethodModel()));
            }

            return constructorModel;
        }

        public IPropertyType Visit(BasePropertyDeclarationSyntax syntaxNode, IPropertyType modelType)
        {
            if (modelType is not PropertyModel propertyModel)
            {
                return modelType;
            }

            if (syntaxNode.AccessorList == null)
            {
                return propertyModel;
            }

            foreach (var accessor in syntaxNode.AccessorList.Accessors)
            {
                if (accessor.Body == null)
                {
                    continue;
                }

                foreach (var localFunctionStatementSyntax in accessor.Body.ChildNodes()
                    .OfType<LocalFunctionStatementSyntax>())
                {
                    propertyModel.LocalFunctions.Add(
                        _cSharpLocalFunctionsModelCreator.Create(localFunctionStatementSyntax, new MethodModel()));
                }
            }

            return propertyModel;
        }

        public IMethodTypeWithLocalFunctions Visit(LocalFunctionStatementSyntax syntaxNode,
            IMethodTypeWithLocalFunctions modelType)
        {
            if (syntaxNode.Body == null)
            {
                return modelType;
            }

            foreach (var localFunctionStatementSyntax in
                syntaxNode.Body.ChildNodes().OfType<LocalFunctionStatementSyntax>())
            {
                modelType.LocalFunctions.Add(
                    _cSharpLocalFunctionsModelCreator.Create(localFunctionStatementSyntax, new MethodModel()));
            }

            return modelType;
        }
    }
}

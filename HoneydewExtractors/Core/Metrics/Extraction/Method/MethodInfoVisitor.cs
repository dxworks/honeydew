using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.CSharp.Metrics.Extraction;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Method
{
    public class MethodInfoVisitor : IRequireCSharpExtractionHelperMethodsVisitor,
        ICSharpMethodVisitor, ICSharpMethodAccessorVisitor, ICSharpArrowExpressionMethodVisitor
    {
        public CSharpExtractionHelperMethods CSharpHelperMethods { get; set; }

        public void Accept(IVisitor visitor)
        {
        }

        public IMethodType Visit(MethodDeclarationSyntax syntaxNode, IMethodType modelType)
        {
            var containingClassName = CSharpHelperMethods.GetParentDeclaredType(syntaxNode);

            var isInterface = CSharpHelperMethods.GetParentDeclarationSyntax<InterfaceDeclarationSyntax>(syntaxNode) !=
                              null;
            var accessModifier = isInterface
                ? CSharpConstants.DefaultInterfaceMethodAccessModifier
                : CSharpConstants.DefaultClassMethodAccessModifier;
            var modifier = isInterface
                ? CSharpConstants.DefaultInterfaceMethodModifier
                : syntaxNode.Modifiers.ToString();

            CSharpConstants.SetModifiers(syntaxNode.Modifiers.ToString(), ref accessModifier, ref modifier);

            var returnType = CSharpHelperMethods.GetFullName(syntaxNode.ReturnType);

            var returnTypeModifier = CSharpHelperMethods.SetTypeModifier(syntaxNode.ReturnType.ToString(), "");


            modelType.Name = syntaxNode.Identifier.ToString();
            modelType.ReturnValue = new ReturnValueModel
            {
                Type = returnType,
                Modifier = returnTypeModifier
            };
            modelType.ContainingTypeName = containingClassName;
            modelType.Modifier = modifier;
            modelType.AccessModifier = accessModifier;
            modelType.CyclomaticComplexity = CSharpHelperMethods.CalculateCyclomaticComplexity(syntaxNode);

            return modelType;
        }

        public IMethodType Visit(AccessorDeclarationSyntax syntaxNode, IMethodType modelType)
        {
            var accessModifier = "public";
            var modifier = syntaxNode.Modifiers.ToString();

            CSharpConstants.SetModifiers(syntaxNode.Modifiers.ToString(), ref accessModifier, ref modifier);
            var keyword = syntaxNode.Keyword.ToString();

            IEntityType returnType = new EntityTypeModel
            {
                Name = "void"
            };
            if (keyword == "get")
            {
                var basePropertyDeclarationSyntax =
                    CSharpHelperMethods.GetParentDeclarationSyntax<BasePropertyDeclarationSyntax>(syntaxNode);
                if (basePropertyDeclarationSyntax != null)
                {
                    returnType = CSharpHelperMethods.GetFullName(basePropertyDeclarationSyntax.Type);
                }
            }

            modelType.Name = keyword;
            modelType.ReturnValue = new ReturnValueModel
            {
                Type = returnType
            };
            modelType.ContainingTypeName = CSharpHelperMethods.GetParentDeclaredType(syntaxNode);
            modelType.Modifier = modifier;
            modelType.AccessModifier = accessModifier;
            modelType.CyclomaticComplexity = CSharpHelperMethods.CalculateCyclomaticComplexity(syntaxNode);

            return modelType;
        }

        public IMethodType Visit(ArrowExpressionClauseSyntax syntaxNode, IMethodType modelType)
        {
            IEntityType returnType = new EntityTypeModel
            {
                Name = "void"
            };
            var basePropertyDeclarationSyntax =
                CSharpHelperMethods.GetParentDeclarationSyntax<BasePropertyDeclarationSyntax>(syntaxNode);
            if (basePropertyDeclarationSyntax != null)
            {
                returnType = CSharpHelperMethods.GetFullName(basePropertyDeclarationSyntax.Type);
            }

            modelType.Name = "get";
            modelType.AccessModifier = "public";
            modelType.ContainingTypeName = CSharpHelperMethods.GetParentDeclaredType(syntaxNode);
            modelType.ReturnValue = new ReturnValueModel
            {
                Type = returnType
            };
            modelType.CyclomaticComplexity = CSharpHelperMethods.CalculateCyclomaticComplexity(syntaxNode);

            return modelType;
        }
    }
}

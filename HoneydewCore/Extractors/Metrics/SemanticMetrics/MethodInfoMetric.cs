using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Models;
using HoneydewCore.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewCore.Extractors.Metrics.SemanticMetrics
{
    public class MethodInfoMetric : CSharpMetricExtractor, ISemanticMetric
    {
        public IList<MethodModel> MethodInfos { get; } = new List<MethodModel>();

        private string _containingClassName = "";
        private string _baseTypeName = CSharpConstants.ObjectIdentifier;
        private bool _isInterface;

        public override IMetric GetMetric()
        {
            return new Metric<IList<MethodModel>>(MethodInfos);
        }

        public override string PrettyPrint()
        {
            return "Methods Info";
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            AddInfoForNode(node);
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            _isInterface = true;
            AddInfoForNode(node);
        }

        public override void VisitRecordDeclaration(RecordDeclarationSyntax node)
        {
            AddInfoForNode(node);
        }

        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            AddInfoForNode(node);
        }

        private void AddInfoForNode(TypeDeclarationSyntax node)
        {
            _containingClassName = ExtractorSemanticModel.GetDeclaredSymbol(node)?.ToDisplayString();
            _baseTypeName = ExtractorSemanticModel.GetDeclaredSymbol(node)?.BaseType?.ToDisplayString();

            foreach (var memberDeclarationSyntax in node.Members)
            {
                if (memberDeclarationSyntax is MethodDeclarationSyntax syntax)
                {
                    AddMethodInfo(syntax);
                }
            }
        }

        private void AddMethodInfo(MethodDeclarationSyntax node)
        {
            GetModifiersForNode(node, out var accessModifier, out var modifier);

            var returnType = node.ReturnType.ToString();

            var returnValueSymbol = ExtractorSemanticModel.GetDeclaredSymbol(node.ReturnType);
            if (returnValueSymbol != null)
            {
                returnType = returnValueSymbol.ToString();
            }

            var methodModel = new MethodModel
            {
                Name = node.Identifier.ToString(),
                ReturnType = returnType,
                ContainingClassName = _containingClassName,
                Modifier = modifier,
                AccessModifier = accessModifier,
            };

            foreach (var parameter in node.ParameterList.Parameters)
            {
                var parameterSymbol = ExtractorSemanticModel.GetDeclaredSymbol(parameter);
                if (parameterSymbol != null)
                {
                    methodModel.ParameterTypes.Add(parameterSymbol.ToString());
                }
                else if (parameter.Type != null)
                {
                    methodModel.ParameterTypes.Add(parameter.Type.ToString());
                }
            }

            if (node.Body != null)
            {
                foreach (var invocationExpressionSyntax in node.Body.DescendantNodes()
                    .OfType<InvocationExpressionSyntax>())
                {
                    switch (invocationExpressionSyntax.Expression)
                    {
                        case IdentifierNameSyntax:
                            methodModel.CalledMethods.Add(new MethodCallModel
                            {
                                MethodName = invocationExpressionSyntax.Expression.ToString(),
                                ContainingClassName = _containingClassName,
                                ParameterTypes = GetParameterTypes(invocationExpressionSyntax)
                            });
                            break;
                        case MemberAccessExpressionSyntax memberAccessExpressionSyntax:
                        {
                            var className = _containingClassName;

                            if (memberAccessExpressionSyntax.Expression.ToFullString() ==
                                CSharpConstants.BaseClassIdentifier)
                            {
                                className = _baseTypeName;
                            }
                            else
                            {
                                var symbolInfo =
                                    ExtractorSemanticModel.GetSymbolInfo(memberAccessExpressionSyntax.Expression);
                                if (symbolInfo.Symbol is ILocalSymbol localSymbol)
                                {
                                    className = localSymbol.Type.ToDisplayString();
                                }
                                else if (symbolInfo.Symbol != null)
                                {
                                    className = symbolInfo.Symbol.ToString();
                                }
                            }

                            methodModel.CalledMethods.Add(new MethodCallModel
                            {
                                MethodName = memberAccessExpressionSyntax.Name.ToString(),
                                ContainingClassName = className,
                                ParameterTypes = GetParameterTypes(invocationExpressionSyntax)
                            });
                            break;
                        }
                    }
                }
            }

            MethodInfos.Add(methodModel);
        }

        private IList<string> GetParameterTypes(ExpressionSyntax invocationExpressionSyntax)
        {
            IList<string> parameterTypes = new List<string>();

            var symbolInfo = ExtractorSemanticModel.GetSymbolInfo(invocationExpressionSyntax);
            if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
            {
                foreach (var parameter in methodSymbol.Parameters)
                {
                    parameterTypes.Add(parameter.ToString());
                }
            }

            return parameterTypes;
        }

        private void GetModifiersForNode(MemberDeclarationSyntax node, out string accessModifier, out string modifier)
        {
            var allModifiers = node.Modifiers.ToString();

            accessModifier = _isInterface
                ? CSharpConstants.DefaultInterfaceMethodAccessModifier
                : CSharpConstants.DefaultClassMethodAccessModifier;
            modifier = _isInterface ? CSharpConstants.DefaultInterfaceMethodModifier : allModifiers;

            CSharpConstants.SetModifiers(allModifiers, ref accessModifier, ref modifier);
        }
    }
}
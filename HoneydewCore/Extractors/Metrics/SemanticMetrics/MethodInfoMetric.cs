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
        public MethodInfoDataMetric DataMetric { get; set; } = new();

        private string _containingClassName = "";
        private string _baseTypeName = CSharpConstants.ObjectIdentifier;
        private bool _isInterface;

        public override IMetric GetMetric()
        {
            return new Metric<MethodInfoDataMetric>(DataMetric);
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
                switch (memberDeclarationSyntax)
                {
                    case MethodDeclarationSyntax syntax:
                        AddMethodInfo(syntax);
                        break;
                    case ConstructorDeclarationSyntax constructorSyntax:
                        AddMethodInfo(constructorSyntax);
                        break;
                }
            }
        }

        private void AddMethodInfo(ConstructorDeclarationSyntax syntax)
        {
            GetModifiersForNode(syntax, out var accessModifier, out var modifier);

            var methodModel = new MethodModel
            {
                Name = syntax.Identifier.ToString(),
                ReturnType = null,
                ContainingClassName = _containingClassName,
                Modifier = modifier,
                AccessModifier = accessModifier,
                IsConstructor = true
            };

            ExtractInfoAboutParameters(syntax, methodModel);

            ExtractInfoAboutConstructorCalls(syntax, methodModel);

            ExtractInfoAboutCalledMethods(syntax, methodModel);

            DataMetric.ConstructorInfos.Add(methodModel);
        }

        private void AddMethodInfo(MethodDeclarationSyntax syntax)
        {
            GetModifiersForNode(syntax, out var accessModifier, out var modifier);

            var returnType = syntax.ReturnType.ToString();

            var returnValueSymbol = ExtractorSemanticModel.GetDeclaredSymbol(syntax.ReturnType);
            if (returnValueSymbol != null)
            {
                returnType = returnValueSymbol.ToString();
            }

            var methodModel = new MethodModel
            {
                Name = syntax.Identifier.ToString(),
                ReturnType = returnType,
                ContainingClassName = _containingClassName,
                Modifier = modifier,
                AccessModifier = accessModifier,
            };

            ExtractInfoAboutParameters(syntax, methodModel);

            ExtractInfoAboutCalledMethods(syntax, methodModel);

            DataMetric.MethodInfos.Add(methodModel);
        }

        private void ExtractInfoAboutParameters(BaseMethodDeclarationSyntax syntax, MethodModel methodModel)
        {
            foreach (var parameter in syntax.ParameterList.Parameters)
            {
                var parameterSymbol = ExtractorSemanticModel.GetDeclaredSymbol(parameter);
                var parameterType = parameter.Type?.ToString();
                if (parameterSymbol != null)
                {
                    parameterType = parameterSymbol.Type.ToDisplayString();
                }

                methodModel.ParameterTypes.Add(new ParameterModel
                {
                    Type = parameterType,
                    Modifier = parameter.Modifiers.ToString(),
                    DefaultValue = parameter.Default?.Value.ToString()
                });
            }
        }

        private void ExtractInfoAboutCalledMethods(BaseMethodDeclarationSyntax syntax, MethodModel methodModel)
        {
            if (syntax.Body == null)
            {
                return;
            }

            foreach (var invocationExpressionSyntax in syntax.Body.DescendantNodes()
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

        private void ExtractInfoAboutConstructorCalls(ConstructorDeclarationSyntax syntax, MethodModel methodModel)
        {
            if (syntax.Initializer == null)
            {
                return;
            }

            var containingClassName = _containingClassName;

            var methodName = syntax.Identifier.ToString();
            if (syntax.Initializer.ThisOrBaseKeyword.ValueText == "base")
            {
                containingClassName = _baseTypeName;
                methodName = _baseTypeName;
            }

            IList<ParameterModel> parameterModels = new List<ParameterModel>();


            var symbol = ExtractorSemanticModel.GetSymbolInfo(syntax.Initializer).Symbol;
            if (symbol is IMethodSymbol methodSymbol)
            {
                parameterModels = GetParameterTypes(methodSymbol);
                methodName = methodSymbol.ContainingType.Name;
            }

            methodModel.CalledMethods.Add(new MethodCallModel
            {
                MethodName = methodName,
                ContainingClassName = containingClassName,
                ParameterTypes = parameterModels
            });
        }

        private IList<ParameterModel> GetParameterTypes(ExpressionSyntax invocationExpressionSyntax)
        {
            var symbolInfo = ExtractorSemanticModel.GetSymbolInfo(invocationExpressionSyntax);
            return symbolInfo.Symbol is not IMethodSymbol methodSymbol
                ? new List<ParameterModel>()
                : GetParameterTypes(methodSymbol);
        }

        private static IList<ParameterModel> GetParameterTypes(IMethodSymbol methodSymbol)
        {
            IList<ParameterModel> parameterTypes = new List<ParameterModel>();
            foreach (var parameter in methodSymbol.Parameters)
            {
                var modifier = parameter.RefKind switch
                {
                    RefKind.In => "in",
                    RefKind.Out => "out",
                    RefKind.Ref => "ref",
                    _ => ""
                };

                if (parameter.IsParams)
                {
                    modifier = "params";
                }

                if (parameter.IsThis)
                {
                    modifier = "this";
                }

                string defaultValue = null;
                if (parameter.HasExplicitDefaultValue)
                {
                    defaultValue = parameter.ExplicitDefaultValue!.ToString();
                }

                parameterTypes.Add(new ParameterModel
                {
                    Type = parameter.Type.ToString(),
                    Modifier = modifier,
                    DefaultValue = defaultValue
                });
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
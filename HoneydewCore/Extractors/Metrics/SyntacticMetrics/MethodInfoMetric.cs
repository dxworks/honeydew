using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewCore.Extractors.Metrics.SyntacticMetrics
{
    public class MethodInfoMetric : CSharpMetricExtractor, ISemanticMetric
    {
        public IList<MethodModel> MethodInfos { get; } = new List<MethodModel>();

        private string _containingClassName = "";
        private string _baseTypeName = "object";
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
            _containingClassName = SemanticModel.GetDeclaredSymbol(node)?.ToDisplayString();
            _baseTypeName = (SemanticModel.GetDeclaredSymbol(node) as ITypeSymbol)?.BaseType?.ToDisplayString();

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

            var methodModel = new MethodModel
            {
                Name = node.Identifier.ToString(),
                ReturnType = node.ReturnType.ToString(),
                ContainingClassName = _containingClassName,
                Modifier = modifier,
                AccessModifier = accessModifier,
            };

            foreach (var parameter in node.ParameterList.Parameters)
            {
                methodModel.ParameterTypes.Add(parameter.Type?.ToString());
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
                                ContainingClassName = _containingClassName
                            });
                            break;
                        case MemberAccessExpressionSyntax memberAccessExpressionSyntax:
                        {
                            var className = _containingClassName;
                            
                            if (memberAccessExpressionSyntax.Expression.ToFullString() == "base")
                            {
                                className = _baseTypeName;
                            }
                            else
                            {
                                var symbolInfo = SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax.Expression);
                                if (symbolInfo.Symbol is ILocalSymbol localSymbol)
                                {
                                    className = localSymbol.Type.ToDisplayString();
                                }
                            }

                            methodModel.CalledMethods.Add(new MethodCallModel
                            {
                                MethodName = memberAccessExpressionSyntax.Name.ToString(),
                                ContainingClassName = className
                            });
                            break;
                        }
                    }
                }
            }

            MethodInfos.Add(methodModel);
        }

        private void GetModifiersForNode(MemberDeclarationSyntax node, out string accessModifier, out string modifier)
        {
            var allModifiers = node.Modifiers.ToString();
            accessModifier = allModifiers;
            modifier = "";
            string[] modifiers = {"virtual", "static", "override", "abstract"};
            foreach (var m in modifiers)
            {
                if (!allModifiers.Contains(m)) continue;

                modifier = m;
                accessModifier = allModifiers.Replace(m, "");
                accessModifier = accessModifier.Trim();
                break;
            }

            if (string.IsNullOrEmpty(accessModifier))
            {
                accessModifier = _isInterface ? "public" : "private";
                if (_isInterface)
                {
                    if (string.IsNullOrEmpty(modifier))
                    {
                        modifier = "abstract";
                    }
                }
            }
        }
    }
}
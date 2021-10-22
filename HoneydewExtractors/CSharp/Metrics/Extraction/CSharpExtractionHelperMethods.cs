using System.Collections.Generic;
using System.Linq;
using System.Text;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction
{
    public class CSharpExtractionHelperMethods : IExtractionHelperMethods
    {
        private readonly SemanticModel _semanticModel;
        private readonly CSharpFullNameProvider _fullNameProvider;

        public CSharpExtractionHelperMethods(CSharpSemanticModel semanticModel)
        {
            _semanticModel = semanticModel.Model;
            _fullNameProvider = new CSharpFullNameProvider(_semanticModel);
        }

        public IEntityType GetFullName(SyntaxNode syntaxNode, out bool isNullable)
        {
            return _fullNameProvider.GetFullName(syntaxNode, out isNullable);
        }

        public IEntityType GetFullName(SyntaxNode syntaxNode)
        {
            return _fullNameProvider.GetFullName(syntaxNode, out _);
        }

        public IList<IEntityType> GetBaseInterfaces(BaseTypeDeclarationSyntax node)
        {
            var declaredSymbol = _semanticModel.GetDeclaredSymbol(node);

            IList<IEntityType> interfaces = new List<IEntityType>();

            if (declaredSymbol is not ITypeSymbol typeSymbol)
            {
                return interfaces;
            }

            foreach (var interfaceSymbol in typeSymbol.Interfaces)
            {
                interfaces.Add(_fullNameProvider.CreateEntityTypeModel(interfaceSymbol.ToString()));
            }

            return interfaces;
        }

        public IEntityType GetBaseClassName(TypeDeclarationSyntax node)
        {
            var declaredSymbol = ModelExtensions.GetDeclaredSymbol(_semanticModel, node);

            if (declaredSymbol is not ITypeSymbol typeSymbol)
            {
                return _fullNameProvider.CreateEntityTypeModel(CSharpConstants.ObjectIdentifier);
            }

            if (typeSymbol.BaseType == null)
            {
                return _fullNameProvider.CreateEntityTypeModel(CSharpConstants.ObjectIdentifier);
            }

            return _fullNameProvider.CreateEntityTypeModel(typeSymbol.BaseType.ToString());
        }

        public IEntityType GetBaseClassName(BaseTypeDeclarationSyntax baseTypeDeclarationSyntax)
        {
            if (baseTypeDeclarationSyntax is TypeDeclarationSyntax typeDeclarationSyntax)
            {
                return GetBaseClassName(typeDeclarationSyntax);
            }

            return _fullNameProvider.CreateEntityTypeModel(CSharpConstants.ObjectIdentifier);
        }

        public IMethodSymbol GetMethodSymbol(CSharpSyntaxNode expressionSyntax)
        {
            var symbolInfo = _semanticModel.GetSymbolInfo(expressionSyntax);
            var symbol = symbolInfo.Symbol;
            if (symbol is IMethodSymbol methodSymbol)
            {
                return methodSymbol;
            }

            return null;
        }

        public MethodCallModel GetMethodCallModel(InvocationExpressionSyntax invocationExpressionSyntax)
        {
            var calledMethodName = invocationExpressionSyntax.Expression.ToString();

            var containingClassName = GetContainingType(invocationExpressionSyntax).Name;

            return invocationExpressionSyntax.Expression switch
            {
                MemberAccessExpressionSyntax memberAccessExpressionSyntax => new MethodCallModel
                {
                    Name = memberAccessExpressionSyntax.Name.ToString(),
                    ContainingTypeName = containingClassName,
                    ParameterTypes = GetParameters(invocationExpressionSyntax)
                },
                _ => new MethodCallModel
                {
                    Name = calledMethodName,
                    ContainingTypeName = containingClassName,
                    ParameterTypes = GetParameters(invocationExpressionSyntax)
                }
            };
        }

        public IList<IParameterType> GetParameters(IMethodSymbol methodSymbol)
        {
            IList<IParameterType> parameters = new List<IParameterType>();
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
                    defaultValue = parameter.ExplicitDefaultValue == null
                        ? "null"
                        : parameter.ExplicitDefaultValue.ToString();
                    if (defaultValue is "False" or "True")
                    {
                        defaultValue = defaultValue.ToLower();
                    }
                }

                parameters.Add(new ParameterModel
                {
                    Type = _fullNameProvider.CreateEntityTypeModel(parameter.Type.ToString()),
                    Modifier = modifier,
                    DefaultValue = defaultValue
                });
            }

            return parameters;
        }

        public string GetAliasTypeOfNamespace(NameSyntax nodeName)
        {
            var symbolInfo = ModelExtensions.GetSymbolInfo(_semanticModel, nodeName);
            return symbolInfo.Symbol switch
            {
                null => nameof(EAliasType.NotDetermined),
                INamespaceSymbol => nameof(EAliasType.Namespace),
                _ => nameof(EAliasType.Class)
            };
        }

        public ParameterModel ExtractInfoAboutParameter(BaseParameterSyntax baseParameterSyntax)
        {
            var parameterType = GetFullName(baseParameterSyntax.Type, out var isNullable);

            string defaultValue = null;

            if (baseParameterSyntax is ParameterSyntax parameterSyntax)
            {
                defaultValue = parameterSyntax.Default?.Value.ToString();
            }

            return new ParameterModel
            {
                Type = parameterType,
                Modifier = baseParameterSyntax.Modifiers.ToString(),
                DefaultValue = defaultValue,
                IsNullable = isNullable
            };
        }

        public string GetParentDeclaredType(SyntaxNode syntaxNode)
        {
            CSharpSyntaxNode declarationSyntax = syntaxNode.GetParentDeclarationSyntax<LocalFunctionStatementSyntax>();

            if (declarationSyntax == null)
            {
                declarationSyntax = syntaxNode.GetParentDeclarationSyntax<BaseMethodDeclarationSyntax>();
            }

            if (declarationSyntax == null)
            {
                declarationSyntax = syntaxNode.GetParentDeclarationSyntax<BasePropertyDeclarationSyntax>();
            }

            if (declarationSyntax == null)
            {
                declarationSyntax = syntaxNode.GetParentDeclarationSyntax<EventDeclarationSyntax>();
            }

            if (declarationSyntax == null)
            {
                declarationSyntax = syntaxNode.GetParentDeclarationSyntax<DelegateDeclarationSyntax>();
            }

            if (declarationSyntax == null)
            {
                declarationSyntax = syntaxNode.GetParentDeclarationSyntax<BaseTypeDeclarationSyntax>();
            }

            if (declarationSyntax == null)
            {
                declarationSyntax = syntaxNode.GetParentDeclarationSyntax<NamespaceDeclarationSyntax>();
            }

            if (declarationSyntax != null)
            {
                var declaredSymbol = _semanticModel.GetDeclaredSymbol(declarationSyntax);
                if (declaredSymbol == null)
                {
                    return syntaxNode.ToString();
                }

                if (declarationSyntax is LocalFunctionStatementSyntax)
                {
                    var parent = GetParentDeclaredType(declarationSyntax.Parent);
                    return $"{parent}.{declaredSymbol}";
                }

                return declaredSymbol.ToString();
            }

            return syntaxNode.ToString();
        }

        public IEntityType GetContainingType(SyntaxNode syntaxNode)
        {
            if (syntaxNode is InvocationExpressionSyntax invocationExpressionSyntax)
            {
                switch (invocationExpressionSyntax.Expression)
                {
                    case MemberAccessExpressionSyntax memberAccessExpressionSyntax:
                    {
                        var entityType = GetFullName(memberAccessExpressionSyntax.Expression);
                        if (string.IsNullOrEmpty(entityType.Name))
                        {
                            entityType = GetFullName(memberAccessExpressionSyntax.Name);
                        }

                        return entityType;
                    }

                    default:
                    {
                        var symbolInfo = _semanticModel.GetSymbolInfo(syntaxNode);
                        if (symbolInfo.Symbol != null)
                        {
                            var containingSymbol = symbolInfo.Symbol.ContainingSymbol;
                            var stringBuilder = new StringBuilder(containingSymbol.ToDisplayString());
                            while (containingSymbol.Kind == SymbolKind.Method)
                            {
                                containingSymbol = containingSymbol.ContainingSymbol;
                                if (containingSymbol.Kind == SymbolKind.Method)
                                {
                                    stringBuilder = new StringBuilder(containingSymbol.ToDisplayString())
                                        .Append('.')
                                        .Append(stringBuilder);
                                }
                            }

                            return _fullNameProvider.CreateEntityTypeModel(stringBuilder.ToString());
                        }

                        var typeInfo = _semanticModel.GetTypeInfo(syntaxNode);
                        if (typeInfo.Type != null)
                        {
                            return _fullNameProvider.CreateEntityTypeModel(typeInfo.Type.ToDisplayString());
                        }
                    }
                        break;
                }
            }

            var baseFieldDeclarationSyntax = syntaxNode.GetParentDeclarationSyntax<BaseFieldDeclarationSyntax>();
            if (baseFieldDeclarationSyntax != null)
            {
                return GetFullName(baseFieldDeclarationSyntax.Declaration.Type);
            }

            var basePropertyDeclarationSyntax = syntaxNode.GetParentDeclarationSyntax<BasePropertyDeclarationSyntax>();
            if (basePropertyDeclarationSyntax != null)
            {
                return GetFullName(basePropertyDeclarationSyntax.Type);
            }

            var variableDeclarationSyntax = syntaxNode.GetParentDeclarationSyntax<VariableDeclarationSyntax>();
            if (variableDeclarationSyntax != null)
            {
                var fullName = GetFullName(variableDeclarationSyntax);
                if (fullName.Name != CSharpConstants.VarIdentifier)
                {
                    return fullName;
                }
            }

            return _fullNameProvider.CreateEntityTypeModel(syntaxNode.ToString());
        }

        private IList<IParameterType> GetParameters(InvocationExpressionSyntax invocationSyntax)
        {
            var methodSymbol = GetMethodSymbol(invocationSyntax);
            if (methodSymbol == null)
            {
                // try to reconstruct the parameters from the method call
                var parameterList = new List<IParameterType>();
                var success = true;

                foreach (var argumentSyntax in invocationSyntax.ArgumentList.Arguments)
                {
                    var parameterSymbolInfo = ModelExtensions.GetSymbolInfo(_semanticModel, argumentSyntax.Expression);
                    if (parameterSymbolInfo.Symbol != null)
                    {
                        parameterList.Add(new ParameterModel
                        {
                            Type = GetFullName(argumentSyntax.Expression)
                        });
                        continue;
                    }

                    if (argumentSyntax.Expression is not LiteralExpressionSyntax literalExpressionSyntax)
                    {
                        success = false;
                        break;
                    }

                    if (literalExpressionSyntax.Token.Value == null)
                    {
                        success = false;
                        break;
                    }

                    parameterList.Add(new ParameterModel
                    {
                        Type = _fullNameProvider.CreateEntityTypeModel(literalExpressionSyntax.Token.Value.GetType()
                            .FullName),
                    });
                }

                return success ? parameterList : new List<IParameterType>();
            }

            return GetParameters(methodSymbol);
        }

        public string SetTypeModifier(string typeString, string modifier)
        {
            if (typeString.StartsWith(CSharpConstants.RefReadonlyIdentifier))
            {
                modifier += $" {CSharpConstants.RefReadonlyIdentifier}";
                modifier = modifier.Trim();
            }
            else if (typeString.StartsWith(CSharpConstants.RefIdentifier))
            {
                modifier += $" {CSharpConstants.RefIdentifier}";
                modifier = modifier.Trim();
            }

            return modifier;
        }

        public int CalculateCyclomaticComplexity(ArrowExpressionClauseSyntax syntax)
        {
            return CalculateCyclomaticComplexityForSyntaxNode(syntax) + 1;
        }

        public int CalculateCyclomaticComplexity(AccessorDeclarationSyntax syntax)
        {
            return CalculateCyclomaticComplexityForSyntaxNode(syntax) + 1;
        }

        public int CalculateCyclomaticComplexity(MemberDeclarationSyntax syntax)
        {
            if (syntax is not BasePropertyDeclarationSyntax basePropertyDeclarationSyntax)
            {
                return CalculateCyclomaticComplexityForSyntaxNode(syntax) + 1;
            }

            var accessorCount = 0;
            if (basePropertyDeclarationSyntax.AccessorList != null)
            {
                accessorCount = basePropertyDeclarationSyntax.AccessorList.Accessors.Count;
            }

            return CalculateCyclomaticComplexityForSyntaxNode(syntax) + accessorCount;
        }

        public int CalculateCyclomaticComplexity(LocalFunctionStatementSyntax syntax)
        {
            return CalculateCyclomaticComplexityForSyntaxNode(syntax) + 1;
        }

        public bool IsAbstractModifier(string modifier)
        {
            return CSharpConstants.AbstractIdentifier == modifier;
        }

        private int CalculateCyclomaticComplexity(ExpressionSyntax conditionExpressionSyntax)
        {
            if (conditionExpressionSyntax == null)
            {
                return 1;
            }

            var logicalOperatorsCount = conditionExpressionSyntax
                .DescendantTokens()
                .Count(token =>
                {
                    var syntaxKind = token.Kind();
                    return syntaxKind is SyntaxKind.AmpersandAmpersandToken or SyntaxKind.BarBarToken or SyntaxKind
                        .AndKeyword or SyntaxKind.OrKeyword;
                });

            return logicalOperatorsCount + 1;
        }

        private int CalculateCyclomaticComplexityForSyntaxNode(SyntaxNode syntax)
        {
            var count = 0;
            foreach (var descendantNode in syntax.DescendantNodes())
            {
                switch (descendantNode)
                {
                    case WhileStatementSyntax whileStatementSyntax:
                    {
                        count += CalculateCyclomaticComplexity(whileStatementSyntax.Condition);
                    }
                        break;
                    case IfStatementSyntax ifStatementSyntax:
                    {
                        count += CalculateCyclomaticComplexity(ifStatementSyntax.Condition);
                    }
                        break;
                    case ForStatementSyntax forStatementSyntax:
                    {
                        count += CalculateCyclomaticComplexity(forStatementSyntax.Condition);
                    }
                        break;
                    case DoStatementSyntax:
                    case CaseSwitchLabelSyntax:
                    case DefaultSwitchLabelSyntax:
                    case CasePatternSwitchLabelSyntax:
                    case ForEachStatementSyntax:
                    case ConditionalExpressionSyntax:
                    case ConditionalAccessExpressionSyntax:
                        count++;
                        break;
                    default:
                    {
                        switch (descendantNode.Kind())
                        {
                            case SyntaxKind.CoalesceAssignmentExpression:
                            case SyntaxKind.CoalesceExpression:
                                count++;
                                break;
                        }
                    }
                        break;
                }
            }

            return count;
        }

        public IEntityType GetAttributeContainingType(AttributeSyntax syntaxNode)
        {
            var accessorDeclarationSyntax = syntaxNode.GetParentDeclarationSyntax<AccessorDeclarationSyntax>();
            if (accessorDeclarationSyntax != null)
            {
                return GetFullName(accessorDeclarationSyntax);
            }

            var propertyDeclarationSyntax = syntaxNode.GetParentDeclarationSyntax<BasePropertyDeclarationSyntax>();
            if (propertyDeclarationSyntax != null)
            {
                return GetFullName(propertyDeclarationSyntax);
            }

            var baseMethodDeclarationSyntax = syntaxNode.GetParentDeclarationSyntax<BaseMethodDeclarationSyntax>();
            if (baseMethodDeclarationSyntax != null)
            {
                return GetFullName(baseMethodDeclarationSyntax);
            }

            var delegateDeclarationSyntax = syntaxNode.GetParentDeclarationSyntax<DelegateDeclarationSyntax>();
            if (delegateDeclarationSyntax != null)
            {
                return GetFullName(delegateDeclarationSyntax);
            }

            var parentDeclarationSyntax = syntaxNode.GetParentDeclarationSyntax<BaseTypeDeclarationSyntax>();
            if (parentDeclarationSyntax != null)
            {
                return GetFullName(parentDeclarationSyntax);
            }

            return _fullNameProvider.CreateEntityTypeModel(syntaxNode.ToString());
        }

        public IEnumerable<IParameterType> GetParameters(AttributeSyntax attributeSyntax)
        {
            var symbolInfo = _semanticModel.GetSymbolInfo(attributeSyntax);
            if (symbolInfo.Symbol == null)
            {
                // try to reconstruct parameters
                var parameterTypes = new List<IParameterType>();

                if (attributeSyntax.ArgumentList == null)
                {
                    return parameterTypes;
                }

                foreach (var argumentSyntax in attributeSyntax.ArgumentList.Arguments)
                {
                    var parameterSymbolInfo = _semanticModel.GetSymbolInfo(argumentSyntax.Expression);
                    var parameterType = CSharpConstants.SystemObject;
                    if (parameterSymbolInfo.Symbol != null)
                    {
                        parameterTypes.Add(new ParameterModel
                        {
                            Type = GetFullName(argumentSyntax.Expression)
                        });
                        continue;
                    }

                    if (argumentSyntax.Expression is LiteralExpressionSyntax literalExpressionSyntax)
                    {
                        if (literalExpressionSyntax.Token.Value != null)
                        {
                            parameterType = literalExpressionSyntax.Token.Value.GetType().FullName;
                        }
                    }

                    parameterTypes.Add(new ParameterModel
                    {
                        Type = _fullNameProvider.CreateEntityTypeModel(parameterType)
                    });
                }

                return parameterTypes;
            }

            if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
            {
                return GetParameters(methodSymbol);
            }

            return new List<IParameterType>();
        }

        public string GetAttributeTarget(AttributeSyntax syntaxNode)
        {
            var attributeListSyntax = syntaxNode.GetParentDeclarationSyntax<AttributeListSyntax>();

            if (attributeListSyntax?.Target == null)
            {
                return "";
            }

            return attributeListSyntax.Target.Identifier.ToString();
        }

        public AccessedField GetAccessField(ExpressionSyntax identifierNameSyntax)
        {
            if (identifierNameSyntax == null)
            {
                return null;
            }

            var symbolInfo = _semanticModel.GetSymbolInfo(identifierNameSyntax);

            if (symbolInfo.Symbol is IFieldSymbol fieldSymbol)
            {
                return new AccessedField
                {
                    Name = fieldSymbol.Name,
                    ContainingTypeName = fieldSymbol.ContainingType.ToString(),
                    Type = GetAccessType(identifierNameSyntax)
                };
            }

            if (symbolInfo.Symbol is IPropertySymbol propertySymbol)
            {
                return new AccessedField
                {
                    Name = propertySymbol.Name,
                    ContainingTypeName = propertySymbol.ContainingType.ToString(),
                    Type = GetAccessType(identifierNameSyntax)
                };
            }

            if (identifierNameSyntax is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
            {
                if (memberAccessExpressionSyntax.Parent is InvocationExpressionSyntax)
                {
                    return null;
                }

                return new AccessedField
                {
                    Name = memberAccessExpressionSyntax.Name.ToString(),
                    ContainingTypeName = memberAccessExpressionSyntax.Expression.ToString(),
                    Type = GetAccessType(memberAccessExpressionSyntax)
                };
            }

            return null;

            AccessedField.AccessType GetAccessType(SyntaxNode syntax)
            {
                if (syntax?.Parent is AssignmentExpressionSyntax)
                {
                    return AccessedField.AccessType.Setter;
                }

                return AccessedField.AccessType.Getter;
            }
        }
    }
}

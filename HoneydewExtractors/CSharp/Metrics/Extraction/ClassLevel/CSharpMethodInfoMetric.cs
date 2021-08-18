using System.Collections.Generic;
using System.Linq;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels;
using HoneydewModels.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel
{
    public class CSharpMethodInfoMetric : HoneydewCSharpSyntaxWalker,
        IExtractionMetric<CSharpSyntacticModel, CSharpSemanticModel, CSharpSyntaxNode>
    {
        public CSharpSyntacticModel HoneydewSyntacticModel { get; set; }
        public CSharpSemanticModel HoneydewSemanticModel { get; set; }

        public CSharpMethodInfoDataMetric DataMetric { get; set; } = new();

        private string _containingClassName = "";
        private string _baseTypeName = CSharpConstants.ObjectIdentifier;
        private bool _isInterface;

        private readonly CSharpLinesOfCodeCounter _linesOfCodeCounter = new();

        public ExtractionMetricType GetMetricType()
        {
            return ExtractionMetricType.ClassLevel;
        }

        public override IMetricValue GetMetric()
        {
            return new MetricValue<CSharpMethodInfoDataMetric>(DataMetric);
        }

        public override string PrettyPrint()
        {
            return "Methods Info";
        }

        public override void VisitDelegateDeclaration(DelegateDeclarationSyntax syntax)
        {
            _containingClassName = HoneydewSemanticModel.GetFullName(syntax);

            var returnType = HoneydewSemanticModel.GetFullName(syntax.ReturnType);

            var methodModel = new MethodModel
            {
                Name = _containingClassName,
                ReturnType = new ReturnTypeModel
                {
                    Type = returnType
                },
                ContainingClassName = _containingClassName,
                Modifier = "",
                AccessModifier = "",
                CyclomaticComplexity = 0
            };

            ExtractInfoAboutParameters(syntax.ParameterList, methodModel);

            DataMetric.MethodInfos.Add(methodModel);
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
            _containingClassName = HoneydewSemanticModel.GetFullName(node);
            _baseTypeName = HoneydewSemanticModel.GetBaseClassName(node);

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
                IsConstructor = true,
                Loc = _linesOfCodeCounter.Count(syntax.ToString()),
                CyclomaticComplexity = HoneydewSyntacticModel.CalculateCyclomaticComplexity(syntax)
            };

            ExtractInfoAboutParameters(syntax.ParameterList, methodModel);

            ExtractInfoAboutConstructorCalls(syntax, methodModel);

            ExtractInfoAboutCalledMethods(syntax, methodModel);

            DataMetric.ConstructorInfos.Add(methodModel);
        }

        private void AddMethodInfo(MethodDeclarationSyntax syntax)
        {
            GetModifiersForNode(syntax, out var accessModifier, out var modifier);

            var returnType = HoneydewSemanticModel.GetFullName(syntax.ReturnType);

            var returnTypeModifier = HoneydewSyntacticModel.SetTypeModifier(syntax.ReturnType.ToString(), "");

            var methodModel = new MethodModel
            {
                Name = syntax.Identifier.ToString(),
                ReturnType = new ReturnTypeModel
                {
                    Type = returnType,
                    Modifier = returnTypeModifier
                },
                ContainingClassName = _containingClassName,
                Modifier = modifier,
                AccessModifier = accessModifier,
                Loc = _linesOfCodeCounter.Count(syntax.ToString()),
                CyclomaticComplexity = HoneydewSyntacticModel.CalculateCyclomaticComplexity(syntax)
            };

            ExtractInfoAboutParameters(syntax.ParameterList, methodModel);

            ExtractInfoAboutCalledMethods(syntax, methodModel);

            DataMetric.MethodInfos.Add(methodModel);
        }

        private void ExtractInfoAboutParameters(BaseParameterListSyntax parameterList, MethodModel methodModel)
        {
            foreach (var parameter in parameterList.Parameters)
            {
                var parameterType = HoneydewSemanticModel.GetFullName(parameter.Type);

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
                var methodCallModel =
                    HoneydewSemanticModel.GetMethodCallModel(invocationExpressionSyntax, _baseTypeName);
                if (methodCallModel != null)
                {
                    methodModel.CalledMethods.Add(methodCallModel);
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

            var methodSymbol = HoneydewSemanticModel.GetMethodSymbol(syntax.Initializer);

            if (methodSymbol != null)
            {
                parameterModels = HoneydewSemanticModel.GetParameterTypes(methodSymbol);
                methodName = methodSymbol.ContainingType.Name;
            }

            methodModel.CalledMethods.Add(new MethodCallModel
            {
                MethodName = methodName,
                ContainingClassName = containingClassName,
                ParameterTypes = parameterModels
            });
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

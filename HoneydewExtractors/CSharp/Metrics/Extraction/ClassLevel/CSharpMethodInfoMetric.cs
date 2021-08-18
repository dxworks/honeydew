using System.Collections.Generic;
using System.Linq;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
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
        private string _baseName = CSharpConstants.ObjectIdentifier;
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
                    Name = returnType
                },
                ContainingTypeName = _containingClassName,
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
            _baseName = HoneydewSemanticModel.GetBaseClassName(node);

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

            var constructorModel = new ConstructorModel
            {
                Name = syntax.Identifier.ToString(),
                ContainingTypeName = _containingClassName,
                Modifier = modifier,
                AccessModifier = accessModifier,
                Loc = _linesOfCodeCounter.Count(syntax.ToString()),
                CyclomaticComplexity = HoneydewSyntacticModel.CalculateCyclomaticComplexity(syntax)
            };

            ExtractInfoAboutParameters(syntax.ParameterList, constructorModel);

            ExtractInfoAboutConstructorCalls(syntax, constructorModel);

            ExtractInfoAboutCalledMethods(syntax, constructorModel);

            DataMetric.ConstructorInfos.Add(constructorModel);
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
                    Name = returnType,
                    Modifier = returnTypeModifier
                },
                ContainingTypeName = _containingClassName,
                Modifier = modifier,
                AccessModifier = accessModifier,
                Loc = _linesOfCodeCounter.Count(syntax.ToString()),
                CyclomaticComplexity = HoneydewSyntacticModel.CalculateCyclomaticComplexity(syntax)
            };

            ExtractInfoAboutParameters(syntax.ParameterList, methodModel);

            ExtractInfoAboutCalledMethods(syntax, methodModel);

            DataMetric.MethodInfos.Add(methodModel);
        }

        private void ExtractInfoAboutParameters(BaseParameterListSyntax parameterList, IMethodSignatureType methodModel)
        {
            foreach (var parameter in parameterList.Parameters)
            {
                var parameterType = HoneydewSemanticModel.GetFullName(parameter.Type);

                methodModel.ParameterTypes.Add(new ParameterModel
                {
                    Name = parameterType,
                    Modifier = parameter.Modifiers.ToString(),
                    DefaultValue = parameter.Default?.Value.ToString()
                });
            }
        }

        private void ExtractInfoAboutCalledMethods(BaseMethodDeclarationSyntax syntax, ICallingMethodsType methodModel)
        {
            if (syntax.Body == null)
            {
                return;
            }

            foreach (var invocationExpressionSyntax in syntax.Body.DescendantNodes()
                .OfType<InvocationExpressionSyntax>())
            {
                var methodCallModel =
                    HoneydewSemanticModel.GetMethodCallModel(invocationExpressionSyntax, _baseName);
                if (methodCallModel != null)
                {
                    methodModel.CalledMethods.Add(methodCallModel);
                }
            }
        }

        private void ExtractInfoAboutConstructorCalls(ConstructorDeclarationSyntax syntax,
            ICallingMethodsType methodModel)
        {
            if (syntax.Initializer == null)
            {
                return;
            }

            var containingClassName = _containingClassName;

            var methodName = syntax.Identifier.ToString();
            if (syntax.Initializer.ThisOrBaseKeyword.ValueText == "base")
            {
                containingClassName = _baseName;
                methodName = _baseName;
            }

            IList<IParameterType> parameterModels = new List<IParameterType>();

            var methodSymbol = HoneydewSemanticModel.GetMethodSymbol(syntax.Initializer);

            if (methodSymbol != null)
            {
                parameterModels = HoneydewSemanticModel.GetParameters(methodSymbol);
                methodName = methodSymbol.ContainingType.Name;
            }

            methodModel.CalledMethods.Add(new MethodCallModel
            {
                Name = methodName,
                ContainingTypeName = containingClassName,
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

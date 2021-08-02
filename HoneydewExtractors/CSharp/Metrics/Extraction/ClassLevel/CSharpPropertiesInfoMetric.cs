using System.Collections.Generic;
using System.Linq;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels;
using HoneydewModels.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel
{
    public class CSharpPropertiesInfoMetric : HoneydewCSharpSyntaxWalker,
        IExtractionMetric<CSharpSyntacticModel, CSharpSemanticModel, CSharpSyntaxNode>
    {
        public CSharpSyntacticModel HoneydewSyntacticModel { get; set; }
        public CSharpSemanticModel HoneydewSemanticModel { get; set; }

        private readonly CSharpLinesOfCodeCounter _linesOfCodeCounter = new();

        public IList<PropertyModel> PropertyInfos { get; } = new List<PropertyModel>();

        public ExtractionMetricType GetMetricType()
        {
            return ExtractionMetricType.ClassLevel;
        }

        public override IMetricValue GetMetric()
        {
            return new MetricValue<IList<PropertyModel>>(PropertyInfos);
        }

        public override string PrettyPrint()
        {
            return "Properties Info";
        }

        public override void VisitEventDeclaration(EventDeclarationSyntax node)
        {
            AddPropertyInfo(node, node.Identifier.ToString(), true);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            AddPropertyInfo(node, node.Identifier.ToString(), false);
        }

        private void AddPropertyInfo(BasePropertyDeclarationSyntax node, string name, bool isEvent)
        {
            var allModifiers = node.Modifiers.ToString();
            var accessModifier = CSharpConstants.DefaultFieldAccessModifier;
            var modifier = allModifiers;

            var containingClass = "";
            if (node.Parent is BaseTypeDeclarationSyntax classDeclarationSyntax)
            {
                containingClass = HoneydewSemanticModel.GetFullName(classDeclarationSyntax);
            }

            CSharpConstants.SetModifiers(allModifiers, ref accessModifier, ref modifier);

            var typeName = HoneydewSemanticModel.GetFullName(node.Type);

            var calledMethods = new List<MethodCallModel>();
            foreach (var invocationExpressionSyntax in node.DescendantNodes()
                .OfType<InvocationExpressionSyntax>())
            {
                var methodCallModel = HoneydewSemanticModel.GetMethodCallModel(invocationExpressionSyntax,
                    containingClass);
                if (methodCallModel != null)
                {
                    calledMethods.Add(methodCallModel);
                }
            }

            var accessors = new List<string>();

            if (node.AccessorList != null)
            {
                foreach (var accessor in node.AccessorList.Accessors)
                {
                    var accessorModifiers = accessor.Modifiers.ToString();
                    var accessorKeyword = accessor.Keyword.ToString();
                    
                    if (string.IsNullOrEmpty(accessorModifiers))
                    {
                        accessors.Add(accessorKeyword);
                    }
                    else
                    {
                        accessors.Add(accessorModifiers + " " + accessorKeyword);
                    }
                }
            }

            PropertyInfos.Add(new PropertyModel
            {
                AccessModifier = accessModifier,
                Modifier = modifier,
                IsEvent = isEvent,
                Type = typeName,
                Name = name,
                ContainingClassName = containingClass,
                CalledMethods = calledMethods,
                Accessors = accessors,
                Loc = _linesOfCodeCounter.Count(node.ToString())
            });
        }
    }
}

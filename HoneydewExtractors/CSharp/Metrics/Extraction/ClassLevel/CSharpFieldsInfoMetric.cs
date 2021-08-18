using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel
{
    public class CSharpFieldsInfoMetric : HoneydewCSharpSyntaxWalker,
        IExtractionMetric<CSharpSyntacticModel, CSharpSemanticModel, CSharpSyntaxNode>
    {
        public CSharpSyntacticModel HoneydewSyntacticModel { get; set; }
        public CSharpSemanticModel HoneydewSemanticModel { get; set; }
        
        public IList<IFieldType> FieldInfos { get; } = new List<IFieldType>();

        public ExtractionMetricType GetMetricType()
        {
            return ExtractionMetricType.ClassLevel;
        }

        public override IMetricValue GetMetric()
        {
            return new MetricValue<IList<IFieldType>>(FieldInfos);
        }

        public override string PrettyPrint()
        {
            return "Fields Info";
        }

        public override void VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
        {
            AddFieldInfo(node, true);
        }

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            AddFieldInfo(node, false);
        }

        private void AddFieldInfo(BaseFieldDeclarationSyntax node, bool isEvent)
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

            var typeName = HoneydewSemanticModel.GetFullName(node.Declaration);

            foreach (var variable in node.Declaration.Variables)
            {
                FieldInfos.Add(new FieldModel
                {
                    AccessModifier = accessModifier,
                    Modifier = modifier,
                    IsEvent = isEvent,
                    Type = typeName,
                    Name = variable.Identifier.ToString(),
                    ContainingTypeName = containingClass
                });
            }
        }
    }
}

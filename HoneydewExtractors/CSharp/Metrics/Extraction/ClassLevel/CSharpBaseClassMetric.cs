using HoneydewExtractors.Core.Metrics;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewModels;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel
{
    /// <summary>
    /// Retrieves The Base class and the implemented interfaces 
    /// </summary>
    public class CSharpBaseClassMetric : HoneydewCSharpSyntaxWalker,
        IExtractionMetric<CSharpSyntacticModel, CSharpSemanticModel, CSharpSyntaxNode>
    {
        public CSharpSyntacticModel HoneydewSyntacticModel { get; set; }
        public CSharpSemanticModel HoneydewSemanticModel { get; set; }

        public CSharpInheritanceMetric InheritanceMetric { get; set; } = new();

        public ExtractionMetricType GetMetricType()
        {
            return ExtractionMetricType.ClassLevel;
        }

        public override IMetricValue GetMetric()
        {
            return new MetricValue<CSharpInheritanceMetric>(InheritanceMetric);
        }

        public override string PrettyPrint()
        {
            return "Inherits Class";
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            InheritanceMetric.BaseClassName = null;
            InheritanceMetric.Interfaces = HoneydewSemanticModel.GetBaseInterfaces(node);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            InheritanceMetric.BaseClassName = HoneydewSemanticModel.GetBaseClassName(node);
            InheritanceMetric.Interfaces = HoneydewSemanticModel.GetBaseInterfaces(node);
        }
    }
}

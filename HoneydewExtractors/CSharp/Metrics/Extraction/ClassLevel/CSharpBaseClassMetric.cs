using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewModels;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
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

        public IList<IBaseType> BaseTypes { get; set; } = new List<IBaseType>();

        public ExtractionMetricType GetMetricType()
        {
            return ExtractionMetricType.ClassLevel;
        }

        public override IMetricValue GetMetric()
        {
            return new MetricValue<IList<IBaseType>>(BaseTypes);
        }

        public override string PrettyPrint()
        {
            return "Inherits Class";
        }

        public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node)
        {
            BaseTypes.Add(new BaseTypeModel
            {
                Name = "System.Delegate",
                ClassType = "class"
            });
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            foreach (var baseInterface in HoneydewSemanticModel.GetBaseInterfaces(node))
            {
                BaseTypes.Add(new BaseTypeModel
                {
                    Name = baseInterface,
                    ClassType = "interface"
                });
            }
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            BaseTypes.Add(new BaseTypeModel
            {
                Name = HoneydewSemanticModel.GetBaseClassName(node),
                ClassType = "class"
            });

            foreach (var baseInterface in HoneydewSemanticModel.GetBaseInterfaces(node))
            {
                BaseTypes.Add(new BaseTypeModel
                {
                    Name = baseInterface,
                    ClassType = "interface"
                });
            }
        }
    }
}

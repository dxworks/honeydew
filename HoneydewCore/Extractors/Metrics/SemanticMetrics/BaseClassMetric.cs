using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewCore.Extractors.Metrics.SemanticMetrics
{
    /// <summary>
    /// Retrieves The Base class and the interfaces 
    /// </summary>
    public class BaseClassMetric : CSharpMetricExtractor
    {
        private InheritanceMetric _inheritanceMetric;

        public override MetricType GetMetricType()
        {
            return MetricType.Semantic;
        }

        public override IMetric GetMetric()
        {
            return new Metric<InheritanceMetric>(_inheritanceMetric);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            _inheritanceMetric = new InheritanceMetric();

            var declaredSymbol = SemanticModel.GetDeclaredSymbol(node);

            if (declaredSymbol is not ITypeSymbol typeSymbol) return;

            if (typeSymbol.BaseType == null)
            {
                _inheritanceMetric.BaseClassName = "Object";
                return;
            }

            _inheritanceMetric.BaseClassName = typeSymbol.BaseType.Name;

            if (typeSymbol.BaseType.Constructors.IsEmpty)
            {
                _inheritanceMetric.Interfaces.Add(typeSymbol.BaseType?.ToString());
                _inheritanceMetric.BaseClassName = "Object";
            }


            foreach (var i in typeSymbol.Interfaces)
            {
                _inheritanceMetric.Interfaces.Add(i.ToString());
            }
        }
    }
}
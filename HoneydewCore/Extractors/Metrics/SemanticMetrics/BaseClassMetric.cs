using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewCore.Extractors.Metrics.SemanticMetrics
{
    /// <summary>
    /// Retrieves The Base class and the implemented interfaces 
    /// </summary>
    public class BaseClassMetric : CSharpMetricExtractor, ISemanticMetric
    {
        public InheritanceMetric InheritanceMetric { get; set; } = new InheritanceMetric();

        public override IMetric GetMetric()
        {
            return new Metric<InheritanceMetric>(InheritanceMetric);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var declaredSymbol = SemanticModel.GetDeclaredSymbol(node);

            if (declaredSymbol is not ITypeSymbol typeSymbol) return;

            if (typeSymbol.BaseType == null)
            {
                InheritanceMetric.BaseClassName = "Object";
                return;
            }

            InheritanceMetric.BaseClassName = typeSymbol.BaseType.Name;

            if (typeSymbol.BaseType.Constructors.IsEmpty)
            {
                InheritanceMetric.Interfaces.Add(typeSymbol.BaseType?.ToString());
                InheritanceMetric.BaseClassName = "Object";
            }


            foreach (var i in typeSymbol.Interfaces)
            {
                InheritanceMetric.Interfaces.Add(i.ToString());
            }
        }
    }
}
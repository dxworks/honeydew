using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewCore.Extractors.Metrics.SemanticMetrics
{
    /// <summary>
    /// Retrieves The Base class and the implemented interfaces 
    /// </summary>
    public class BaseClassMetric : CSharpMetricExtractor, ISemanticMetric
    {
        public InheritanceMetric InheritanceMetric { get; set; } = new();

        public override IMetric GetMetric()
        {
            return new Metric<InheritanceMetric>(InheritanceMetric);
        }

        public override string PrettyPrint()
        {
            return "Inherits Class";
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            var declaredSymbol = SemanticModel.GetDeclaredSymbol(node);

            if (declaredSymbol is not ITypeSymbol typeSymbol) return;
            InheritanceMetric.BaseClassName = null;

            foreach (var interfaceSymbol in typeSymbol.Interfaces)
            {
                InheritanceMetric.Interfaces.Add(interfaceSymbol.ToString());
            }
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var declaredSymbol = SemanticModel.GetDeclaredSymbol(node);

            if (declaredSymbol is not ITypeSymbol typeSymbol) return;

            if (typeSymbol.BaseType == null)
            {
                InheritanceMetric.BaseClassName = "object";
                return;
            }

            InheritanceMetric.BaseClassName = typeSymbol.BaseType.ToString();

            if (typeSymbol.BaseType.Constructors.IsEmpty)
            {
                InheritanceMetric.Interfaces.Add(typeSymbol.BaseType?.ToString());
                InheritanceMetric.BaseClassName = "object";
            }

            foreach (var interfaceSymbol in typeSymbol.Interfaces)
            {
                InheritanceMetric.Interfaces.Add(interfaceSymbol.ToString());
            }
        }
    }
}
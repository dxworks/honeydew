using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Class
{
    public class IsAbstractClassVisitor : ExtractionVisitor<CSharpSyntacticModel, CSharpSemanticModel>,
        ICSharpClassVisitor
    {
        public IPropertyMembersClassType Visit(BaseTypeDeclarationSyntax syntaxNode,
            IPropertyMembersClassType modelType)
        {
            var isAbstract = false;

            if (syntaxNode is InterfaceDeclarationSyntax)
            {
                isAbstract = true;
            }
            else
            {
                foreach (var m in syntaxNode.Modifiers)
                {
                    if (!InheritedSyntacticModel.IsAbstractModifier(m.ValueText))
                    {
                        continue;
                    }

                    isAbstract = true;

                    break;
                }
            }

            modelType.Metrics.Add(new MetricModel
            {
                Value = isAbstract,
                ValueType = isAbstract.GetType().ToString(),
                ExtractorName = GetType().ToString()
            });

            return modelType;
        }
    }
}

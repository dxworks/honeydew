using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.CSharp.Metrics.Extraction;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Class
{
    public class IsAbstractClassVisitor : IRequireCSharpExtractionHelperMethodsVisitor,
        ICSharpClassVisitor
    {
        public CSharpExtractionHelperMethods CSharpHelperMethods { get; set; }

        public void Accept(IVisitor visitor)
        {
        }

        public IClassType Visit(BaseTypeDeclarationSyntax syntaxNode, IClassType modelType)
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
                    if (!CSharpHelperMethods.IsAbstractModifier(m.ValueText))
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

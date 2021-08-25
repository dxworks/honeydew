using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Attributes;
using HoneydewExtractors.CSharp.Metrics.Extraction;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Attribute
{
    public class AttributeInfoVisitor : IRequireCSharpExtractionHelperMethodsVisitor, ICSharpAttributeVisitor
    {
        public CSharpExtractionHelperMethods CSharpHelperMethods { get; set; }

        public void Accept(IVisitor visitor)
        {
        }

        public IAttributeType Visit(AttributeSyntax syntaxNode, IAttributeType modelType)
        {
            modelType.Name = CSharpHelperMethods.GetFullName(syntaxNode);
            modelType.ContainingTypeName = CSharpHelperMethods.GetAttributeContainingType(syntaxNode);

            foreach (var parameterType in CSharpHelperMethods.GetParameters(syntaxNode))
            {
                modelType.ParameterTypes.Add(parameterType);
            }

            return modelType;
        }
    }
}

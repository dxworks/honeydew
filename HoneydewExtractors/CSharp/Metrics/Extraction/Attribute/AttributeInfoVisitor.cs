using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Attributes;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Attribute
{
    public class AttributeInfoVisitor : IRequireCSharpExtractionHelperMethodsVisitor, ICSharpAttributeVisitor
    {
        public CSharpExtractionHelperMethods CSharpHelperMethods { get; set; }

        public void Accept(IVisitor visitor)
        {
        }

        public IAttributeType Visit(AttributeSyntax syntaxNode, IAttributeType modelType)
        {
            modelType.Name = CSharpHelperMethods.GetFullName(syntaxNode).Name;
            modelType.ContainingTypeName = CSharpHelperMethods.GetAttributeContainingType(syntaxNode).Name;
            var attributeTarget = CSharpHelperMethods.GetAttributeTarget(syntaxNode);
            if (!string.IsNullOrEmpty(attributeTarget))
            {
                modelType.Target = attributeTarget;
            }

            foreach (var parameterType in CSharpHelperMethods.GetParameters(syntaxNode))
            {
                modelType.ParameterTypes.Add(parameterType);
            }

            return modelType;
        }
    }
}

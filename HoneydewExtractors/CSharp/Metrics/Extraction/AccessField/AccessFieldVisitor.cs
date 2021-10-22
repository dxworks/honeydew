using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.AccessedFields;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.AccessField
{
    public class AccessFieldVisitor : IRequireCSharpExtractionHelperMethodsVisitor,
        ICSharpAccessedFieldsVisitor
    {
        public CSharpExtractionHelperMethods CSharpHelperMethods { get; set; }

        public void Accept(IVisitor visitor)
        {
        }

        public AccessedField Visit(ExpressionSyntax syntaxNode, AccessedField modelType)
        {
            return CSharpHelperMethods.GetAccessField(syntaxNode);
        }
    }
}

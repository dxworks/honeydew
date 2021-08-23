using HoneydewExtractors.CSharp.Metrics.Extraction;

namespace HoneydewExtractors.Core.Metrics.Visitors
{
    public interface IRequireCSharpExtractionHelperMethodsVisitor : ITypeVisitor
    {
        public CSharpExtractionHelperMethods CSharpHelperMethods { get; set; }
    }
}

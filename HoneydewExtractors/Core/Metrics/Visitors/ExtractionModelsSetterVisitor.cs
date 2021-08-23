using HoneydewExtractors.CSharp.Metrics.Extraction;

namespace HoneydewExtractors.Core.Metrics.Visitors
{
    public class ExtractionModelsSetterVisitor : IVisitor
    {
        private readonly CSharpExtractionHelperMethods _extractionHelperMethods;

        public ExtractionModelsSetterVisitor(CSharpExtractionHelperMethods extractionHelperMethods)
        {
            _extractionHelperMethods = extractionHelperMethods;
        }

        public void Visit(ITypeVisitor visitor)
        {
            if (visitor is IRequireCSharpExtractionHelperMethodsVisitor
                requireExtractionHelperMethodsVisitor)
            {
                requireExtractionHelperMethodsVisitor.CSharpHelperMethods = _extractionHelperMethods;
            }

            visitor.Accept(this);
        }
    }
}

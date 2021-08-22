using HoneydewExtractors.CSharp.Metrics;

namespace HoneydewExtractors.Core.Metrics.Visitors
{
    public class ExtractionModelsSetterVisitor : IVisitor
    {
        private readonly CSharpSyntacticModel _syntacticModel;
        private readonly CSharpSemanticModel _semanticModel;

        public ExtractionModelsSetterVisitor(CSharpSyntacticModel syntacticModel, CSharpSemanticModel semanticModel)
        {
            _syntacticModel = syntacticModel;
            _semanticModel = semanticModel;
        }

        public void Visit(ITypeVisitor visitor)
        {
            if (visitor is ExtractionVisitor<CSharpSyntacticModel, CSharpSemanticModel>
                extractionVisitor)
            {
                extractionVisitor.InheritedSyntacticModel = _syntacticModel;
                extractionVisitor.InheritedSemanticModel = _semanticModel;
            }

            visitor.Accept(this);
        }
    }
}

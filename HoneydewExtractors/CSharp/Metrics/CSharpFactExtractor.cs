using System.Linq;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.Core.Metrics.Extraction.ModelCreators;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.CompilationUnit;
using HoneydewModels.CSharp;
using HoneydewModels.Types;

namespace HoneydewExtractors.CSharp.Metrics
{
    public class CSharpFactExtractor : IFactExtractor
    {
        private readonly CSharpSyntacticModelCreator _syntacticModelCreator;
        private readonly CSharpSemanticModelCreator _semanticModelCreator;
        private readonly VisitorList _visitorList;

        public CSharpFactExtractor(CSharpSyntacticModelCreator syntacticModelCreator,
            CSharpSemanticModelCreator semanticModelCreator, VisitorList visitorList)
        {
            _syntacticModelCreator = syntacticModelCreator;
            _semanticModelCreator = semanticModelCreator;
            _visitorList = visitorList;
        }

        public ICompilationUnitType Extract(string text)
        {
            var syntacticModel = _syntacticModelCreator.Create(text);
            var semanticModel = _semanticModelCreator.Create(syntacticModel);

            foreach (var extractionVisitor in
                _visitorList.OfType<IExtractionVisitor<CSharpSyntacticModel, CSharpSemanticModel>>())
            {
                extractionVisitor.SetSyntacticModel(syntacticModel);
                extractionVisitor.SetSemanticModel(semanticModel);
            }

            var compilationUnitType = new CompilationUnitModel();

            var compilationUnitModelCreator =
                new CSharpCompilationUnitModelCreator(_visitorList.OfType<ICSharpCompilationUnitVisitor>().ToList());

            return compilationUnitModelCreator.Create(syntacticModel.CompilationUnitSyntax, compilationUnitType);
        }
    }
}

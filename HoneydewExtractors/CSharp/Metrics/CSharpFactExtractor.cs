using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction;
using HoneydewModels.CSharp;
using HoneydewModels.Types;

namespace HoneydewExtractors.CSharp.Metrics
{
    public class CSharpFactExtractor : IFactExtractor
    {
        private readonly CSharpSyntacticModelCreator _syntacticModelCreator;
        private readonly CSharpSemanticModelCreator _semanticModelCreator;
        private readonly ICompositeVisitor _compositeVisitor;

        public CSharpFactExtractor(CSharpSyntacticModelCreator syntacticModelCreator,
            CSharpSemanticModelCreator semanticModelCreator, ICompositeVisitor compositeVisitor)
        {
            _syntacticModelCreator = syntacticModelCreator;
            _semanticModelCreator = semanticModelCreator;
            _compositeVisitor = compositeVisitor;
        }

        public ICompilationUnitType Extract(string text)
        {
            var syntacticModel = _syntacticModelCreator.Create(text);
            var semanticModel = _semanticModelCreator.Create(syntacticModel);


            IVisitor extractionModelsSetterVisitor =
                new ExtractionModelsSetterVisitor(new CSharpExtractionHelperMethods(semanticModel));
            
            _compositeVisitor.Accept(extractionModelsSetterVisitor);

            ICompilationUnitType compilationUnitModel = new CompilationUnitModel();

            foreach (var visitor in _compositeVisitor.GetContainedVisitors())
            {
                if (visitor is ICSharpCompilationUnitVisitor compilationUnitVisitor)
                {
                    compilationUnitModel =
                        compilationUnitVisitor.Visit(syntacticModel.CompilationUnitSyntax, compilationUnitModel);
                }
            }

            return compilationUnitModel;
        }
    }
}

using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.CSharp.Metrics;

namespace HoneydewExtractors.Core
{
    public class FactExtractorCreator : IFactExtractorCreator
    {
        private readonly ICompositeVisitor _compositeVisitor;
        private readonly ICompilationMaker _compilationMaker;

        private CSharpFactExtractor _cSharpFactExtractor;

        public FactExtractorCreator(ICompositeVisitor compositeVisitor, ICompilationMaker compilationMaker)
        {
            _compositeVisitor = compositeVisitor;
            _compilationMaker = compilationMaker;
        }

        public IFactExtractor Create(string language)
        {
            switch (language)
            {
                case "C#":

                    return _cSharpFactExtractor ??= new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                        new CSharpSemanticModelCreator(_compilationMaker), _compositeVisitor);

                case "Visual Basic":
                    break;
            }

            return null;
        }
    }
}

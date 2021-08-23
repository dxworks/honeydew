using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.CSharp.Metrics;

namespace HoneydewExtractors.Core
{
    public class FactExtractorCreator : IFactExtractorCreator
    {
        private readonly ICompositeVisitor _compositeVisitor;
        private CSharpFactExtractor _cSharpFactExtractor;

        public FactExtractorCreator(ICompositeVisitor compositeVisitor)
        {
            _compositeVisitor = compositeVisitor;
        }

        public IFactExtractor Create(string language)
        {
            switch (language)
            {
                case "C#":

                    return _cSharpFactExtractor ??= new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                        new CSharpSemanticModelCreator(new CSharpCompilationMaker()), _compositeVisitor);

                case "Visual Basic":
                    break;
            }

            return null;
        }
    }
}

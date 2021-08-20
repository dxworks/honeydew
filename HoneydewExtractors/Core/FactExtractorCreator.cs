using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.CSharp.Metrics;

namespace HoneydewExtractors.Core
{
    public class FactExtractorCreator : IFactExtractorCreator
    {
        private readonly IVisitorList _visitorList;
        private CSharpFactExtractor _cSharpFactExtractor;

        public FactExtractorCreator(IVisitorList visitorList)
        {
            _visitorList = visitorList;
        }

        public IFactExtractor Create(string language)
        {
            switch (language)
            {
                case "C#":

                    return _cSharpFactExtractor ??= new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                        new CSharpSemanticModelCreator(new CSharpCompilationMaker()), _visitorList);

                case "Visual Basic":
                    break;
            }

            return null;
        }
    }
}

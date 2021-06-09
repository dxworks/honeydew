using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Extractors;
using HoneydewCore.Models;

namespace HoneydewCore.IO.Readers.Strategies
{
    public class DirectSolutionLoading : ISolutionLoadingStrategy
    {
        public IList<CompilationUnitModel> Extract(string fileContent, IList<IFactExtractor> extractors)
        {
            extractors ??= new List<IFactExtractor>();

            return extractors.Select(extractor => extractor.Extract(fileContent)).ToList();
        }
    }
}
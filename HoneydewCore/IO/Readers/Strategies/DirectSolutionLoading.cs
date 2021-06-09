using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Extractors;
using HoneydewCore.Extractors.Models;

namespace HoneydewCore.IO.Readers.Strategies
{
    public class DirectSolutionLoading : ISolutionLoadingStrategy
    {
        public IList<ClassModel> Load(string fileContent, IList<IFactExtractor> extractors)
        {
            extractors ??= new List<IFactExtractor>();

            return extractors.SelectMany(extractor => extractor.Extract(fileContent)).ToList();
        }
    }
}
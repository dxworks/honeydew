using System.Collections.Generic;
using HoneydewCore.Extractors;
using HoneydewCore.Models;

namespace HoneydewCore.IO.Readers.Strategies
{
    public interface ISolutionLoadingStrategy
    {
        IList<ClassModel> Load(string fileContent, IList<IFactExtractor> extractors);
    }
}
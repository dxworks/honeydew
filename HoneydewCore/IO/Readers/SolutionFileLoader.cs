using System.Collections.Generic;
using HoneydewCore.Extractors;
using HoneydewCore.IO.Readers.Strategies;
using HoneydewCore.Models;

namespace HoneydewCore.IO.Readers
{
    public class SolutionFileLoader : ISolutionLoader
    {
        private readonly IList<IFactExtractor> _extractors;

        private readonly ISolutionProvider _solutionProvider;
        private readonly ISolutionLoadingStrategy _solutionLoadingStrategy;

        public SolutionFileLoader(IList<IFactExtractor> extractors, ISolutionProvider solutionProvider,
            ISolutionLoadingStrategy solutionLoadingStrategy)
        {
            _extractors = extractors;
            _solutionProvider = solutionProvider;
            this._solutionLoadingStrategy = solutionLoadingStrategy;
        }

        public SolutionModel LoadSolution(string pathToFile)
        {
            var solution = _solutionProvider.GetSolution(pathToFile);

            return _solutionLoadingStrategy.Load(solution, _extractors);
        }
    }
}
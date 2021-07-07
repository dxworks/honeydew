using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HoneydewCore.Extractors;
using HoneydewCore.IO.Readers.Strategies;
using HoneydewCore.Logging;
using HoneydewCore.Models;
using HoneydewCore.Processors;

namespace HoneydewCore.IO.Readers.SolutionRead
{
    public class SolutionFileLoader : ISolutionLoader
    {
        private readonly IProgressLogger _progressLogger;
        private readonly IList<IFactExtractor> _extractors;

        private readonly ISolutionProvider _solutionProvider;
        private readonly ISolutionLoadingStrategy _solutionLoadingStrategy;

        public SolutionFileLoader(IProgressLogger progressLogger, IList<IFactExtractor> extractors,
            ISolutionProvider solutionProvider,
            ISolutionLoadingStrategy solutionLoadingStrategy)
        {
            _progressLogger = progressLogger;
            _extractors = extractors;
            _solutionProvider = solutionProvider;
            _solutionLoadingStrategy = solutionLoadingStrategy;
        }

        public async Task<SolutionModel> LoadSolution(string pathToFile)
        {
            _progressLogger.LogLine();
            _progressLogger.LogLine();
            _progressLogger.LogLine($"Opening the solution from {pathToFile}");

            var solution = await _solutionProvider.GetSolution(pathToFile);
            
            _progressLogger.LogLine($"Found {solution?.Projects.Count()} C# Projects in solution {pathToFile}");

            var solutionModel = await _solutionLoadingStrategy.Load(solution, _extractors);
            
            return solutionModel;
        }
    }
}
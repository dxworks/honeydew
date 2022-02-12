using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HoneydewCore.Logging;
using HoneydewExtractors.Core;
using HoneydewExtractors.CSharp.RepositoryLoading.Strategies;

namespace HoneydewExtractors.CSharp.RepositoryLoading.SolutionRead
{
    public class SolutionFileLoader : ISolutionLoader
    {
        private readonly ILogger _logger;
        private readonly IProgressLogger _progressLogger;
        private readonly IFactExtractorCreator _extractorCreator;

        private readonly ISolutionProvider _solutionProvider;
        private readonly ISolutionLoadingStrategy _solutionLoadingStrategy;

        public SolutionFileLoader(ILogger logger, IProgressLogger progressLogger, IFactExtractorCreator extractorCreator,
            ISolutionProvider solutionProvider, ISolutionLoadingStrategy solutionLoadingStrategy)
        {
            _logger = logger;
            _extractorCreator = extractorCreator;
            _solutionProvider = solutionProvider;
            _solutionLoadingStrategy = solutionLoadingStrategy;
            _progressLogger = progressLogger;
        }

        public async Task<SolutionLoadingResult> LoadSolution(string pathToFile, ISet<string> processedProjectPaths)
        {
            _logger.Log();
            _logger.Log();
            _logger.Log($"Opening the solution from {pathToFile}");

            try
            {
                var solution = await _solutionProvider.GetSolution(pathToFile);

                _logger.Log($"Found {solution?.Projects.Count()} C# Projects in solution {pathToFile}");

                var loadingResult = await _solutionLoadingStrategy.Load(solution, _extractorCreator, processedProjectPaths);

                return loadingResult;
            }
            catch (Exception e)
            {
                _logger.Log($"Could not open solution from {pathToFile} because {e}", LogLevels.Error);
                _progressLogger.Log($"Could not open solution from {pathToFile} because {e}");
            }

            return null;
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using HoneydewCore.Logging;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.RepositoryLoading.Strategies;
using HoneydewModels.CSharp;

namespace HoneydewExtractors.CSharp.RepositoryLoading.SolutionRead
{
    public class SolutionFileLoader : ISolutionLoader
    {
        private readonly ILogger _logger;
        private readonly CSharpFactExtractor _extractor;

        private readonly ISolutionProvider _solutionProvider;
        private readonly ISolutionLoadingStrategy _solutionLoadingStrategy;

        public SolutionFileLoader(ILogger logger, CSharpFactExtractor extractor,
            ISolutionProvider solutionProvider,
            ISolutionLoadingStrategy solutionLoadingStrategy)
        {
            _logger = logger;
            _extractor = extractor;
            _solutionProvider = solutionProvider;
            _solutionLoadingStrategy = solutionLoadingStrategy;
        }

        public async Task<SolutionModel> LoadSolution(string pathToFile)
        {
            _logger.Log();
            _logger.Log();
            _logger.Log($"Opening the solution from {pathToFile}");

            try
            {
                var solution = await _solutionProvider.GetSolution(pathToFile);

                _logger.Log($"Found {solution?.Projects.Count()} C# Projects in solution {pathToFile}");

                var solutionModel = await _solutionLoadingStrategy.Load(solution, _extractor);

                return solutionModel;
            }
            catch (Exception e)
            {
                _logger.Log($"Could not open solution from {pathToFile} because {e}", LogLevels.Error);
            }

            return null;
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using HoneydewCore.Logging;
using HoneydewExtractors.Core;
using HoneydewExtractors.CSharp.RepositoryLoading.Strategies;
using HoneydewModels.CSharp;

namespace HoneydewExtractors.CSharp.RepositoryLoading.SolutionRead
{
    public class SolutionFileLoader : ISolutionLoader
    {
        private readonly ILogger _logger;
        private readonly IFactExtractorCreator _extractorCreator;

        private readonly ISolutionProvider _solutionProvider;
        private readonly ISolutionLoadingStrategy _solutionLoadingStrategy;

        public SolutionFileLoader(ILogger logger, IFactExtractorCreator extractorCreator,
            ISolutionProvider solutionProvider, ISolutionLoadingStrategy solutionLoadingStrategy)
        {
            _logger = logger;
            _extractorCreator = extractorCreator;
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

                var solutionModel = await _solutionLoadingStrategy.Load(solution, _extractorCreator);

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

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
        private readonly IProgressLogger _progressLogger;
        private readonly CSharpFactExtractor _extractor;

        private readonly ISolutionProvider _solutionProvider;
        private readonly ISolutionLoadingStrategy _solutionLoadingStrategy;

        public SolutionFileLoader(IProgressLogger progressLogger, CSharpFactExtractor extractor,
            ISolutionProvider solutionProvider,
            ISolutionLoadingStrategy solutionLoadingStrategy)
        {
            _progressLogger = progressLogger;
            _extractor = extractor;
            _solutionProvider = solutionProvider;
            _solutionLoadingStrategy = solutionLoadingStrategy;
        }

        public async Task<SolutionModel> LoadSolution(string pathToFile)
        {
            _progressLogger.Log();
            _progressLogger.Log();
            _progressLogger.Log($"Opening the solution from {pathToFile}");

            try
            {
                var solution = await _solutionProvider.GetSolution(pathToFile);

                _progressLogger.Log($"Found {solution?.Projects.Count()} C# Projects in solution {pathToFile}");

                var solutionModel = await _solutionLoadingStrategy.Load(solution, _extractor);

                return solutionModel;
            }
            catch (Exception e)
            {
                _progressLogger.Log($"Could not open solution from {pathToFile} because {e}", LogLevels.Error);
            }

            return null;
        }
    }
}

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
            _progressLogger.LogLine();
            _progressLogger.LogLine();
            _progressLogger.LogLine($"Opening the solution from {pathToFile}");

            var solution = await _solutionProvider.GetSolution(pathToFile);

            _progressLogger.LogLine($"Found {solution?.Projects.Count()} C# Projects in solution {pathToFile}");

            var solutionModel = await _solutionLoadingStrategy.Load(solution, _extractor);

            return solutionModel;
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HoneydewCore.Extractors;
using HoneydewCore.IO.Readers.ProjectRead;
using HoneydewCore.IO.Readers.SolutionRead;
using HoneydewCore.IO.Readers.Strategies;
using HoneydewCore.Logging;
using HoneydewCore.Models;
using HoneydewCore.Processors;

namespace HoneydewCore.IO.Readers
{
    public class RepositoryLoader : IRepositoryLoader
    {
        private readonly IProgressLogger _progressLogger;
        private readonly IList<IFactExtractor> _extractors;
        private const string CsprojExtension = ".csproj";
        private const string SlnExtension = ".sln";

        public RepositoryLoader(IProgressLogger progressLogger, IList<IFactExtractor> extractors)
        {
            _progressLogger = progressLogger;
            _extractors = extractors;
        }

        public async Task<RepositoryModel> Load(string path)
        {
            var repositoryModel = new RepositoryModel();

            if (File.Exists(path))
            {
                if (path.EndsWith(SlnExtension))
                {
                    _progressLogger.LogLine($"Solution file found at {path}");

                    var solutionLoader = new SolutionFileLoader(_progressLogger, _extractors,
                        new MsBuildSolutionProvider(),
                        new BasicSolutionLoadingStrategy(_progressLogger,
                            new BasicProjectLoadingStrategy(_progressLogger)));
                    var solutionModel = await solutionLoader.LoadSolution(path);
                    repositoryModel.Solutions.Add(solutionModel);
                }
                else if (path.EndsWith(CsprojExtension))
                {
                    _progressLogger.LogLine($"C# Project file found at {path}");

                    var projectLoader = new ProjectLoader(_extractors, new MsBuildProjectProvider(),
                        new BasicProjectLoadingStrategy(_progressLogger));
                    var projectModel = await projectLoader.Load(path);

                    repositoryModel.Solutions.Add(new SolutionModel
                    {
                        Projects = {projectModel}
                    });
                }
                else
                {
                    _progressLogger.LogLine($"No Solution file or C# Project file found at {path}");

                    throw new SolutionNotFoundException();
                }
            }
            else if (Directory.Exists(path))
            {
                _progressLogger.LogLine($"Searching for solution files at {path}");

                var solutionPaths = Directory.GetFiles(path, $"*{SlnExtension}", SearchOption.AllDirectories);

                _progressLogger.LogLine($"Found {solutionPaths.Length} Solutions");

                foreach (var solutionPath in solutionPaths)
                {
                    var solutionLoader =
                        new SolutionFileLoader(_progressLogger, _extractors, new MsBuildSolutionProvider(),
                            new BasicSolutionLoadingStrategy(_progressLogger,
                                new BasicProjectLoadingStrategy(_progressLogger)));
                    var solutionModel = await solutionLoader.LoadSolution(solutionPath);
                    repositoryModel.Solutions.Add(solutionModel);
                }

                _progressLogger.LogLine(
                    $"Searching for C# Project files that are not in any of the found solutions at {path}");

                var defaultSolutionModel = new SolutionModel();
                var projectPaths = Directory.GetFiles(path, $"*{CsprojExtension}", SearchOption.AllDirectories);

                foreach (var relativeProjectPath in projectPaths)
                {
                    var projectPath = Path.GetFullPath(relativeProjectPath);

                    var isUsedInASolution = repositoryModel.Solutions.Any(solutionModel =>
                        solutionModel.Projects.Any(project => project.FilePath == projectPath));

                    if (isUsedInASolution) continue;

                    _progressLogger.LogLine($"C# Project file found at {projectPath}");

                    var projectLoader = new ProjectLoader(_extractors, new MsBuildProjectProvider(),
                        new BasicProjectLoadingStrategy(_progressLogger));
                    var projectModel = await projectLoader.Load(projectPath);
                    defaultSolutionModel.Projects.Add(projectModel);
                }

                if (defaultSolutionModel.Projects.Count > 0)
                {
                    _progressLogger.LogLine(
                        $"{defaultSolutionModel.Projects.Count} C# Projects were found that didn't belong to any solution file");
                    
                    repositoryModel.Solutions.Add(defaultSolutionModel);
                }
            }

            return repositoryModel;
        }
    }
}
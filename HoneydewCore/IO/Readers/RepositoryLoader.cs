using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HoneydewCore.Extractors;
using HoneydewCore.IO.Readers.ProjectRead;
using HoneydewCore.IO.Readers.SolutionRead;
using HoneydewCore.IO.Readers.Strategies;
using HoneydewCore.Models;
using HoneydewCore.Processors;

namespace HoneydewCore.IO.Readers
{
    public class RepositoryLoader : IRepositoryLoader
    {
        private readonly IList<IFactExtractor> _extractors;
        private const string CsprojExtension = ".csproj";
        private const string SlnExtension = ".sln";

        public RepositoryLoader(IList<IFactExtractor> extractors)
        {
            _extractors = extractors;
        }

        public async Task<RepositoryModel> Load(string path)
        {
            var repositoryModel = new RepositoryModel();

            if (File.Exists(path))
            {
                if (path.EndsWith(SlnExtension))
                {
                    var solutionLoader = new SolutionFileLoader(_extractors, new MsBuildSolutionProvider(),
                        new BasicSolutionLoadingStrategy(new BasicProjectLoadingStrategy()));
                    var solutionModel = await solutionLoader.LoadSolution(path);
                    repositoryModel.Solutions.Add(solutionModel);
                }
                else if (path.EndsWith(CsprojExtension))
                {
                    var projectLoader = new ProjectLoader(_extractors, new MsBuildProjectProvider(),
                        new BasicProjectLoadingStrategy());
                    var projectModel = await projectLoader.Load(path);

                    repositoryModel.Solutions.Add(new SolutionModel
                    {
                        Projects = {projectModel}
                    });
                }
                else
                {
                    throw new SolutionNotFoundException();
                }
            }
            else if (Directory.Exists(path))
            {
                var solutionPaths = Directory.GetFiles(path, $"*{SlnExtension}", SearchOption.AllDirectories);
                if (solutionPaths.Length > 0)
                {
                    foreach (var solutionPath in solutionPaths)
                    {
                        var solutionLoader =
                            new SolutionFileLoader(_extractors, new MsBuildSolutionProvider(),
                                new BasicSolutionLoadingStrategy(new BasicProjectLoadingStrategy()));
                        var solutionModel = await solutionLoader.LoadSolution(solutionPath);
                        repositoryModel.Solutions.Add(solutionModel);
                    }
                }
                else
                {
                    await Console.Error.WriteLineAsync(
                        $"No {SlnExtension} files found, searching for {CsprojExtension} files");
                    
                    var solutionModel = new SolutionModel();
                    var projectPaths = Directory.GetFiles(path, $"*{CsprojExtension}", SearchOption.AllDirectories);

                    if (projectPaths.Length <= 0)
                    {
                        throw new SolutionNotFoundException();
                    }

                    foreach (var projectPath in projectPaths)
                    {
                        var projectLoader = new ProjectLoader(_extractors, new MsBuildProjectProvider(),
                            new BasicProjectLoadingStrategy());
                        var projectModel = await projectLoader.Load(projectPath);
                        solutionModel.Projects.Add(projectModel);
                    }

                    solutionModel = AddFullNameToDependencies(solutionModel);

                    repositoryModel.Solutions.Add(solutionModel);
                }
            }

            return repositoryModel;
        }

        private static SolutionModel AddFullNameToDependencies(SolutionModel solutionModel)
        {
            var solutionModelProcessable = new ProcessorChain(IProcessable.Of(solutionModel))
                .Process(new FullNameModelProcessor())
                .Finish<SolutionModel>();

            return solutionModelProcessable.Value;
        }
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using HoneydewCore.Extractors;
using HoneydewCore.Models;
using Microsoft.CodeAnalysis;

namespace HoneydewCore.IO.Readers.Strategies
{
    public class BasicSolutionLoadingStrategy : ISolutionLoadingStrategy
    {
        private readonly IProjectLoadingStrategy _projectLoadingStrategy;

        public BasicSolutionLoadingStrategy(IProjectLoadingStrategy projectLoadingStrategy)
        {
            _projectLoadingStrategy = projectLoadingStrategy;
        }

        public async Task<SolutionModel> Load(Solution solution, IList<IFactExtractor> extractors)
        {
            SolutionModel solutionModel = new();

            foreach (var project in solution.Projects)
            {
                var projectModel = await _projectLoadingStrategy.Load(project, extractors);

                solutionModel.Projects.Add(projectModel);
            }

            return solutionModel;
        }
    }
}
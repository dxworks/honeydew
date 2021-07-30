using System;
using System.Threading.Tasks;
using HoneydewExtractors.CSharp.RepositoryLoading.ProjectRead;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace HoneydewExtractors.CSharp.RepositoryLoading.SolutionRead
{
    public class MsBuildSolutionProvider : ISolutionProvider
    {
        public Task<Solution> GetSolution(string path)
        {
            if (!MSBuildLocator.IsRegistered)
            {
                MSBuildLocator.RegisterDefaults();
            }

            var msBuildWorkspace = MSBuildWorkspace.Create();

            if (!msBuildWorkspace.Diagnostics.IsEmpty)
            {
                throw new ProjectWithErrorsException();
            }

            try
            {
                return msBuildWorkspace.OpenSolutionAsync(path);
            }
            catch (Exception)
            {
                throw new SolutionNotFoundException();
            }
        }
    }
}

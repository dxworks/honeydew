using System;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace HoneydewExtractors.CSharp.RepositoryLoading.ProjectRead
{
    public class MsBuildProjectProvider : IProjectProvider
    {
        public Task<Project> GetProject(string path)
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
                return msBuildWorkspace.OpenProjectAsync(path);
            }
            catch (Exception)
            {
                throw new ProjectNotFoundException();
            }
        }
    }
}

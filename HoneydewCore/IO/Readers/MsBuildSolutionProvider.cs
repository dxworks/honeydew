using System;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace HoneydewCore.IO.Readers
{
    public class MsBuildSolutionProvider : ISolutionProvider
    {
        public Task<Solution> GetSolution(string path)
        {
            MSBuildLocator.RegisterDefaults();

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
                throw new ProjectNotFoundException();
            }
        }
    }
}
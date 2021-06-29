using System;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace HoneydewCore.IO.Readers
{
    public class MsBuildSolutionProvider : ISolutionProvider
    {
        public Solution GetSolution(string path)
        {
            MSBuildLocator.RegisterDefaults();

            var msBuildWorkspace = MSBuildWorkspace.Create();

            if (!msBuildWorkspace.Diagnostics.IsEmpty)
            {
                throw new ProjectWithErrorsException();
            }

            try
            {
                return msBuildWorkspace.OpenSolutionAsync(path).Result;
            }
            catch (Exception)
            {
                throw new ProjectNotFoundException();
            }
        }
    }
}
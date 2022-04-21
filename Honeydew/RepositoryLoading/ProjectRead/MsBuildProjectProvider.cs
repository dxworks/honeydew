using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Honeydew.RepositoryLoading.ProjectRead;

public class MsBuildProjectProvider : IProjectProvider
{
    public Task<Project> GetProject(string path, CancellationToken cancellationToken)
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
            return msBuildWorkspace.OpenProjectAsync(path, cancellationToken: cancellationToken);
        }
        catch (Exception)
        {
            throw new ProjectNotFoundException();
        }
    }
}

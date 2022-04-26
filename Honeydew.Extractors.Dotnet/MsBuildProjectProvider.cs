using Honeydew.Extractors.Load;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Honeydew.Extractors.Dotnet;

public class MsBuildProjectProvider : IProjectProvider<Project>
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

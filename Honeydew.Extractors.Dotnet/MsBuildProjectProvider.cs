using Honeydew.Extractors.Load;
using Honeydew.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace Honeydew.Extractors.Dotnet;

public class MsBuildProjectProvider : IProjectProvider<Project>
{
    private readonly ILogger _logger;

    public MsBuildProjectProvider(ILogger logger)
    {
        _logger = logger;
    }

    public Task<Project> GetProject(string path, CancellationToken cancellationToken)
    {
        DotNetSdkLoader.RegisterMsBuild(_logger);

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

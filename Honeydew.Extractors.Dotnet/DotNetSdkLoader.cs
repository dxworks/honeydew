using Honeydew.Logging;
using Microsoft.Build.Locator;

namespace Honeydew.Extractors.Dotnet;

public class DotNetSdkLoader
{
    private const int RequiredNetSdkVersion = 8;

    public static void RegisterMsBuild(ILogger logger)
    {
        if (MSBuildLocator.IsRegistered) return;

        logger.Log("Registering MsBuild");

        var visualStudioInstances = MSBuildLocator.QueryVisualStudioInstances().ToList();
        LogInstances(logger, visualStudioInstances);

        var requiredNetSdk = visualStudioInstances.Where(vsi => vsi.Version.Major == RequiredNetSdkVersion)
            .MaxBy(vsi => vsi.Version);

        if (requiredNetSdk == null)
        {
            throw new InvalidOperationException("Could not find .NET SDK " + RequiredNetSdkVersion);
        }

        MSBuildLocator.RegisterInstance(requiredNetSdk);
    }

    private static void LogInstances(ILogger logger, IEnumerable<VisualStudioInstance> visualStudioInstances)
    {
        var versionString = string.Join(Environment.NewLine,
            visualStudioInstances.Select(vsi => vsi.Name + " " + vsi.Version));
        logger.Log("Detected the following versions: " + versionString);
    }
}

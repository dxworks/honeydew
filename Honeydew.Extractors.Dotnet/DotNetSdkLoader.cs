﻿using Honeydew.Logging;
using Microsoft.Build.Locator;
using System.Runtime.InteropServices;

namespace Honeydew.Extractors.Dotnet;

public class DotNetSdkLoader
{
    private const int RequiredNetSdkVersion = 9;

    public static void RegisterMsBuild(ILogger logger)
     { 
        logger.Log($"[RegisterMsBuild] Already registered: {MSBuildLocator.IsRegistered}");
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

    private static void LogInstances(ILogger logger, IEnumerable<VisualStudioInstance> instances)
    {
        logger.Log($"Process architecture: {RuntimeInformation.ProcessArchitecture}");
        logger.Log($"OS architecture: {RuntimeInformation.OSArchitecture}");
        logger.Log($"OS Description: {RuntimeInformation.OSDescription}");
        logger.Log($"Framework Description: {RuntimeInformation.FrameworkDescription}");
        logger.Log($"Runtime Identifier: {RuntimeInformation.RuntimeIdentifier}");
        
        foreach (var instance in instances)
        {
            logger.Log($"- {instance.Name} {instance.Version} at {instance.MSBuildPath}");
        }
    }
}

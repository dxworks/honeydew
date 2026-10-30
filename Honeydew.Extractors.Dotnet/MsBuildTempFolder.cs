namespace Honeydew.Extractors.Dotnet;

// Loading a project runs MSBuild's design-time build, which writes generated sources (AssemblyInfo,
// GlobalUsings, ...) into the project's obj/ and creates bin/. They go to this temp folder instead,
// so the analysed repository is left untouched; the folder is deleted when Honeydew exits.
// The generated sources are still compiled with the project, but are not part of the extracted model.
public static class MsBuildTempFolder
{
    private static readonly string Root =
        Path.Combine(Path.GetTempPath(), "honeydew-msbuild-" + Guid.NewGuid().ToString("N"));

    static MsBuildTempFolder()
    {
        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            try
            {
                if (Directory.Exists(Root)) Directory.Delete(Root, true);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        };
    }

    // MSBuildWorkspace properties; one folder per workspace, so different solutions never share generated files
    public static Dictionary<string, string> WorkspaceProperties()
    {
        var folder = Path.Combine(Root, Guid.NewGuid().ToString("N"));

        return new Dictionary<string, string>
        {
            ["IntermediateOutputPath"] = Path.Combine(folder, "obj") + Path.DirectorySeparatorChar,
            ["OutputPath"] = Path.Combine(folder, "bin") + Path.DirectorySeparatorChar,
        };
    }

    // True for a source file MSBuild generated into this folder
    public static bool Contains(string? filePath)
    {
        return filePath != null && filePath.StartsWith(Root + Path.DirectorySeparatorChar, StringComparison.Ordinal);
    }
}

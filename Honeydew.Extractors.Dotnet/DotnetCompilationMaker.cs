using Microsoft.CodeAnalysis;

namespace Honeydew.Extractors.Dotnet;

public abstract class DotnetCompilationMaker : ICompilationMaker
{
    protected IEnumerable<MetadataReference>? References;

    protected abstract Compilation GetConcreteCompilation(); 
    
    public Compilation GetCompilation()
    {
        References = FindTrustedReferences();

        return GetConcreteCompilation();
    }

    public IEnumerable<MetadataReference> FindTrustedReferences()
    {
        if (References != null)
        {
            return References;
        }

        var references = new List<MetadataReference>();

        var value = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
        if (value != null)
        {
            var pathToDlls = value.Split(Path.PathSeparator);
            foreach (var reference in pathToDlls.Where(pathToDll => !string.IsNullOrEmpty(pathToDll))
                         .Select(pathToDll => MetadataReference.CreateFromFile(pathToDll)))
            {
                references.Add(reference);
            }
        }

        References = references;

        return references;
    }
}

using System.Text;
using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.ScriptBeePlugin.Loaders;

internal class NamespaceTreeHandler
{
    private readonly Dictionary<string, NamespaceModel> _rootNamespaces = new();

    public IList<NamespaceModel> GetRootNamespaces()
    {
        return _rootNamespaces.Select(pair => pair.Value).ToList();
    }

    public NamespaceModel GetOrAdd(string namespaceName)
    {
        var nameParts = namespaceName.Split('.');

        NamespaceModel? lastNamespace;
        if (_rootNamespaces.TryGetValue(nameParts[0], out var rootNamespace))
        {
            if (nameParts.Length == 1)
            {
                return rootNamespace;
            }

            lastNamespace = rootNamespace;
            for (var currentIndex = 1; currentIndex < nameParts.Length; currentIndex++)
            {
                var nextNamespace =
                    lastNamespace.ChildNamespaces.FirstOrDefault(n => n.Name == nameParts[currentIndex]);
                if (nextNamespace == null)
                {
                    var stringBuilder = new StringBuilder();
                    for (var i = 0; i < currentIndex; i++)
                    {
                        stringBuilder.Append(nameParts[i]);
                        if (i != currentIndex - 1)
                        {
                            stringBuilder.Append('.');
                        }
                    }

                    var childNamespace = CreateChildNamespaces(nameParts, stringBuilder, currentIndex);

                    childNamespace.Parent = lastNamespace;
                    lastNamespace.ChildNamespaces.Add(childNamespace);

                    lastNamespace = childNamespace;
                    while (lastNamespace.ChildNamespaces.Count == 1)
                    {
                        lastNamespace = lastNamespace.ChildNamespaces[0];
                    }

                    return lastNamespace;
                }

                lastNamespace = nextNamespace;
            }

            return lastNamespace;
        }

        rootNamespace = CreateChildNamespaces(nameParts, new StringBuilder(), 0);

        _rootNamespaces.Add(nameParts[0], rootNamespace);

        lastNamespace = rootNamespace;
        while (lastNamespace.ChildNamespaces.Count == 1)
        {
            lastNamespace = lastNamespace.ChildNamespaces[0];
        }

        return lastNamespace;
    }

    private static NamespaceModel CreateChildNamespaces(IReadOnlyList<string> nameParts,
        StringBuilder currentNameStringBuilder, int currentIndex)
    {
        if (!string.IsNullOrEmpty(currentNameStringBuilder.ToString()))
        {
            currentNameStringBuilder.Append('.');
        }

        currentNameStringBuilder.Append(nameParts[currentIndex]);

        var namespaceModel = new NamespaceModel
        {
            Name = nameParts[currentIndex],
            FullName = currentNameStringBuilder.ToString(),
        };

        if (nameParts.Count - 1 == currentIndex)
        {
            return namespaceModel;
        }

        var childNamespaceModel = CreateChildNamespaces(nameParts, currentNameStringBuilder, currentIndex + 1);

        childNamespaceModel.Parent = namespaceModel;
        namespaceModel.ChildNamespaces.Add(childNamespaceModel);

        return namespaceModel;
    }
}

using System.Collections.Generic;

namespace HoneydewExtractors.Processors
{
    internal class FullNameNamespace
    {
        public string Name { get; set; }

        public FullNameNamespace Parent { get; init; }

        public Dictionary<string, FullNameNamespace> Children { get; set; } = new();

        public string GetFullName()
        {
            if (Parent == null)
            {
                return Name;
            }

            var parentName = Parent.GetFullName();

            return $"{parentName}.{Name}";
        }

        public FullNameNamespace GetChild(string[] nameParts)
        {
            if (nameParts == null || nameParts.Length == 0)
            {
                return null;
            }

            if (Name != nameParts[0])
            {
                return null;
            }

            var namespaceToSearch = this;

            for (var i = 1; i < nameParts.Length; i++)
            {
                if (!namespaceToSearch.Children.TryGetValue(nameParts[i], out var childNamespace))
                {
                    return null;
                }

                namespaceToSearch = childNamespace;
            }

            return namespaceToSearch;
        }

        public bool ContainsNameParts(string[] nameParts)
        {
            return GetChild(nameParts) != null;
        }

        public FullNameNamespace AddNamespaceChild(string className, string targetNamespace)
        {
            if (string.IsNullOrEmpty(targetNamespace) || string.IsNullOrEmpty(className))
            {
                return null;
            }

            return AddNamespaceChild(className.Split('.'), targetNamespace.Split('.'));
        }

        public IEnumerable<string> GetPossibleChildren(string childNameToBeSearched)
        {
            IList<string> childNames = new List<string>();

            var name = $".{childNameToBeSearched}";

            var leafChildren = GetLeafChildren(this);
            foreach (var child in leafChildren)
            {
                var childName = child.GetFullName();
                if (childName.EndsWith(name))
                {
                    childNames.Add(childName);
                }
            }

            return childNames;
        }

        private FullNameNamespace AddNamespaceChild(IReadOnlyList<string> classNameParts,
            IReadOnlyList<string> namespaceNameParts)
        {
            if (classNameParts[0] == namespaceNameParts[0])
            {
                return AddNamespaceChild(classNameParts);
            }

            var fullClassName = new List<string>();
            fullClassName.AddRange(namespaceNameParts);
            fullClassName.AddRange(classNameParts);
            return AddNamespaceChild(fullClassName);
        }

        private FullNameNamespace AddNamespaceChild(IEnumerable<string> fullClassName)
        {
            var namespaceToSearch = this;

            foreach (var namespacePart in fullClassName)
            {
                if (namespacePart == namespaceToSearch.Name)
                {
                    continue;
                }

                if (namespaceToSearch.Children.TryGetValue(namespacePart, out var childNamespace))
                {
                    namespaceToSearch = childNamespace;
                    continue;
                }

                var child = new FullNameNamespace
                {
                    Parent = namespaceToSearch,
                    Name = namespacePart
                };
                namespaceToSearch.Children.Add(namespacePart, child);
                namespaceToSearch = child;
            }

            return namespaceToSearch;
        }

        private static IEnumerable<FullNameNamespace> GetLeafChildren(FullNameNamespace nameNamespace)
        {
            if (nameNamespace == null)
            {
                return new List<FullNameNamespace>();
            }

            if (nameNamespace.Children.Count == 0)
            {
                return new List<FullNameNamespace>
                {
                    nameNamespace
                };
            }

            var leafChildren = new List<FullNameNamespace>();

            foreach (var (_, child) in nameNamespace.Children)
            {
                leafChildren.AddRange(GetLeafChildren(child));
            }

            return leafChildren;
        }
    }
}

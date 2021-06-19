using System.Collections.Generic;

namespace HoneydewCore.Models.Representations
{
    public class FileRelationsRepresentation
    {
        public ISet<string> Dependencies { get; } = new HashSet<string>();

        public IDictionary<string, IDictionary<string, IDictionary<string, int>>> FileRelations { get; } =
            new Dictionary<string, IDictionary<string, IDictionary<string, int>>>();

        public void Add(string sourceName, string targetName, string dependencyName, int dependencyValue)
        {
            if (string.IsNullOrWhiteSpace(sourceName))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(targetName))
            {
                Add(sourceName);
                return;
            }

            if (string.IsNullOrWhiteSpace(dependencyName))
            {
                return;
            }

            Dependencies.Add(dependencyName);

            if (!FileRelations.TryGetValue(sourceName, out var targetDictionary))
            {
                FileRelations.Add(sourceName, new Dictionary<string, IDictionary<string, int>>
                {
                    {targetName, new Dictionary<string, int> {{dependencyName, dependencyValue}}}
                });
            }
            else
            {
                if (targetDictionary.TryGetValue(targetName, out var dependenciesDictionary))
                {
                    if (!dependenciesDictionary.ContainsKey(dependencyName))
                    {
                        dependenciesDictionary.Add(dependencyName, dependencyValue);
                    }
                }
                else
                {
                    targetDictionary.Add(targetName, new Dictionary<string, int>
                        {{dependencyName, dependencyValue}});
                }
            }
        }

        public void Add(string sourceName)
        {
            if (string.IsNullOrWhiteSpace(sourceName))
            {
                return;
            }

            if (!FileRelations.ContainsKey(sourceName))
            {
                FileRelations.Add(sourceName, new Dictionary<string, IDictionary<string, int>>());
            }
        }

        public int TotalRelationsCount(string sourceName, string targetName)
        {
            if (!FileRelations.TryGetValue(sourceName, out var targetDictionary))
            {
                return 0;
            }

            if (!targetDictionary.TryGetValue(targetName, out var dependenciesDictionary))
            {
                return 0;
            }

            var sum = 0;
            foreach (var (_, value) in dependenciesDictionary)
            {
                sum += value;
            }

            return sum;
        }
    }
}
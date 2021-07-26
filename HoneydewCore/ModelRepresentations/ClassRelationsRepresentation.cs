using System.Collections.Generic;
using HoneydewCore.IO.Writers.Exporters;

namespace HoneydewCore.ModelRepresentations
{
    public class ClassRelationsRepresentation
    {
        public bool UsePrettyPrint { private get; set; }

        public ISet<string> DependenciesType { get; } = new HashSet<string>();

        public IDictionary<string, IDictionary<string, IDictionary<string, int>>> ClassRelations { get; } =
            new Dictionary<string, IDictionary<string, IDictionary<string, int>>>();

        private readonly IMetricPrettier _metricPrettier;

        public ClassRelationsRepresentation(IMetricPrettier metricPrettier)
        {
            _metricPrettier = metricPrettier;
        }

        public void Add(string sourceName, string targetName, string dependencyType, int dependencyValue)
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

            if (string.IsNullOrWhiteSpace(dependencyType))
            {
                return;
            }


            if (UsePrettyPrint)
            {
                dependencyType = _metricPrettier.Pretty(dependencyType);
            }

            DependenciesType.Add(dependencyType);

            if (!ClassRelations.TryGetValue(sourceName, out var targetDictionary))
            {
                ClassRelations.Add(sourceName, new Dictionary<string, IDictionary<string, int>>
                {
                    {targetName, new Dictionary<string, int> {{dependencyType, dependencyValue}}}
                });
            }
            else
            {
                if (targetDictionary.TryGetValue(targetName, out var dependenciesDictionary))
                {
                    if (!dependenciesDictionary.ContainsKey(dependencyType))
                    {
                        dependenciesDictionary.Add(dependencyType, dependencyValue);
                    }
                }
                else
                {
                    targetDictionary.Add(targetName, new Dictionary<string, int>
                        {{dependencyType, dependencyValue}});
                }
            }
        }

        public void Add(string sourceName)
        {
            if (string.IsNullOrWhiteSpace(sourceName))
            {
                return;
            }

            if (!ClassRelations.ContainsKey(sourceName))
            {
                ClassRelations.Add(sourceName, new Dictionary<string, IDictionary<string, int>>());
            }
        }

        public int TotalRelationsCount(string sourceName, string targetName)
        {
            if (!ClassRelations.TryGetValue(sourceName, out var targetDictionary))
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

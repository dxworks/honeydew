using System.Collections.Generic;
using System.Linq;
using HoneydewCore.IO.Writers.Exporters;

namespace HoneydewCore.Models.Representations
{
    public class FileRelationsRepresentation : PrettyPrintRepresentation, IExportable
    {
        public ISet<string> DependenciesType { get; } = new HashSet<string>();

        public IDictionary<string, IDictionary<string, IDictionary<string, int>>> FileRelations { get; } =
            new Dictionary<string, IDictionary<string, IDictionary<string, int>>>();

        public string Export(IExporter exporter)
        {
            if (exporter is IFileRelationsRepresentationExporter modelExporter)
            {
                return modelExporter.Export(this);
            }

            return string.Empty;
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

            DependenciesType.Add(dependencyType);

            if (!FileRelations.TryGetValue(sourceName, out var targetDictionary))
            {
                FileRelations.Add(sourceName, new Dictionary<string, IDictionary<string, int>>
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

        protected override IList<string> StringsToPretty()
        {
            return DependenciesType.ToList();
        }
    }
}
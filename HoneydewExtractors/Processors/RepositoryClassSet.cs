using System.Collections.Generic;

namespace HoneydewExtractors.Processors
{
    internal class RepositoryClassSet
    {
        private readonly Dictionary<string, ISet<string>> _projectDictionary = new();

        public void Add(string projectName, string classFullName)
        {
            if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(classFullName))
            {
                return;
            }

            if (_projectDictionary.TryGetValue(projectName, out var classSet))
            {
                classSet.Add(classFullName);
                return;
            }

            _projectDictionary.Add(projectName, new HashSet<string>()
            {
                classFullName
            });
        }

        public bool Contains(string projectName, string className)
        {
            if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(className))
            {
                return false;
            }

            return _projectDictionary.TryGetValue(projectName, out var classSet) && classSet.Contains(className);
        }
    }
}

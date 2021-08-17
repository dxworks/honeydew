using System.Collections.Generic;

namespace HoneydewExtractors.Core
{
    public class RepositoryClassSet : IRepositoryClassSet
    {
        private readonly Dictionary<string, ISet<string>> _projectDictionary = new();

        public void Add(string projectName, string classFullName)
        {
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
            return _projectDictionary.TryGetValue(projectName, out var classSet) && classSet.Contains(className);
        }
    }
}

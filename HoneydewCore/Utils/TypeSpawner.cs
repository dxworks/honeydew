using System;
using System.Collections.Generic;
using System.Linq;

namespace HoneydewCore.Utils
{
    public class TypeSpawner<TM>
    {
        private readonly ISet<Type> _types = new HashSet<Type>();

        public void LoadType<T>() where T : TM
        {
            _types.Add(typeof(T));
        }

        public IList<TM> InstantiateMetrics()
        {
            return _types
                .Select(type => (TM) Activator.CreateInstance(type))
                .ToList();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewExtractors.Metrics;

namespace HoneydewExtractors
{
    public abstract class PrettyPrintRepresentation
    {
        public bool UsePrettyPrint { private get; set; }

        protected abstract IList<string> StringsToPretty();

        public IList<string> GetDependenciesTypePretty()
        {
            var stringsToPretty = StringsToPretty();

            if (!UsePrettyPrint)
            {
                return stringsToPretty;
            }

            return stringsToPretty.Select(dependency =>
            {
                var type = Type.GetType(dependency);
                if (type == null || !typeof(IMetric).IsAssignableFrom(type))
                {
                    return dependency;
                }

                var metricExtractor = (IMetric) Activator.CreateInstance(type);

                return metricExtractor == null ? dependency : metricExtractor.PrettyPrint();
            }).ToList();
        }
    }
}

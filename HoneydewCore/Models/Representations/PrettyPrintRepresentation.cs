using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Extractors.Metrics;

namespace HoneydewCore.Models.Representations
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
                if (type == null || !typeof(IMetricExtractor).IsAssignableFrom(type))
                {
                    return dependency;
                }

                var metricExtractor = (IMetricExtractor) Activator.CreateInstance(type);

                return metricExtractor == null ? dependency : metricExtractor.PrettyPrint();
            }).ToList();
        }
    }
}
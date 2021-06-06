﻿using System.Collections.Generic;
using HoneydewCore.Extractors.Metrics;

namespace HoneydewCore.Models
{
    public class ClassModel : ProjectEntity
    {
        public string Namespace { get; init; }

        public IDictionary<string, IMetric> Metrics { get; } = new Dictionary<string, IMetric>();
    }
}
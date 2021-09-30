﻿using System.Collections.Generic;
using HoneydewCore.Logging;

namespace HoneydewExtractors.Core.Metrics.Visitors
{
    public class CompositeVisitor : ICompositeVisitor, IRequireLoggingVisitor
    {
        public ILogger Logger { get; set; }

        private readonly ISet<ITypeVisitor> _visitors = new HashSet<ITypeVisitor>();

        public CompositeVisitor()
        {
        }

        protected CompositeVisitor(IEnumerable<ITypeVisitor> visitors)
        {
            if (visitors == null)
            {
                return;
            }

            foreach (var visitor in visitors)
            {
                _visitors.Add(visitor);
            }
        }

        public void Add(ITypeVisitor visitor)
        {
            _visitors.Add(visitor);
        }

        public IEnumerable<ITypeVisitor> GetContainedVisitors()
        {
            return _visitors;
        }

        public void Accept(IVisitor visitor)
        {
            foreach (var typeVisitor in _visitors)
            {
                visitor.Visit(typeVisitor);
            }
        }
    }
}

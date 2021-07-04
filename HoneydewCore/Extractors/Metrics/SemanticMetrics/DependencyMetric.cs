﻿using System;
using System.Collections.Generic;
using HoneydewCore.Models.Representations;
using HoneydewCore.Utils;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewCore.Extractors.Metrics.SemanticMetrics
{
    // ICompilationUnitMetric is used because The Metric uses the Usings statements
    public abstract class DependencyMetric : CSharpMetricExtractor, ISemanticMetric, ICompilationUnitMetric, IRelationMetric 
    {
        public DependencyDataMetric DataMetric { get; set; } = new();

        public override IMetric GetMetric()
        {
            return new Metric<DependencyDataMetric>(DataMetric);
        }
        
        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            DataMetric.Usings.Add(node.Name.ToString());
        }

        public IList<FileRelation> GetRelations(object metricValue)
        {
            try
            {
                var dataMetric = (DependencyDataMetric) metricValue;

                IList<FileRelation> fileRelations = new List<FileRelation>();

                foreach (var (dependency, count) in dataMetric.Dependencies)
                {
                    var type = Type.GetType(dependency);
                    if (type is {IsPrimitive: true} || CSharpConstants.IsPrimitive(dependency))
                    {
                        continue;
                    }

                    var relationType = GetType().FullName;
                    var fileRelation = new FileRelation
                    {
                        FileTarget = dependency,
                        RelationCount = count,
                        RelationType = relationType
                    };
                    fileRelations.Add(fileRelation);
                }

                return fileRelations;
            }
            catch (Exception)
            {
                return new List<FileRelation>();
            }
        }

        protected void AddDependency(string dependencyName)
        {
            if (DataMetric.Dependencies.ContainsKey(dependencyName))
            {
                DataMetric.Dependencies[dependencyName]++;
            }
            else
            {
                DataMetric.Dependencies.Add(dependencyName, 1);
            }
        }
    }
}
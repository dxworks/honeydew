﻿using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewCore.Utils;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewModels.CSharp;
using HoneydewModels.Types;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations
{
    public class ReturnValueRelationVisitor : IModelVisitor<IClassType>, IRelationVisitor
    {
        public string PrettyPrint()
        {
            return "returns";
        }

        public void Visit(IClassType modelType)
        {
            if (modelType is not IMembersClassType membersClassType)
            {
                return;
            }

            var dependencies = new Dictionary<string, int>();

            foreach (var methodType in membersClassType.Methods)
            {
                var typeName = CSharpConstants.GetNonNullableName(methodType.ReturnValue.Type.Name);
                if (dependencies.ContainsKey(typeName))
                {
                    dependencies[typeName]++;
                }
                else
                {
                    dependencies.Add(typeName, 1);
                }
            }

            membersClassType.Metrics.Add(new MetricModel
            {
                ExtractorName = GetType().ToString(),
                Value = dependencies,
                ValueType = dependencies.GetType().ToString()
            });
        }
    }
}

using System;
using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewCore.Processors;
using HoneydewExtractors.Core.Metrics.Extraction.Class.Relations;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels.CSharp;

namespace HoneydewExtractors.Processors
{
    public class
        RepositoryModelToClassRelationsProcessor : IProcessorFunction<RepositoryModel,
            ClassRelationsRepresentation>
    {
        public ClassRelationsRepresentation Process(RepositoryModel repositoryModel)
        {
            if (repositoryModel == null)
            {
                return new ClassRelationsRepresentation();
            }

            var classRelationsRepresentation = new ClassRelationsRepresentation();

            foreach (var classType in repositoryModel.GetEnumerable())
            {
                foreach (var metricModel in classType.Metrics)
                {
                    try
                    {
                        var instance = Activator.CreateInstance(Type.GetType(metricModel.ExtractorName));
                        if (instance is RelationVisitor relationVisitor)
                        {
                            var dictionary = (Dictionary<string, int>)metricModel.Value;
                            foreach (var (targetName, count) in dictionary)
                            {
                                if (CSharpConstants.IsPrimitive(targetName))
                                {
                                    continue;
                                }

                                classRelationsRepresentation.Add(classType.Name, targetName,
                                    relationVisitor.PrettyPrint(),
                                    count);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        // 
                    }
                }
            }

            return classRelationsRepresentation;
        }
    }
}

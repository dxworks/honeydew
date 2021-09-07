using System;
using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewCore.Processors;
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
                        var type = Type.GetType(metricModel.ExtractorName);
                        if (type == null)
                        {
                            continue;
                        }

                        var instance = Activator.CreateInstance(type);
                        if (instance is IRelationVisitor relationVisitor)
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
                    catch (Exception)
                    {
                        // 
                    }
                }
            }

            return classRelationsRepresentation;
        }
    }
}

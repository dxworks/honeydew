using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewModels.CSharp;
using HoneydewModels.Types;

namespace HoneydewCore.Processors
{
    public class
        RepositoryModelToCyclomaticComplexityPerFileProcessor : IProcessorFunction<RepositoryModel,
            CyclomaticComplexityPerFileRepresentation>
    {
        public CyclomaticComplexityPerFileRepresentation Process(RepositoryModel repositoryModel)
        {
            var representation = new CyclomaticComplexityPerFileRepresentation();

            if (repositoryModel == null)
            {
                return representation;
            }

            var classesGroupedByFilePath = GroupClassesByFilePath(repositoryModel);

            foreach (var (filePath, classModels) in classesGroupedByFilePath)
            {
                CalculateCycloComponents(classModels, out var maxCyclo, out var minCyclo, out var avgCyclo,
                    out var sumCyclo, out var atc);

                representation.AddConcern(new Concern
                {
                    Entity = filePath,
                    Tag = "metric.maxCyclo",
                    Strength = maxCyclo.ToString()
                });

                representation.AddConcern(new Concern
                {
                    Entity = filePath,
                    Tag = "metric.minCyclo",
                    Strength = minCyclo.ToString()
                });

                representation.AddConcern(new Concern
                {
                    Entity = filePath,
                    Tag = "metric.avgCyclo",
                    Strength = avgCyclo.ToString()
                });

                representation.AddConcern(new Concern
                {
                    Entity = filePath,
                    Tag = "metric.sumCyclo",
                    Strength = sumCyclo.ToString()
                });

                representation.AddConcern(new Concern
                {
                    Entity = filePath,
                    Tag = "metric.atc",
                    Strength = atc.ToString()
                });
            }

            return representation;
        }

        private static void CalculateCycloComponents(List<IClassType> classModels, out int maxCyclo, out int minCyclo,
            out int avgCyclo, out int sumCyclo, out int atc)
        {
            var maxCyclomatic = 1;
            var minCyclomatic = int.MaxValue;
            var sumCyclomatic = 0;

            var count = 0;

            maxCyclo = maxCyclomatic;
            minCyclo = minCyclomatic;
            avgCyclo = 0;
            sumCyclo = 0;
            atc = 0;

            if (classModels.Count <= 0)
            {
                minCyclo = 0;
                maxCyclo = 0;
                return;
            }

            var atcSum = 0;

            foreach (var classType in classModels)
            {
                if (classType is not ClassModel classModel)
                {
                    continue;
                }

                foreach (var methodModel in classModel.Methods)
                {
                    UpdateVariables(methodModel.CyclomaticComplexity);
                }

                foreach (var constructorModel in classModel.Constructors)
                {
                    UpdateVariables(constructorModel.CyclomaticComplexity);
                }

                foreach (var propertyModel in classModel.Properties)
                {
                    UpdateVariables(propertyModel.CyclomaticComplexity);
                }
            }

            void UpdateVariables(int cyclomaticComplexity)
            {
                if (maxCyclomatic < cyclomaticComplexity)
                {
                    maxCyclomatic = cyclomaticComplexity;
                }

                if (minCyclomatic > cyclomaticComplexity)
                {
                    minCyclomatic = cyclomaticComplexity;
                }

                sumCyclomatic += cyclomaticComplexity;

                count++;

                atcSum += cyclomaticComplexity / 10;
            }

            atc = atcSum;
            maxCyclo = maxCyclomatic;
            minCyclo = minCyclomatic;
            sumCyclo = sumCyclomatic;
            if (count != 0)
            {
                avgCyclo = sumCyclomatic / count;
            }
            else
            {
                maxCyclo = minCyclo = sumCyclo = avgCyclo = 0;
            }
        }

        private static Dictionary<string, List<IClassType>> GroupClassesByFilePath(RepositoryModel repositoryModel)
        {
            var classModelDictionary = new Dictionary<string, List<IClassType>>();

            foreach (var classModel in repositoryModel.GetEnumerable())
            {
                if (classModelDictionary.TryGetValue(classModel.FilePath, out var classModels))
                {
                    classModels.Add(classModel);
                }
                else
                {
                    classModelDictionary.Add(classModel.FilePath, new List<IClassType>
                    {
                        classModel
                    });
                }
            }

            return classModelDictionary;
        }
    }
}

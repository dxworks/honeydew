using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewModels.CSharp;

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
                    out var sumCyclo);

                representation.AddConcern(new Concern
                {
                    Entity = filePath,
                    Tag = "maxCyclo",
                    Strength = maxCyclo.ToString()
                });

                representation.AddConcern(new Concern
                {
                    Entity = filePath,
                    Tag = "minCyclo",
                    Strength = minCyclo.ToString()
                });

                representation.AddConcern(new Concern
                {
                    Entity = filePath,
                    Tag = "avgCyclo",
                    Strength = avgCyclo.ToString()
                });

                representation.AddConcern(new Concern
                {
                    Entity = filePath,
                    Tag = "sumCyclo",
                    Strength = sumCyclo.ToString()
                });
            }

            return representation;
        }

        private static void CalculateCycloComponents(List<ClassModel> classModels, out int maxCyclo, out int minCyclo,
            out int avgCyclo, out int sumCyclo)
        {
            var maxCyclomatic = 1;
            var minCyclomatic = int.MaxValue;
            var sumCyclomatic = 0;

            var count = 0;

            maxCyclo = maxCyclomatic;
            minCyclo = minCyclomatic;
            avgCyclo = 0;
            sumCyclo = 0;

            if (classModels.Count <= 0)
            {
                minCyclo = 1;
                return;
            }

            foreach (var classModel in classModels)
            {
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
            }

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

        private static Dictionary<string, List<ClassModel>> GroupClassesByFilePath(RepositoryModel repositoryModel)
        {
            var classModelDictionary = new Dictionary<string, List<ClassModel>>();

            foreach (var classModel in repositoryModel.GetEnumerable())
            {
                if (classModelDictionary.TryGetValue(classModel.FilePath, out var classModels))
                {
                    classModels.Add(classModel);
                }
                else
                {
                    classModelDictionary.Add(classModel.FilePath, new List<ClassModel>
                    {
                        classModel
                    });
                }
            }

            return classModelDictionary;
        }
    }
}

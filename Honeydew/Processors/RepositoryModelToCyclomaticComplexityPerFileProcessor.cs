using Honeydew.ModelRepresentations;
using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.Processors;

public class RepositoryModelToCyclomaticComplexityPerFileProcessor
{
    public CyclomaticComplexityPerFileRepresentation Process(RepositoryModel? repositoryModel)
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

            representation.AddConcern(new Concern(filePath, "metric.maxCyclo", maxCyclo.ToString()));

            representation.AddConcern(new Concern(filePath, "metric.minCyclo", minCyclo.ToString()));

            representation.AddConcern(new Concern(filePath, "metric.avgCyclo", avgCyclo.ToString()));

            representation.AddConcern(new Concern(filePath, "metric.sumCyclo", sumCyclo.ToString()));

            representation.AddConcern(new Concern(filePath, "metric.atc", atc.ToString()));
        }

        return representation;
    }

    private static void CalculateCycloComponents(List<EntityModel> entityModels, out int maxCyclo, out int minCyclo,
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

        if (entityModels.Count <= 0)
        {
            minCyclo = 0;
            maxCyclo = 0;
            return;
        }

        var atcSum = 0;

        foreach (var entityModel in entityModels)
        {
            switch (entityModel)
            {
                case ClassModel classModel:
                    foreach (var methodModel in classModel.Methods)
                    {
                        UpdateVariables(methodModel.CyclomaticComplexity);
                    }

                    foreach (var constructorModel in classModel.Constructors)
                    {
                        UpdateVariables(constructorModel.CyclomaticComplexity);
                    }

                    if (classModel.Destructor != null)
                    {
                        UpdateVariables(classModel.Destructor.CyclomaticComplexity);
                    }

                    foreach (var propertyModel in classModel.Properties)
                    {
                        UpdateVariables(propertyModel.CyclomaticComplexity);
                    }

                    break;
                case InterfaceModel interfaceModel:
                    foreach (var methodModel in interfaceModel.Methods)
                    {
                        UpdateVariables(methodModel.CyclomaticComplexity);
                    }

                    foreach (var propertyModel in interfaceModel.Properties)
                    {
                        UpdateVariables(propertyModel.CyclomaticComplexity);
                    }

                    break;
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

    private static Dictionary<string, List<EntityModel>> GroupClassesByFilePath(RepositoryModel repositoryModel)
    {
        var classModelDictionary = new Dictionary<string, List<EntityModel>>();

        foreach (var entityModel in repositoryModel.GetEnumerable())
        {
            if (string.IsNullOrEmpty(entityModel.FilePath))
            {
                continue;
            }

            if (classModelDictionary.TryGetValue(entityModel.FilePath, out var entityModels))
            {
                entityModels.Add(entityModel);
            }
            else
            {
                classModelDictionary.Add(entityModel.FilePath, new List<EntityModel>
                {
                    entityModel
                });
            }
        }

        return classModelDictionary;
    }
}

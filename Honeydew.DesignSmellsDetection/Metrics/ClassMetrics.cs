using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.DesignSmellsDetection.Metrics;

public class ClassMetrics
{
    private const string TotalClassCohesionKey = "tcc";
    private const string WeightedMethodCountKey = "wmc";
    private const string AccessToForeignDataForTypeKey = "atfd";
    private const string WeightOfAClassKey = "woc";
    private const string BaseClassUsageRatioKey = "bur";
    private const string BaseClassOverridingRatioKey = "bovr";
    private const string NumberOfProtectedMembersKey = "nprm";
    private const string AverageMethodWeightKey = "amw";

    private readonly IDictionary<string, double> _classMetrics;

    private ClassMetrics(IDictionary<string, double> classMetrics)
    {
        _classMetrics = classMetrics;
    }

    public double TotalClassCohesion
    {
        get => _classMetrics[TotalClassCohesionKey];
        set => _classMetrics[TotalClassCohesionKey] = value;
    }

    public double WeightOfAClass
    {
        get => _classMetrics[WeightOfAClassKey];
        set => _classMetrics[WeightOfAClassKey] = value;
    }

    public double WeightedMethodCount
    {
        get => _classMetrics[WeightedMethodCountKey];
        set => _classMetrics[WeightedMethodCountKey] = value;
    }

    public int AccessToForeignDataForType
    {
        get => (int)_classMetrics[AccessToForeignDataForTypeKey];
        set => _classMetrics[AccessToForeignDataForTypeKey] = value;
    }

    public double BaseClassUsageRatio
    {
        get => _classMetrics[BaseClassUsageRatioKey];
        set => _classMetrics[BaseClassUsageRatioKey] = value;
    }

    public double BaseClassOverridingRatio
    {
        get => _classMetrics[BaseClassOverridingRatioKey];
        set => _classMetrics[BaseClassOverridingRatioKey] = value;
    }

    public int NumberOfProtectedMembers
    {
        get => (int)_classMetrics[NumberOfProtectedMembersKey];
        set => _classMetrics[NumberOfProtectedMembersKey] = value;
    }

    public double AverageMethodWeight
    {
        get => _classMetrics[AverageMethodWeightKey];
        set => _classMetrics[AverageMethodWeightKey] = value;
    }

    public static ClassMetrics For(ClassModel classModel)
    {
        return new ClassMetrics(classModel.Metrics);
    }
}
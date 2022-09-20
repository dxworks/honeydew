using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.DesignSmellsDetection.Metrics;

public class Metrics
{
    private const string TotalClassCohesionKey = "tcc";
    private const string WeightedMethodCountKey = "wmc";
    private const string AccessToForeignDataKey = "atfd";
    private const string WeightOfAClassKey = "woc";
    private const string BaseClassUsageRatioKey = "bur";
    private const string BaseClassOverridingRatioKey = "bovr";
    private const string NumberOfProtectedMembersKey = "nprm";
    private const string AverageMethodWeightKey = "amw";
    private const string NopOverridingMethodsKey = "nopOverridingMethods";
    private const string LocalityOfAttributeAccessKey = "laa";
    private const string ForeignDataProvidersKey = "fdp";

    private readonly IDictionary<string, double> _classMetrics;

    private Metrics(IDictionary<string, double> classMetrics)
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

    public int AccessToForeignData
    {
        get => (int)_classMetrics[AccessToForeignDataKey];
        set => _classMetrics[AccessToForeignDataKey] = value;
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

    public int NopOverridingMethods
    {
        get => (int)_classMetrics[NopOverridingMethodsKey];
        set => _classMetrics[NopOverridingMethodsKey] = value;
    }

    public double LocalityOfAttributeAccess
    {
        get => _classMetrics[LocalityOfAttributeAccessKey];
        set => _classMetrics[LocalityOfAttributeAccessKey] = value;
    }

    public int ForeignDataProviders
    {
        get => (int)_classMetrics[ForeignDataProvidersKey];
        set => _classMetrics[ForeignDataProvidersKey] = value;
    }

    public static Metrics For(ClassModel classModel)
    {
        return new Metrics(classModel.Metrics);
    }

    public static Metrics For(MethodModel methodModel)
    {
        return new Metrics(methodModel.Metrics);
    }
}
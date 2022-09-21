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
    private const string NumberOfAccessedVariablesKey = "noav";

    private readonly IDictionary<string, double> _metrics;

    private Metrics(IDictionary<string, double> metrics)
    {
        _metrics = metrics;
    }

    public double TotalClassCohesion
    {
        get => _metrics[TotalClassCohesionKey];
        set => _metrics[TotalClassCohesionKey] = value;
    }

    public double WeightOfAClass
    {
        get => _metrics[WeightOfAClassKey];
        set => _metrics[WeightOfAClassKey] = value;
    }

    public double WeightedMethodCount
    {
        get => _metrics[WeightedMethodCountKey];
        set => _metrics[WeightedMethodCountKey] = value;
    }

    public int AccessToForeignData
    {
        get => (int)_metrics[AccessToForeignDataKey];
        set => _metrics[AccessToForeignDataKey] = value;
    }

    public double BaseClassUsageRatio
    {
        get => _metrics[BaseClassUsageRatioKey];
        set => _metrics[BaseClassUsageRatioKey] = value;
    }

    public double BaseClassOverridingRatio
    {
        get => _metrics[BaseClassOverridingRatioKey];
        set => _metrics[BaseClassOverridingRatioKey] = value;
    }

    public int NumberOfProtectedMembers
    {
        get => (int)_metrics[NumberOfProtectedMembersKey];
        set => _metrics[NumberOfProtectedMembersKey] = value;
    }

    public double AverageMethodWeight
    {
        get => _metrics[AverageMethodWeightKey];
        set => _metrics[AverageMethodWeightKey] = value;
    }

    public int NopOverridingMethods
    {
        get => (int)_metrics[NopOverridingMethodsKey];
        set => _metrics[NopOverridingMethodsKey] = value;
    }

    public double LocalityOfAttributeAccess
    {
        get => _metrics[LocalityOfAttributeAccessKey];
        set => _metrics[LocalityOfAttributeAccessKey] = value;
    }

    public int ForeignDataProviders
    {
        get => (int)_metrics[ForeignDataProvidersKey];
        set => _metrics[ForeignDataProvidersKey] = value;
    }

    public int NumberOfAccessedVariables
    {
        get => (int)_metrics[NumberOfAccessedVariablesKey];
        set => _metrics[NumberOfAccessedVariablesKey] = value;
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
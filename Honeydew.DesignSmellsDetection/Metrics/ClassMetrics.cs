using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.DesignSmellsDetection.Metrics;

public class ClassMetrics
{
    private const string TotalClassCohesionKey = "tcc";
    private const string WeightedMethodCountKey = "wmc";
    private const string AccessToForeignDataForTypeKey = "atfd";
    private const string WeightOfAClassKey = "woc";

    private readonly IDictionary<string, double> _classMetrics;

    public static ClassMetrics For(ClassModel classModel)
    {
        return new ClassMetrics(classModel.Metrics);
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

    private ClassMetrics(IDictionary<string, double> classMetrics)
    {
        _classMetrics = classMetrics;
    }
}
using CSharpFunctionalExtensions;
using DxWorks.ScriptBee.Plugins.Honeydew.Models;

namespace Honeydew.DesignSmellsDetection.DetectionStrategies;

public class TraditionBreaker : ClassificationDesignSmellDetectionStrategy
{
    protected override Maybe<DesignSmell> DetectCore(ClassModel t)
    {
        var metrics = Metrics.Metrics.For(t);
        var nopOverridingMethods = metrics.NopOverridingMethods;

        if (nopOverridingMethods > 0)
            return Maybe<DesignSmell>.From(
                new DesignSmell
                {
                    Name = "Tradition Breaker",
                    Severity = CalculateSeverity(nopOverridingMethods),
                    SourceFile = t.FilePath,
                    Metrics = new Dictionary<string, double>
                    {
                        { "nopOverridingMethods", nopOverridingMethods }
                    }
                });

        return Maybe<DesignSmell>.None;
    }

    private static double CalculateSeverity(int nopOverridingMethods)
    {
        return LinearNormalization.WithMeasurementRange(1, 10).ValueFor(2 * nopOverridingMethods);
    }
}

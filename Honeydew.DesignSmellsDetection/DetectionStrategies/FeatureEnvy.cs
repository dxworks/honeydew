using CSharpFunctionalExtensions;
using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.DesignSmellsDetection.DetectionStrategies;

public class FeatureEnvy : IDetectMethodDesignSmell
{
    public Maybe<DesignSmell> Detect(MethodModel m)
    {
        var metrics = Metrics.Metrics.For(m);
        var atfd = metrics.AccessToForeignData;
        var laa = metrics.LocalityOfAttributeAccess;
        var fdp = metrics.ForeignDataProviders;

        if (atfd > 5 && laa < CommonFractionThreshold.OneThird && fdp > 0 && fdp <= 2)
        {
            return Maybe<DesignSmell>.From(
                new DesignSmell
                {
                    Name = "Feature Envy",
                    Severity = CalculateSeverity(atfd),
                    SourceFile = m.Entity.FilePath,
                    Metrics = new Dictionary<string, double>
                    {
                        { "atfd", atfd },
                        { "laa", laa },
                        { "fdp", fdp }
                    }
                });
        }

        return Maybe<DesignSmell>.None;
    }

    private static double CalculateSeverity(int atfd)
    {
        return LinearNormalization.WithMeasurementRange(Few, 20).ValueFor(atfd);
    }
}
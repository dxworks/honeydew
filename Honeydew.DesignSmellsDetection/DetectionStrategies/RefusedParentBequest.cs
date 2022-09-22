using CSharpFunctionalExtensions;
using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.DesignSmellsDetection.DetectionStrategies;

public class RefusedParentBequest : ClassificationDesignSmellDetectionStrategy
{
    protected override Maybe<DesignSmell> DetectCore(ClassModel t)
    {
        const int few = 3;
        const int amwAverage = 2;
        const int wmcAverage = 14;
        const int nomAverage = 7;

        var metrics = Metrics.Metrics.For(t);
        var bur = metrics.BaseClassUsageRatio;
        var bovr = metrics.BaseClassOverridingRatio;
        var nprotm = metrics.NumberOfProtectedMembers;
        var wmc = metrics.WeightedMethodCount;
        var nom = t.MethodsPropertiesAndConstructors.Count;
        var amw = metrics.AverageMethodWeight;

        var childClassIgnoresBequest = nprotm > few && bur < CommonFractionThreshold.OneThird
                                       || bovr < CommonFractionThreshold.OneThird;
        var childClassIsNotTooSmallAndSimple = (amw > amwAverage || wmc > wmcAverage) && nom > nomAverage;

        if (nprotm > 0 && childClassIgnoresBequest && childClassIsNotTooSmallAndSimple)
        {
            return Maybe<DesignSmell>.From(
                new DesignSmell
                {
                    Name = "Refused Parent Bequest",
                    Severity = CalculateSeverity(bur, bovr),
                    SourceFile = t.FilePath,
                    Metrics = new Dictionary<string, double>
                    {
                        { "bur", bur },
                        { "bovr", bovr },
                        { "nprotm", nprotm },
                        { "wmc", wmc },
                        { "nom", nom },
                        { "amw", amw }
                    }
                });
        }

        return Maybe<DesignSmell>.None;
    }

    private static double CalculateSeverity(double bur, double bovr)
    {
        var severityBur = LinearNormalization.WithMeasurementRange(0.75, 1).ValueFor(1 - bur);
        var severityBovr = LinearNormalization.WithMeasurementRange(0.75, 1).ValueFor(1 - bovr);

        return (severityBur + severityBovr) / 2;
    }
}

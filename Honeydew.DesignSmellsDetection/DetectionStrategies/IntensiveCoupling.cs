using CSharpFunctionalExtensions;
using DxWorks.ScriptBee.Plugins.Honeydew.Models;

namespace Honeydew.DesignSmellsDetection.DetectionStrategies;

public class IntensiveCoupling : IDetectMethodDesignSmell
{
    public Maybe<DesignSmell> Detect(MethodModel m)
    {
        const int few = 5;
        const int shortMemoryCap = 7;

        // TODO needs nesting
        //var maxNesting = m.ILNestingDepth;

        var metrics = Metrics.Metrics.For(m);
        var cint = metrics.CouplingIntensity;
        var cdisp = metrics.CouplingDispersion;

        if (cint > shortMemoryCap && cdisp < CommonFractionThreshold.Half
             || cint > few && cdisp < CommonFractionThreshold.AQuarter)
        {
            return Maybe<DesignSmell>.From(
                new DesignSmell
                {
                    Name = "Intensive Coupling",
                    Severity = CalculateSeverity(cint, cdisp),
                    SourceFile = m.Entity.FilePath,
                    Metrics = new Dictionary<string, double>
                    {
                        {"cint", cint},
                        {"cdisp", cdisp}
                    }
                });
        }

        return Maybe<DesignSmell>.None;
    }

    private static double CalculateSeverity(int cint, double cdisp)
    {
        var severityCdisp = LinearNormalization.WithMeasurementRange(3, 9).ValueFor(cdisp);
        var severityCint = LinearNormalization.WithMeasurementRange(7, 21).ValueFor(cint);

        return (3 * severityCdisp + severityCint) / 4;
    }
}

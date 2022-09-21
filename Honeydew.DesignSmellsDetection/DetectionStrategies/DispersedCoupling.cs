using CSharpFunctionalExtensions;
using Honeydew.DesignSmellsDetection.Metrics;
using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.DesignSmellsDetection.DetectionStrategies;

public class DispersedCoupling : IDetectMethodDesignSmell
{
    public Maybe<DesignSmell> Detect(MethodModel m)
    {
        const int shortMemoryCap = 7;

        //TODO needs nesting
        //var maxNesting = m.ILNestingDepth;
        var metrics = Metrics.Metrics.For(m);
        var cint = metrics.CouplingIntensity;
        var cdisp = metrics.CouplingDispersion;

        if (cint > shortMemoryCap && cdisp >= CommonFractionThreshold.Half)
            return Maybe<DesignSmell>.From(
                new DesignSmell
                {
                    Name = "Dispersed Coupling",
                    Severity = CalculateSeverity(m),
                    SourceFile = m.Entity.FilePath,
                    Metrics = new Dictionary<string, double>
                    {
                        { "cint", cint },
                        { "cdisp", cdisp }
                    }
                });

        return Maybe<DesignSmell>.None;
    }

    private static double CalculateSeverity(MethodModel method)
    {
        List<Tuple<EntityModel, IList<MemberModel>>> relevantCouplingIntensityPerProvider =
            method.CouplingIntensityPerProvider().Where(t => t.Item2.Count >= 7).ToList();

        var relevantOcio = relevantCouplingIntensityPerProvider.Sum(g => g.Item2.Count);
        var severityRelevantOcio = LinearNormalization.WithMeasurementRange(7, 21).ValueFor(relevantOcio);

        var relevantOcdo = relevantCouplingIntensityPerProvider.Count;
        var severityRelevantOcdo = LinearNormalization.WithMeasurementRange(1, 4).ValueFor(relevantOcdo);

        return (2 * severityRelevantOcio + severityRelevantOcdo) / 3;
    }
}
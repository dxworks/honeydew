using CSharpFunctionalExtensions;
using Honeydew.DesignSmellsDetection.Metrics;
using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.DesignSmellsDetection.DetectionStrategies;

public class DataClass : IDetectTypeDesignSmell
{
    private const int Few = 4;

    public Maybe<DesignSmell> Detect(ClassModel t)
    {
        var metrics = Metrics.Metrics.For(t);

        var woc = metrics.WeightOfAClass;
        var wmc = metrics.WeightedMethodCount;
        IList<FieldModel> publicAttributes = t.PublicAttributes().ToList();
        var nopa = publicAttributes.Count;
        IList<PropertyModel> accessors = t.Accessors().ToList();
        var noam = accessors.Count;

        if (woc < CommonFractionThreshold.OneThird
            && ((nopa + noam > Few && wmc < 31)
                || (nopa + noam > 8 && wmc < 47)))
            return Maybe<DesignSmell>.From(
                new DesignSmell
                {
                    Name = "Data Class",
                    Severity = CalculateSeverity(publicAttributes, accessors, t),
                    SourceFile = t.FilePath,
                    Metrics = new Dictionary<string, double>
                    {
                        { "woc", woc },
                        { "wmc", wmc },
                        { "nopa", nopa },
                        { "noam", noam }
                    }
                });

        return Maybe<DesignSmell>.None;
    }

    private static double CalculateSeverity(IList<FieldModel> publicAttributes, IList<PropertyModel> accessors,
        ClassModel type)
    {
        var severityExploit = SeverityExploit(publicAttributes, accessors, type);

        var severityExposure = SeverityExposure(publicAttributes, accessors);

        return (2 * severityExploit + severityExposure) / 3;
    }

    private static double SeverityExposure(IList<FieldModel> publicAttributes, IList<PropertyModel> accessors)
    {
        return LinearNormalization.WithMeasurementRange(Few, 20).ValueFor(publicAttributes.Count + accessors.Count);
    }

    private static double SeverityExploit(IList<FieldModel> publicAttributes, IList<PropertyModel> accessors,
        ClassModel type)
    {
        var attributeAccesses = publicAttributes.SelectMany(pa => pa.Accesses);
        var fieldAccesses = attributeAccesses.Concat(accessors.SelectMany(a => a.Accesses)).ToHashSet();
        var methodsUsingData = fieldAccesses.Select(fa => fa.Caller).ToHashSet();

        var classesUsingPublicData = methodsUsingData.Select(m => m.Entity).ToHashSet();
        classesUsingPublicData.Remove(type);

        return LinearNormalization.WithMeasurementRange(2, 10).ValueFor(classesUsingPublicData.Count);
    }
}

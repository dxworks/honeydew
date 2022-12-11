using CSharpFunctionalExtensions;
using DxWorks.ScriptBee.Plugins.Honeydew.Models;

namespace Honeydew.DesignSmellsDetection.DetectionStrategies;

public class ShotgunSurgery : IDetectMethodDesignSmell
{
    public Maybe<DesignSmell> Detect(MethodModel m)
    {
        const int shortMemoryCap = 7;
        const int many = 10;

        var methodsCallingMe = m.IncomingCalls.Select(method => method.Caller).ToHashSet();
        var changingClasses = methodsCallingMe.Select(method => method.Entity).ToHashSet();

        var cm = methodsCallingMe.Count;
        var cc = changingClasses.Count;

        if (cm > shortMemoryCap && cc > many)
            return Maybe<DesignSmell>.From(
                new DesignSmell
                {
                    Name = "Shotgun Surgery",
                    Severity = CalculateSeverity(cm, cc, m),
                    SourceFile = m.Entity.FilePath,
                    Metrics = new Dictionary<string, double>
                    {
                        { "cm", cm },
                        { "cc", cc }
                    }
                });

        return Maybe<DesignSmell>.None;
    }

    private static double CalculateSeverity(int icio, int icdo, MethodModel method)
    {
        var metrics = Metrics.Metrics.For(method);
        var severityIcio = LinearNormalization.WithMeasurementRange(7, 21).ValueFor(icio);
        var severityIcdo = LinearNormalization.WithMeasurementRange(3, 9).ValueFor(icdo);

        var ocio = metrics.CouplingIntensity;
        var severityOcio = LinearNormalization.WithMeasurementRange(7, 21).ValueFor(ocio);

        var ocdo = metrics.CouplingDispersion;
        var severityOcdo = LinearNormalization.WithMeasurementRange(3, 9).ValueFor(ocdo);

        return (2 * severityIcio + 2 * severityIcdo + severityOcio + severityOcdo) / 6;
    }
}

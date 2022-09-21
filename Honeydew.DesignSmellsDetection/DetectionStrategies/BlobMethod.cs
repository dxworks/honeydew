using CSharpFunctionalExtensions;
using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.DesignSmellsDetection.DetectionStrategies;

public class BlobMethod : IDetectMethodDesignSmell
{
    public Maybe<DesignSmell> Detect(MethodModel m)
    {
        const int many = 7;
        const double cycloPerMethodHigh = 3.1;
        const int locPerClassHigh = 100;

        var loc = m.LinesOfCode.SourceLines;
        var cyclo = m.CyclomaticComplexity;
        
        // TODO: calculate nesting depth;
        //var maxNesting = m.NestingDepth;
        var metrics = Metrics.Metrics.For(m);
        var noav = metrics.NumberOfAccessedVariables;

        if (loc > locPerClassHigh / 2 && cyclo >= cycloPerMethodHigh && noav > many)
        {
            return Maybe<DesignSmell>.From(
                new DesignSmell
                {
                    Name = "Blob Method",
                    Severity = CalculateSeverity(loc, cyclo),
                    SourceFile = m.Entity.FilePath,
                    Metrics = new Dictionary<string, double>
                    {
                        { "loc", loc },
                        { "cyclo", cyclo },
                        { "noav", noav }
                    }
                });
        }

        return Maybe<DesignSmell>.None;
    }

    private static double CalculateSeverity(int loc, int cyclo)
    {
        var severityCyclo = LinearNormalization.WithMeasurementRange(3, 15).ValueFor(cyclo);
        var severityLoc = LinearNormalization.WithMeasurementRange(100, 500).ValueFor(loc);

        return (2 * severityCyclo + severityLoc) / 3;
    }
}

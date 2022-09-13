﻿using CSharpFunctionalExtensions;
using Honeydew.DesignSmellsDetection.Metrics;
using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.DesignSmellsDetection.DetectionStrategies;

public class GodClass : IDetectTypeDesignSmell
{
    private const int Few = 4;

    public Maybe<DesignSmell> Detect(ClassModel t)
    {
        var wmcVeryHigh = 47;

        var metrics = ClassMetrics.For(t);
        var atfd = metrics.AccessToForeignDataForType;
        var wmc = metrics.WeightedMethodCount;
        var tcc = metrics.TotalClassCohesion;


        if (atfd > Few && wmc >= wmcVeryHigh && tcc < CommonFractionThreshold.OneThird)
            return Maybe<DesignSmell>.From(new DesignSmell
            {
                Name = "God Class",
                Severity = CalculateSeverity(atfd),
                Metrics = new Dictionary<string, double>
                {
                    {"atfd", atfd},
                    {"wmc", wmc},
                    {"tcc", tcc}
                },
                SourceFile = t.FilePath,
                Source = t
            });

        return Maybe<DesignSmell>.None;
    }

    private static double CalculateSeverity(int atfd)
    {
        return LinearNormalization.WithMeasurementRange(Few, 20).ValueFor(atfd);
    }
}
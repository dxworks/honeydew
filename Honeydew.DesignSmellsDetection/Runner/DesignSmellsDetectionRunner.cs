using System.Diagnostics;
using Honeydew.DesignSmellsDetection.DetectionStrategies;
using Honeydew.DesignSmellsDetection.Metrics;
using Honeydew.Logging;
using DxWorks.ScriptBee.Plugins.Honeydew.Models;

namespace Honeydew.DesignSmellsDetection.Runner;

public class DesignSmellsDetectionRunner
{
    private readonly ILogger _logger;

    private readonly IList<IDetectTypeDesignSmell> _typeDesignSmellDetectionStrategies =
        new List<IDetectTypeDesignSmell>
        {
            new GodClass(), new DataClass(), new RefusedParentBequest(), new TraditionBreaker()
        };

    private readonly IList<IDetectMethodDesignSmell> _methodDesignSmellDetectionStrategies =
        new List<IDetectMethodDesignSmell>
        {
            new FeatureEnvy(),
            new BlobMethod(),
            new IntensiveCoupling(),
            new DispersedCoupling(),
            new ShotgunSurgery()
        };

    public DesignSmellsDetectionRunner(ILogger logger)
    {
        _logger = logger;
    }

    public IEnumerable<DesignSmell> Detect(RepositoryModel codeBase)
    {
        var typeDesignSmells = DetectTypeDesignSmells(codeBase);

        var groupedDesignSmells = GroupByDesignSmellAndSourceFile(typeDesignSmells);
        return groupedDesignSmells;
    }

    private static IEnumerable<DesignSmell> GroupByDesignSmellAndSourceFile(IEnumerable<DesignSmell> designSmells)
    {
        return designSmells.GroupBy(s => s.SourceFile + s.Name).Select(
            g => new DesignSmell
            {
                SourceFile = g.First().SourceFile,
                Name = g.First().Name,
                Severity = LinearNormalization.WithMeasurementRange(1, 10).ValueFor(g.Sum(d => d.Severity)),
                Metrics = g.First().Metrics // TODO: could improve by merging the metrics
            });
    }


    private static bool IsGenerated(EntityModel type)
    {
        return type.Attributes.Any(a => a.Type.Name.Contains("GeneratedCode"));
    }

    private IEnumerable<DesignSmell> DetectTypeDesignSmells(RepositoryModel codeBase)
    {
        _logger.Log("\tDetecting Design Smells in types");

        var stopWatch = Stopwatch.StartNew();

        var designSmells = new List<DesignSmell>();
        foreach (var t in codeBase.GetEnumerable())
        {
            if (t is not ClassModel classModel) continue;
            _logger.Log("\t\t" + classModel, LogLevels.Debug);

            if (string.IsNullOrWhiteSpace(t.FilePath)) continue;

            if (IsGenerated(classModel)) continue;

            foreach (var typeDesignSmellDetectionStrategy in _typeDesignSmellDetectionStrategies)
            {
                var designSmell = typeDesignSmellDetectionStrategy.Detect(classModel);
                if (designSmell.HasValue) designSmells.Add(designSmell.Value);
            }

            var methodDesignSmells = DetectMethodDesignSmells(classModel);
            designSmells.AddRange(methodDesignSmells);
        }

        stopWatch.Stop();

        _logger.Log($"\tDetected Design Smells in types in {stopWatch.ElapsedMilliseconds:000} ms");
        return designSmells;
    }

    private IEnumerable<DesignSmell> DetectMethodDesignSmells(ClassModel classModel)
    {
        var allMethods = classModel.MethodsToConsiderForDesignSmells();
        foreach (var method in allMethods)
        {
            foreach (var methodDesignSmellDetectionStrategy in _methodDesignSmellDetectionStrategies)
            {
                var designSmell = methodDesignSmellDetectionStrategy.Detect(method);
                if (designSmell.HasValue)
                {
                    yield return designSmell.Value;
                }
            }
        }
    }
}

using System.Diagnostics;
using Honeydew.Logging;
using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.DesignSmellsDetection.DetectionStrategies;

public class DesignSmellsDetectionRunner
{
    private readonly ILogger _logger;

    private readonly IList<IDetectTypeDesignSmell> _typeDesignSmellDetectionStrategies =
        new List<IDetectTypeDesignSmell>
        {
            new GodClass(), new DataClass(), new RefusedParentBequest()
        };

    public DesignSmellsDetectionRunner(ILogger logger)
    {
        _logger = logger;
    }

    public IEnumerable<DesignSmell> Detect(RepositoryModel codeBase)
    {
        var typeDesignSmells = DetectTypeDesignSmells(codeBase);

        //IEnumerable<DesignSmell> methodDesignSmells = DetectMethodDesignSmells(codeBase);

        //return typeDesignSmells.Concat(methodDesignSmells);

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
                Source = g.First().Source,
                Metrics = g.First().Metrics // TODO: could improve by merging the metrics
            });
    }


    private static bool IsGenerated(EntityModel type)
    {
        return type.Attributes.Any(a => a.Type.Name.Contains("GeneratedCode"));
    }

    //private IEnumerable<DesignSmell> DetectMethodDesignSmells(ICodeBase codeBase)
    //{
    //    var methodDesignSmells = new List<DesignSmell>();
    //    var stopWatch = Stopwatch.StartNew();
    //    _logger.Information("\tDetecting Design Smells in methods");
    //    foreach (var m in codeBase.Application.Methods)
    //    {
    //        _logger.Debug("\t\t" + m.FullName);

    //        if (string.IsNullOrWhiteSpace(m.ParentType.SourceFile()))
    //        {
    //            continue;
    //        }

    //        if (IsGenerated(codeBase, m.ParentType))
    //        {
    //            continue;
    //        }

    //        if (IsDefaultConstructorGeneratedByCompiler(m))
    //        {
    //            continue;
    //        }

    //        foreach (var methodDesignSmellDetectionStrategy in _methodDesignSmellDetectionStrategies)
    //        {
    //            Maybe<DesignSmell> designSmell = methodDesignSmellDetectionStrategy.Detect(m);
    //            if (designSmell.HasValue)
    //            {
    //                methodDesignSmells.Add(designSmell.Value);
    //            }
    //        }
    //    }

    //    stopWatch.Stop();
    //    _logger.Information(
    //        "\tDetected Design Smells in methods in {Elapsed:000} ms",
    //        stopWatch.ElapsedMilliseconds);

    //   
    //}

    private IEnumerable<DesignSmell> DetectTypeDesignSmells(RepositoryModel codeBase)
    {
        _logger.Log("\tDetecting Design Smells in types");

        var stopWatch = Stopwatch.StartNew();

        var typeDesignSmells = new List<DesignSmell>();
        foreach (var t in codeBase.GetEnumerable())
        {
            if (t is not ClassModel classModel) continue;
            _logger.Log("\t\t" + classModel, LogLevels.Debug);

            if (string.IsNullOrWhiteSpace(t.FilePath)) continue;

            if (IsGenerated(classModel)) continue;

            foreach (var typeDesignSmellDetectionStrategy in _typeDesignSmellDetectionStrategies)
            {
                var designSmell = typeDesignSmellDetectionStrategy.Detect(classModel);
                if (designSmell.HasValue) typeDesignSmells.Add(designSmell.Value);
            }
        }

        stopWatch.Stop();

        _logger.Log($"\tDetected Design Smells in types in {stopWatch.ElapsedMilliseconds:000} ms");
        return typeDesignSmells;
    }
}

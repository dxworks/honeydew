using CSharpFunctionalExtensions;
using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.DesignSmellsDetection.DetectionStrategies;

public abstract class ClassificationDesignSmellDetectionStrategy : IDetectTypeDesignSmell
{
    public Maybe<DesignSmell> Detect(ClassModel t)
    {
        return ShouldSkip(t) ? Maybe<DesignSmell>.None : DetectCore(t);
    }

    protected abstract Maybe<DesignSmell> DetectCore(ClassModel type);

    private static bool ShouldSkip(ClassModel type)
    {
        return !HasParent(type);
    }

    private static bool HasParent(ClassModel t)
    {
        return t.BaseClass != null && t.BaseClass.Name != "object";
    }
}
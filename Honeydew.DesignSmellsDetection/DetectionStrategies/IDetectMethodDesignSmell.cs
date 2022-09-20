using CSharpFunctionalExtensions;
using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.DesignSmellsDetection.DetectionStrategies;

public interface IDetectMethodDesignSmell
{
    Maybe<DesignSmell> Detect(MethodModel m);
}
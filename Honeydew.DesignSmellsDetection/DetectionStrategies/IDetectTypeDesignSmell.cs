using CSharpFunctionalExtensions;
using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.DesignSmellsDetection.DetectionStrategies;

public interface IDetectTypeDesignSmell
{
    Maybe<DesignSmell> Detect(ClassModel t);
}

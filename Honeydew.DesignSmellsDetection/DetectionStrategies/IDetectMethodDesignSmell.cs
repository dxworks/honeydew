using CSharpFunctionalExtensions;
using DxWorks.ScriptBee.Plugins.Honeydew.Models;

namespace Honeydew.DesignSmellsDetection.DetectionStrategies;

public interface IDetectMethodDesignSmell
{
    Maybe<DesignSmell> Detect(MethodModel m);
}

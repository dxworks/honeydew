using CSharpFunctionalExtensions;
using DxWorks.ScriptBee.Plugins.Honeydew.Models;

namespace Honeydew.DesignSmellsDetection.DetectionStrategies;

public interface IDetectTypeDesignSmell
{
    Maybe<DesignSmell> Detect(ClassModel t);
}

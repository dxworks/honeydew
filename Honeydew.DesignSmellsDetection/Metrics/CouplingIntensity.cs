﻿using DxWorks.ScriptBee.Plugins.Honeydew.Models;

namespace Honeydew.DesignSmellsDetection.Metrics;

public static class CouplingIntensity
{
    public static int Value(MethodModel method)
    {
        return method.CoupledMethodsAndProperties().Count();
    }
}

﻿using DxWorks.ScriptBee.Plugins.Honeydew.Models;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;

namespace DxWorks.ScriptBee.Plugins.Honeydew.Loaders;

public static class MetricAdder
{
    public static void AddMetrics(FileModel fileModel, ITypeWithMetrics typeWithMetrics)
    {
        foreach (var metricModel in typeWithMetrics.Metrics)
        {
            if (string.IsNullOrEmpty(metricModel.Name))
            {
                continue;
            }

            if (CSharpConstants.IsNumericType(metricModel.ValueType))
            {
                fileModel.Metrics[metricModel.Name] = (int)metricModel.Value;
            }
            else
            {
                fileModel[metricModel.Name] = metricModel.Value;
            }
        }
    }

    public static void AddMetrics(EntityModel entityModel, ITypeWithMetrics typeWithMetrics)
    {
        foreach (var metricModel in typeWithMetrics.Metrics)
        {
            if (string.IsNullOrEmpty(metricModel.Name))
            {
                continue;
            }

            if (CSharpConstants.IsNumericType(metricModel.ValueType))
            {
                entityModel.Metrics[metricModel.Name] = (int)metricModel.Value;
            }
            else
            {
                entityModel[metricModel.Name] = metricModel.Value;
            }
        }
    }

    public static void AddMetrics(FieldModel fieldModel, ITypeWithMetrics typeWithMetrics)
    {
        foreach (var metricModel in typeWithMetrics.Metrics)
        {
            if (string.IsNullOrEmpty(metricModel.Name))
            {
                continue;
            }

            if (CSharpConstants.IsNumericType(metricModel.ValueType))
            {
                fieldModel.Metrics[metricModel.Name] = (int)metricModel.Value;
            }
            else
            {
                fieldModel[metricModel.Name] = metricModel.Value;
            }
        }
    }


    public static void AddMetrics(MethodModel methodModel, ITypeWithMetrics typeWithMetrics)
    {
        foreach (var metricModel in typeWithMetrics.Metrics)
        {
            if (string.IsNullOrEmpty(metricModel.Name))
            {
                continue;
            }

            if (CSharpConstants.IsNumericType(metricModel.ValueType))
            {
                methodModel.Metrics[metricModel.Name] = (long)metricModel.Value;
            }
            else
            {
                methodModel[metricModel.Name] = metricModel.Value;
            }
        }
    }
}

using System.Collections.Generic;
using Honeydew.PostExtraction.ReferenceRelations;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;

namespace Honeydew.Processors;

internal static class RelationVisitorPrettyPrint
{
    public static readonly Dictionary<string, string> PrettyPrint = new()
    {
        { nameof(DeclarationRelationVisitor), DeclarationRelationVisitor.DeclarationsMetricName },
        { nameof(ExceptionsThrownRelationVisitor), ExceptionsThrownRelationVisitor.ExceptionsThrownDependencyMetricName },
        { nameof(ExternCallsRelationVisitor), ExternCallsRelationVisitor.ExtCallsMetricName },
        { nameof(ExternDataRelationVisitor), ExternDataRelationVisitor.ExtDataMetricName },
        { nameof(FieldsRelationVisitor), FieldsRelationVisitor.FieldsDependencyMetricName },
        { nameof(HierarchyRelationVisitor), HierarchyRelationVisitor.HierarchyMetricName },
        { nameof(LocalVariablesRelationVisitor), LocalVariablesRelationVisitor.LocalVariablesDependencyMetricName },
        { nameof(ObjectCreationRelationVisitor), ObjectCreationRelationVisitor.ObjectCreationDependencyMetricName },
        { nameof(ParameterRelationVisitor), ParameterRelationVisitor.ParameterDependencyMetricName },
        { nameof(PropertiesRelationVisitor), PropertiesRelationVisitor.PropertiesDependencyMetricName },
        { nameof(ReturnValueRelationVisitor), ReturnValueRelationVisitor.ReturnsMetricName },
    };
}

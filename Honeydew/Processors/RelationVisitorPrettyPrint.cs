using System.Collections.Generic;
using Honeydew.PostExtraction.ReferenceRelations;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;

namespace Honeydew.Processors;

internal static class RelationVisitorPrettyPrint
{
    public static readonly Dictionary<string, string> PrettyPrint = new()
    {
        { nameof(DeclarationRelationVisitor), new DeclarationRelationVisitor().PrettyPrint() },
        { nameof(ExceptionsThrownRelationVisitor), new ExceptionsThrownRelationVisitor().PrettyPrint() },
        { nameof(ExternCallsRelationVisitor), new ExternCallsRelationVisitor().PrettyPrint() },
        { nameof(ExternDataRelationVisitor), new ExternDataRelationVisitor().PrettyPrint() },
        { nameof(FieldsRelationVisitor), new FieldsRelationVisitor().PrettyPrint() },
        { nameof(HierarchyRelationVisitor), new HierarchyRelationVisitor().PrettyPrint() },
        { nameof(LocalVariablesRelationVisitor), new LocalVariablesRelationVisitor().PrettyPrint() },
        { nameof(ObjectCreationRelationVisitor), new ObjectCreationRelationVisitor().PrettyPrint() },
        { nameof(ParameterRelationVisitor), new ParameterRelationVisitor().PrettyPrint() },
        { nameof(PropertiesRelationVisitor), new PropertiesRelationVisitor().PrettyPrint() },
        { nameof(ReturnValueRelationVisitor), new ReturnValueRelationVisitor().PrettyPrint() },
    };
}

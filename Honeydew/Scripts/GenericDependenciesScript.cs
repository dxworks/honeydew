using System.Collections.Generic;
using System.IO;
using System.Linq;
using HoneydewCore.IO.Writers.Exporters;
using HoneydewCore.ModelRepresentations;
using HoneydewCore.Utils;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class.ReferenceRelations;
using HoneydewModels.Reference;

namespace Honeydew.Scripts;

/// <summary>
/// Requires the following arguments:
/// <list type="bullet">
///     <item>
///         <description>outputPath</description>
///     </item>
///     <item>
///         <description>referenceRepositoryModel</description>
///     </item>
///     <item>
///         <description>genericDependenciesOutputName</description>
///     </item>
///     <item>
///         <description>addStrategy</description>
///     </item>
///     <item>
///         <description>ignorePrimitives</description>
///     </item>
/// </list>
/// </summary>
public class GenericDependenciesScript : Script
{
    private readonly CsvRelationsRepresentationExporter _representationExporter;

    public GenericDependenciesScript(CsvRelationsRepresentationExporter representationExporter)
    {
        _representationExporter = representationExporter;
    }

    public override void Run(Dictionary<string, object> arguments)
    {
        var outputPath = VerifyArgument<string>(arguments, "outputPath");
        var repositoryModel = VerifyArgument<RepositoryModel>(arguments, "referenceRepositoryModel");
        var outputName = VerifyArgument<string>(arguments, "genericDependenciesOutputName");
        var addStrategy = VerifyArgument<IAddStrategy>(arguments, "addStrategy");
        var ignorePrimitives = VerifyArgument<bool>(arguments, "ignorePrimitives");

        var relationsRepresentation = new RelationsRepresentation();

        var visitors = new List<IReferenceModelVisitor>
        {
            new DeclarationRelationVisitor(
                new LocalVariablesRelationVisitor(addStrategy),
                new ParameterRelationVisitor(addStrategy),
                new FieldsAndPropertiesRelationVisitor(addStrategy)),
            new ReturnValueRelationVisitor(addStrategy),
            new ExternCallsRelationVisitor(addStrategy),
            new ExternDataRelationVisitor(addStrategy),
            new HierarchyRelationVisitor(addStrategy),
        };

        var relations = new List<Relation>();

        foreach (var project in repositoryModel.Projects)
        {
            foreach (var file in project.Files)
            {
                foreach (var classModel in file.Classes)
                {
                    foreach (var visitor in visitors)
                    {
                        var dependencies = visitor.Visit(classModel);
                        var relationEnumerable = dependencies.Where(pair => pair.Key != classModel.Name);
                        if (ignorePrimitives)
                        {
                            relationEnumerable = relationEnumerable.Where(pair =>
                                !CSharpConstants.IsPrimitive(pair.Key) &&
                                !CSharpConstants.IsPrimitiveArray(pair.Key));
                        }

                        relations.AddRange(relationEnumerable.Select(targetCountPair => new Relation
                        {
                            Source = classModel.Name,
                            Target = targetCountPair.Key,
                            Type = visitor.PrettyPrint(),
                            Strength = targetCountPair.Value
                        }));
                    }
                }
            }
        }

        foreach (var relation in relations)
        {
            relationsRepresentation.Add(relation);
        }

        _representationExporter.Export(Path.Combine(outputPath, outputName), relationsRepresentation);
    }
}

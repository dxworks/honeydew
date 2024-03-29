﻿using Honeydew.IO.Writers.Exporters;
using Honeydew.ModelRepresentations;
using Honeydew.PostExtraction.ReferenceRelations;
using DxWorks.ScriptBee.Plugins.Honeydew.Models;

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
/// </list>
/// </summary>
public class GenericDependenciesScript : Script
{
    private readonly CsvRelationsRepresentationExporter _representationExporter;

    public GenericDependenciesScript(CsvRelationsRepresentationExporter representationExporter)
    {
        _representationExporter = representationExporter;
    }

    public override void Run(Dictionary<string, object?> arguments)
    {
        var outputPath = VerifyArgument<string>(arguments, "outputPath")!;
        var repositoryModel = VerifyArgument<RepositoryModel>(arguments, "referenceRepositoryModel")!;
        var outputName = VerifyArgument<string>(arguments, "genericDependenciesOutputName")!;
        var addStrategy = VerifyArgument<IAddStrategy>(arguments, "addStrategy")!;

        var relationsRepresentation = new RelationsRepresentation();

        var visitors = new List<IEntityModelVisitor>
        {
            new DeclarationRelationVisitor(
                addStrategy,
                new LocalVariablesRelationVisitor(addStrategy),
                new ParameterRelationVisitor(addStrategy),
                new FieldsRelationVisitor(addStrategy),
                new PropertiesRelationVisitor(addStrategy)),
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
                foreach (var entityModel in file.Entities)
                {
                    foreach (var visitor in visitors)
                    {
                        var dictionary = visitor.Visit(entityModel);

                        var relationEnumerable = dictionary.Where(pair => pair.Key != entityModel.Name);

                        relations.AddRange(relationEnumerable.Select(targetCountPair =>
                            new Relation(entityModel.Name, targetCountPair.Key, visitor.Name, targetCountPair.Value)));
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

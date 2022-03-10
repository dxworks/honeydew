using System.Collections.Generic;
using System.IO;
using System.Linq;
using Honeydew.PostExtraction.ReferenceRelations;
using HoneydewCore.IO.Writers.Exporters;
using HoneydewCore.ModelRepresentations;
using HoneydewScriptBeePlugin.Models;

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

    public override void Run(Dictionary<string, object> arguments)
    {
        var outputPath = VerifyArgument<string>(arguments, "outputPath");
        var repositoryModel = VerifyArgument<RepositoryModel>(arguments, "referenceRepositoryModel");
        var outputName = VerifyArgument<string>(arguments, "genericDependenciesOutputName");
        var addStrategy = VerifyArgument<IAddStrategy>(arguments, "addStrategy");

        var relationsRepresentation = new RelationsRepresentation();

        var visitors = new List<IReferenceModelVisitor>
        {
            new DeclarationRelationVisitor(
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
                        visitor.Visit(entityModel);
                    }

                    foreach (var (metricName, value) in entityModel.GetProperties())
                    {
                        if (value is not Dictionary<string, int> dictionary)
                        {
                            continue;
                        }

                        var relationEnumerable = dictionary.Where(pair => pair.Key != entityModel.Name);
                        
                        relations.AddRange(relationEnumerable.Select(targetCountPair => new Relation
                        {
                            Source = entityModel.Name,
                            Target = targetCountPair.Key,
                            Type = metricName,
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

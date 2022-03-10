using System.Collections.Generic;
using System.IO;
using System.Linq;
using HoneydewCore.IO.Writers.Exporters;
using HoneydewCore.ModelRepresentations;
using HoneydewScriptBeePlugin.Models;

namespace Honeydew.Scripts;

public class ClassRelationScript : Script
{
    private readonly CsvRelationsRepresentationExporter _representationExporter;

    public ClassRelationScript(CsvRelationsRepresentationExporter representationExporter)
    {
        _representationExporter = representationExporter;
    }

    public override void Run(Dictionary<string, object> arguments)
    {
        var outputPath = VerifyArgument<string>(arguments, "outputPath");
        var outputName = VerifyArgument<string>(arguments, "classRelationsOutputName");
        var repositoryModel = VerifyArgument<RepositoryModel>(arguments, "referenceRepositoryModel");

        var relationsRepresentation = new RelationsRepresentation();

        var relations = repositoryModel.Projects.SelectMany(project => project.Files)
            .SelectMany(file => file.Entities).SelectMany(entityModel => ExtCallsRelations(entityModel)
                .Concat(new List<Relation>())
            );

        foreach (var relation in relations)
        {
            relationsRepresentation.Add(relation);
        }

        _representationExporter.Export(Path.Combine(outputPath, outputName), relationsRepresentation);

        IEnumerable<Relation> ExtCallsRelations(EntityModel entityModel)
        {
            var methods = entityModel switch
            {
                ClassModel classModel => classModel.Methods
                    .Concat(classModel.Properties.SelectMany(property => property.Accessors))
                    .Concat(classModel.Constructors)
                    .Concat(classModel.Destructor == null
                        ? new List<MethodModel>()
                        : new List<MethodModel>
                        {
                            classModel.Destructor
                        }),
                InterfaceModel interfaceModel => interfaceModel.Methods
                    .Concat(interfaceModel.Properties.SelectMany(property => property.Accessors)),
                _ => new List<MethodModel>()
            };

            return methods.SelectMany(method => method.OutgoingCalls)
                .Where(method => method.Caller is { Entity: { } })
                .GroupBy(method => method.Caller.Entity.Name)
                .Where(grouping => entityModel.Name != grouping.Key)
                .Select(grouping => new Relation
                {
                    Source = entityModel.Name,
                    Target = grouping.Key,
                    Type = "extCalls",
                    Strength = grouping.Count()
                });
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using HoneydewCore.IO.Writers.Exporters;
using HoneydewCore.ModelRepresentations;
using HoneydewModels.Reference;

namespace Honeydew.Scripts
{
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
                .SelectMany(file => file.Classes).SelectMany(classModel => ExtCallsRelations(classModel)
                    .Concat(new List<Relation>())
                );

            foreach (var relation in relations)
            {
                relationsRepresentation.Add(relation);
            }

            _representationExporter.Export(Path.Combine(outputPath, outputName), relationsRepresentation);

            IEnumerable<Relation> ExtCallsRelations(ClassModel classModel)
            {
                return classModel.Methods.SelectMany(method => method.CalledMethods)
                    .Where(method => method.Class != null)
                    .GroupBy(method => method.Class.Name)
                    .Where(grouping => classModel.Name != grouping.Key)
                    .Select(grouping => new Relation
                    {
                        Source = classModel.Name,
                        Target = grouping.Key,
                        Type = "extCalls",
                        Strength = grouping.Count()
                    });
            }
        }
    }
}

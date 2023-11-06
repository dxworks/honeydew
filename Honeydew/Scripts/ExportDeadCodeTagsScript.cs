using DxWorks.ScriptBee.Plugins.Honeydew.Models;
using Honeydew.Extractors.Exporters;

namespace Honeydew.Scripts;

public class ExportDeadCodeTagsScript : Script
{
    private readonly JsonModelExporter _modelExporter;

    public ExportDeadCodeTagsScript(JsonModelExporter modelExporter)
    {
        _modelExporter = modelExporter;
    }

    public override void Run(Dictionary<string, object?> arguments)
    {
        var outputPath = VerifyArgument<string>(arguments, "outputPath")!;
        var outputName = VerifyArgument<string>(arguments, "deadCodeOutputName")!;
        var repositoryModel = VerifyArgument<RepositoryModel>(arguments, "referenceRepositoryModel")!;

        var filesFromUnprocessedProjects = repositoryModel.UnprocessedProjects.SelectMany(up => up.Files)
            .Select(f => f.FilePath).ToHashSet()
            .Select(fp => new Tag(fp, "codebaseorganisation.deadcode.unreferencedprojects")).ToList();

        var filesFromUnprocessedSourceFiles = repositoryModel.UnprocessedSourceFiles.SelectMany(up => up.Files)
            .Select(f => f.FilePath).ToHashSet()
            .Select(fp => new Tag(fp, "codebaseorganisation.deadcode.unreferencedfiles")).ToList();

        var tags = filesFromUnprocessedSourceFiles.Concat(filesFromUnprocessedProjects);

        _modelExporter.Export(Path.Combine(outputPath, outputName), new Output
        {
            file = new Classifiers
            {
                classifiers = tags.ToArray()
            }
        });
    }

    internal class Output
    {
        public Classifiers file { get; set; }
    }

    internal class Classifiers
    {
        public Tag[] classifiers { get; set; }
    }

    internal class Tag
    {
        public Tag(string file, string tag)
        {
            entity = file;
            this.tag = tag;
        }

        public string entity { get; }

        public string tag { get; }
    }
}

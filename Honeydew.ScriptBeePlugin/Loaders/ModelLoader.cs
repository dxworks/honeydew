using DxWorks.ScriptBee.Plugin.Api;
using Honeydew.Models;
using Honeydew.ScriptBeePlugin.Converters;
using Honeydew.ScriptBeePlugin.Models;
using RepositoryModel = Honeydew.Models.RepositoryModel;

namespace Honeydew.ScriptBeePlugin.Loaders;

public class ModelLoader : IModelLoader
{
    private const string CSharp = "C#";
    private const string VisualBasic = "Visual Basic";

    public async Task<Dictionary<string, Dictionary<string, ScriptBeeModel>>> LoadModel(List<Stream> fileStreams,
        Dictionary<string, object>? configuration = default, CancellationToken cancellationToken = default)
    {
        var progressLogger = new NoBarsProgressLogger();

        var repositoryDictionary = new Dictionary<string, ScriptBeeModel>();
        var projectsDictionary = new Dictionary<string, ScriptBeeModel>();
        var classDictionary = new Dictionary<string, ScriptBeeModel>();

        foreach (var stream in fileStreams)
        {
            var guid = Guid.NewGuid();

            progressLogger.Log($"Loading model {guid} from stream");
            progressLogger.Log();

            RepositoryModel? repositoryModel = null;
            try
            {
                repositoryModel = await LoadModel(stream, cancellationToken);
            }
            catch (Exception e)
            {
                progressLogger.Log($"Failed to load model {guid} from stream  because {e}");
            }

            if (repositoryModel is null)
            {
                progressLogger.Log($"Skipping model loading {guid}");
                continue;
            }

            var referenceRepositoryModel =
                new RepositoryModelToReferenceRepositoryModelProcessor(new EmptyLogger(), progressLogger).Process(
                    repositoryModel);

            repositoryDictionary.Add(guid.ToString(), referenceRepositoryModel);
            foreach (var projectModel in referenceRepositoryModel.Projects)
            {
                projectsDictionary.Add($"{guid}|{projectModel.Name}", projectModel);
                foreach (var entityModel in projectModel.Files.SelectMany(file => file.Entities))
                {
                    var entityId = entityModel switch
                    {
                        ClassModel classModel =>
                            $"{guid}|{projectModel.Name}|{entityModel.Name}|{classModel.GenericParameters.Count}",
                        InterfaceModel interfaceModel =>
                            $"{guid}|{projectModel.Name}|{entityModel.Name}|{interfaceModel.GenericParameters.Count}",
                        _ => $"{guid}|{projectModel.Name}|{entityModel.Name}"
                    };

                    if (classDictionary.ContainsKey(entityId))
                    {
                        progressLogger.Log("Duplicate entity found");
                    }
                    else
                    {
                        classDictionary.Add(entityId, entityModel);
                    }
                }
            }
        }

        progressLogger.Log("Done Loading");

        return new Dictionary<string, Dictionary<string, ScriptBeeModel>>
        {
            { "Repository", repositoryDictionary },
            { "Project", projectsDictionary },
            { "Class", classDictionary }
        };
    }

    public string GetName()
    {
        return "honeydew";
    }

    private static async Task<RepositoryModel?> LoadModel(Stream inputStream, CancellationToken cancellationToken)
    {
        var cSharpConverterList = new CSharpConverterList();

        var repositoryModelImporter = new RepositoryModelImporter(new ProjectModelConverter(
            new Dictionary<string, IConverterList>
            {
                { CSharp, cSharpConverterList },
                { VisualBasic, new VisualBasicConverterList() },
            }, cSharpConverterList));

        return await repositoryModelImporter.Import(inputStream, cancellationToken);
    }
}

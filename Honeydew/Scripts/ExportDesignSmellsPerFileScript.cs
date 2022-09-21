using Honeydew.DesignSmellsDetection.Runner;
using Honeydew.Extractors.Exporters;
using Honeydew.Logging;
using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.Scripts;

public class ExportDesignSmellsPerFileScript : Script
{
    private readonly JsonModelExporter _modelExporter;
    private readonly ILogger _logger;

    public ExportDesignSmellsPerFileScript(JsonModelExporter modelExporter, ILogger logger)
    {
        _modelExporter = modelExporter;
        _logger = logger;
    }

    public override void Run(Dictionary<string, object?> arguments)
    {
        var outputPath = VerifyArgument<string>(arguments, "outputPath")!;
        var outputName = VerifyArgument<string>(arguments, "designSmellsOutputName")!;
        var repositoryModel = VerifyArgument<RepositoryModel>(arguments, "referenceRepositoryModel")!;

        // #1 The base type of CategoryRepository is BaseRepository<EntityT, IdT>, which should be marked as internal
        var entityModel = repositoryModel.GetEnumerable().SingleOrDefault(e => e.Name.Contains("NetCoreCMS.EasyNews.Repositories.CategoryRepository"));
        var classModel = entityModel as ClassModel;
        var baseType = classModel.BaseTypes.SingleOrDefault();
        if (!baseType.IsInternal)
        {
            //should be internal
            var entityIsInternal = baseType.Entity.IsInternal; // also false
        }

        // #2 The NccRazorPage.FireEvent method uses a _mediator instance variable. But the FieldAccess.Field.Entity is different than the method's Entity.
        entityModel = repositoryModel.GetEnumerable().SingleOrDefault(e => e.Name.Contains("NccRazorPage"));
        classModel = entityModel as ClassModel;
        var methodModel = classModel.Methods.SingleOrDefault(m => m.Name.Contains("FireEvent"));
        var fieldAccess = methodModel.FieldAccesses.FirstOrDefault(fa => fa.Field.Name == "_mediator");

        if (methodModel.Entity != fieldAccess.Field.Entity)
        {
            // they should be the same, as the field is defined in the same class as the method
            var classModelName = classModel.Name; //NetCoreCMS.Framework.Core.Mvc.Views.NccRazorPage<TModel>
            var accessEntityType = fieldAccess.AccessEntityType.Name; //NetCoreCMS.Framework.Core.Mvc.Views.NccRazorPage<TModel>
            var fieldAccessFieldEntity = fieldAccess.Field.Entity.Name; //NetCoreCMS.Framework.Core.Mvc.Views.NccRazorPage
            var methodEntity = methodModel.Entity.Name; //NetCoreCMS.Framework.Core.Mvc.Views.NccRazorPage<TModel>
        }

        var designSmells = new DesignSmellsDetectionRunner(_logger).Detect(repositoryModel).OrderBy(ds => ds.SourceFile);

        DesignSmellsJsonWriter.Export(_modelExporter, designSmells, Path.Combine(outputPath, outputName));
    }
}
namespace Honeydew.Extractors.Load;

public record SolutionSchema(string Extension, ISolutionExtractor SolutionExtractor,
    List<ProjectSchema> ProjectSchemas);

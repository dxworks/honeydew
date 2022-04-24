namespace Honeydew.Extractors.Load;

public record ProjectSchema(string Extension, IProjectExtractor ProjectExtractor, List<FileSchema> FileSchemas);

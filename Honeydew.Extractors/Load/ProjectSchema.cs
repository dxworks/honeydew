namespace Honeydew.Extractors.Load;

public record ProjectSchema(string Language, string Extension, IProjectExtractor ProjectExtractor,
    List<FileSchema> FileSchemas);

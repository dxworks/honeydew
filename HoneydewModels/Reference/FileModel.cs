using System.Collections.Generic;

namespace HoneydewModels.Reference;

public class FileModel : ReferenceEntity
{
    public string FilePath { get; set; }

    public IList<EntityModel> Entities { get; set; } = new List<EntityModel>();

    public IList<ImportModel> Imports { get; set; } = new List<ImportModel>();

    public ProjectModel Project { get; set; }

    public LinesOfCode Loc { get; set; }


    public IDictionary<string, int> Metrics { get; set; } = new Dictionary<string, int>();
}

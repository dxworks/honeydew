using System.Collections.Generic;
using System.Linq;

namespace HoneydewModels.Reference;

public class FileModel : ReferenceEntity
{
    public string FilePath { get; set; }

    public IList<ClassModel> Classes { get; set; } = new List<ClassModel>();

    public IList<DelegateModel> Delegates { get; set; } = new List<DelegateModel>();

    public IList<ImportModel> Imports { get; set; } = new List<ImportModel>();

    public LinesOfCode Loc { get; set; }

    public IList<MetricModel> Metrics { get; init; } = new List<MetricModel>();

    public ProjectModel Project { get; set; }

    public IEnumerable<ClassOption> ClassOptions =>
        Classes.Select<ClassModel, ClassOption>(@class => new ClassOption.Class(@class))
            .Concat(Delegates.Select(@delegate => new ClassOption.Delegate(@delegate)));
}

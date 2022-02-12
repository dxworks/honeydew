using System.Collections.Generic;

namespace HoneydewModels.Reference
{
    public class DelegateModel : ReferenceEntity
    {
        public string FilePath { get; set; }

        public string Name { get; set; }
        
        public EntityType Type { get; set; }

        public string ClassType { get; set; }

        public NamespaceModel Namespace { get; set; }

        public FileModel File { get; set; }

        public string AccessModifier { get; set; }

        public string Modifier { get; set; }

        public IList<GenericParameterModel> GenericParameters { get; set; } = new List<GenericParameterModel>();

        public IList<ClassModel> BaseTypes { get; set; } = new List<ClassModel>();

        public IList<ImportModel> Imports { get; set; } = new List<ImportModel>();

        public IList<AttributeModel> Attributes { get; set; } = new List<AttributeModel>();

        public IList<ParameterModel> Parameters { get; set; } = new List<ParameterModel>();

        public ReturnValueModel ReturnValue { get; set; }

        public IList<MetricModel> Metrics { get; init; } = new List<MetricModel>();
    }
}

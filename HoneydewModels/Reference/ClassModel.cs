using System.Collections.Generic;

namespace HoneydewModels.Reference
{
    public class ClassModel : ReferenceEntity
    {
        public string ClassType { get; set; }

        public string Name { get; set; }
        
        public string FilePath { get; set; }

        public NamespaceModel Namespace { get; set; }

        public FileModel File { get; set; }

        public string AccessModifier { get; set; }

        public string Modifier { get; set; } = "";
     
        public IList<GenericParameterModel> GenericParameters { get; set; } = new List<GenericParameterModel>();
        
        public IList<ClassModel> BaseTypes { get; set; } = new List<ClassModel>();

        public IList<ImportModel> Imports { get; set; } = new List<ImportModel>();

        public IList<FieldModel> Fields { get; set; } = new List<FieldModel>();

        public IList<PropertyModel> Properties { get; set; } = new List<PropertyModel>();

        public IList<ConstructorModel> Constructors { get; set; } = new List<ConstructorModel>();

        public IList<MethodModel> Methods { get; set; } = new List<MethodModel>();

        public IList<AttributeModel> Attributes { get; set; } = new List<AttributeModel>();

        public LinesOfCode Loc { get; set; }
        
        public IList<MetricModel> Metrics { get; init; } = new List<MetricModel>();
    }
}

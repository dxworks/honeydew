using System;
using System.Collections.Generic;
using HoneydewModels.Types;

namespace HoneydewModels.CSharp
{
    public record ClassModel : IPropertyMembersClassType, IModelEntity
    {
        public string ClassType { get; set; }

        public string Name { get; set; }

        public string FilePath { get; set; }

        public LinesOfCode Loc { get; set; }

        public string AccessModifier { get; set; }

        public string Modifier { get; set; } = "";


        private string _namespace = "";

        public string ContainingTypeName
        {
            get
            {
                if (!string.IsNullOrEmpty(_namespace) || string.IsNullOrEmpty(Name))
                {
                    return _namespace;
                }

                var lastIndexOf = Name.LastIndexOf(".", StringComparison.Ordinal);
                if (lastIndexOf < 0)
                {
                    return "";
                }

                _namespace = Name[..lastIndexOf];
                return _namespace;
            }
            set => _namespace = value;
        }

        public IList<IBaseType> BaseTypes { get; set; } = new List<IBaseType>();

        public IList<IImportType> Imports { get; set; } = new List<IImportType>();

        public IList<IFieldType> Fields { get; init; } = new List<IFieldType>();

        public IList<IPropertyType> Properties { get; set; } = new List<IPropertyType>();

        public IList<IConstructorType> Constructors { get; init; } = new List<IConstructorType>();

        public IList<IMethodType> Methods { get; init; } = new List<IMethodType>();

        public IList<MetricModel> Metrics { get; init; } = new List<MetricModel>();

        public IList<IAttributeType> Attributes { get; set; } = new List<IAttributeType>();
    }
}

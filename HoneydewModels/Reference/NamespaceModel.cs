using System.Collections.Generic;

namespace HoneydewModels.Reference
{
    public class NamespaceModel : ReferenceEntity
    {
        public string FullName { get; set; } = "";

        public string Name { get; set; } = "";

        public NamespaceModel Parent { get; set; }

        public IList<NamespaceModel> ChildNamespaces { get; set; } = new List<NamespaceModel>();

        public IList<ClassModel> Classes { get; set; } = new List<ClassModel>();

        public IList<DelegateModel> Delegates { get; set; } = new List<DelegateModel>();
    }
}

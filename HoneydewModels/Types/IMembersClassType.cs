using System.Collections.Generic;

namespace HoneydewModels.Types
{
    public interface IMembersClassType : IClassType, ITypeWithLinesOfCode
    {
        public IList<IFieldType> Fields { get; init; }

        public IList<IConstructorType> Constructors { get; init; }

        public IList<IMethodType> Methods { get; init; }
    }
}

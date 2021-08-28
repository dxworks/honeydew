using System;
using HoneydewModels.CSharp;
using HoneydewModels.Types;

namespace HoneydewModels.Converters
{
    public class CSharpClassTypeConverter : ITypeConverter<IClassType>
    {
        public Type Convert(string type)
        {
            return type == "delegate" ? typeof(DelegateModel) : typeof(ClassModel);
        }

        public Type DefaultType()
        {
            return typeof(ClassModel);
        }

        public object Convert(IClassType type)
        {
            if (type is DelegateModel delegateModel)
            {
                return delegateModel;
            }

            return (ClassModel)type;
        }
    }
}

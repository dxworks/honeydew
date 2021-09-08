using HoneydewModels.CSharp;
using HoneydewModels.Types;

namespace HoneydewModels.Converters
{
    public class CSharpClassTypeConverter : ITypeConverter<IClassType>
    {
        IClassType ITypeConverter<IClassType>.Convert(string type)
        {
            return type == "delegate" ? new DelegateModel() : new ClassModel();
        }

        public IClassType Convert(object value)
        {
            {
                if (value is DelegateModel delegateModel)
                {
                    return delegateModel;
                }

                return (ClassModel)value;
            }
        }
    }
}

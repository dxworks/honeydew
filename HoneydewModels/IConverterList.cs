﻿using System.Collections.Generic;
using System.Text.Json.Serialization;
using HoneydewModels.CSharp;
using HoneydewModels.Types;

namespace HoneydewModels
{
    public interface IConverterList
    {
        IList<JsonConverter> GetConverters();
    }

    public class ConverterList : IConverterList
    {
        public IList<JsonConverter> GetConverters()
        {
            return new List<JsonConverter>
            {
                new ModelJsonConverter<IParameterType, ParameterModel>(),
                new ModelJsonConverter<IBaseType, BaseTypeModel>(),
                new ModelJsonConverter<IConstructorType, ConstructorModel>(),
                new ModelJsonConverter<IMethodType, MethodModel>(),
                new ModelJsonConverter<IClassType, ClassModel>(),
                new ModelJsonConverter<IFieldType, FieldModel>(),
                new ModelJsonConverter<IPropertyType, PropertyModel>(),
                new ModelJsonConverter<IReturnType, ReturnTypeModel>(),
                new ModelJsonConverter<IImportType, UsingModel>(),
                new ModelJsonConverter<IMethodSignatureType, MethodCallModel>(),
            };
        }
    }
}
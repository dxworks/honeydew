using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Fields;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Metrics.Extraction.Class;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnit;
using HoneydewExtractors.CSharp.Metrics.Extraction.Field;
using HoneydewExtractors.CSharp.Metrics.Extraction.Property;
using HoneydewModels.CSharp;
using Xunit;

namespace HoneydewExtractorsTests.CSharp.Metrics.Extraction.Property
{
    public class CSharpGenericPropertyTests
    {
        private readonly CSharpFactExtractor _factExtractor;

        public CSharpGenericPropertyTests()
        {
            var compositeVisitor = new CompositeVisitor();

            compositeVisitor.Add(new ClassSetterCompilationUnitVisitor(new List<ICSharpClassVisitor>
            {
                new BaseInfoClassVisitor(),
                new PropertySetterClassVisitor(new List<ICSharpPropertyVisitor>
                {
                    new PropertyInfoVisitor()
                })
            }));

            _factExtractor = new CSharpFactExtractor(new CSharpSyntacticModelCreator(),
                new CSharpSemanticModelCreator(new CSharpCompilationMaker()), compositeVisitor);
        }

        [Theory]
        [InlineData("class")]
        [InlineData("record")]
        [InlineData("struct")]
        public void Extract_ShouldHaveFieldOfGenericType_WhenProvidedDifferentClassType(string classType)
        {
            var fileContent = $@"namespace Namespace1
{{
    public {classType} Class1 
    {{       
        public GenericClass<string> GField {{get;set;}} 

        public event System.Func<int> FField {{add{{}} remove{{}}}}
    }}

    public class GenericClass<T> {{}}
}}";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal(2, classModel.Properties.Count);

            var property1 = classModel.Properties[0];
            Assert.Equal("Namespace1.GenericClass<string>", property1.Type.Name);
            Assert.Equal(1, property1.Type.ContainedTypes.Count);
            Assert.Equal("Namespace1.GenericClass", property1.Type.ContainedTypes[0].Name);
            Assert.Empty(property1.Type.ContainedTypes[0].Constrains);
            Assert.Equal(1, property1.Type.ContainedTypes[0].ContainedTypes.Count);
            Assert.Equal("string", property1.Type.ContainedTypes[0].ContainedTypes[0].Name);
            Assert.Empty(property1.Type.ContainedTypes[0].ContainedTypes[0].Constrains);
            Assert.Empty(property1.Type.ContainedTypes[0].ContainedTypes[0].ContainedTypes);

            var property2 = classModel.Properties[1];
            Assert.Equal("System.Func<int>", property2.Type.Name);
            Assert.Equal(1, property2.Type.ContainedTypes.Count);
            Assert.Equal("System.Func", property2.Type.ContainedTypes[0].Name);
            Assert.Empty(property2.Type.ContainedTypes[0].Constrains);
            Assert.Equal(1, property2.Type.ContainedTypes[0].ContainedTypes.Count);
            Assert.Equal("int", property2.Type.ContainedTypes[0].ContainedTypes[0].Name);
            Assert.Empty(property2.Type.ContainedTypes[0].ContainedTypes[0].Constrains);
            Assert.Empty(property2.Type.ContainedTypes[0].ContainedTypes[0].ContainedTypes);
        }


        [Theory]
        [InlineData("class")]
        [InlineData("record")]
        [InlineData("struct")]
        public void Extract_ShouldHaveFieldOfGenericTypeWithMultipleContainedTypes_WhenProvidedDifferentClassType(
            string classType)
        {
            var fileContent = $@"namespace Namespace1
{{
    public {classType} Class1 
    {{       
        public GenericClass<string,int, double> GField {{get;set;}}

        public GenericClass<GenericClass<int,string,double>, char, GenericClass<long,string,float>> FField{{get;set;}}
    }}

    public class GenericClass<T,R,K> {{}}
}}";

            var classTypes = _factExtractor.Extract(fileContent).ClassTypes;

            var classModel = (ClassModel)classTypes[0];

            Assert.Equal(2, classModel.Properties.Count);

            var property1 = classModel.Properties[0];
            Assert.Equal("Namespace1.GenericClass<string, int, double>", property1.Type.Name);
            Assert.Equal(1, property1.Type.ContainedTypes.Count);
            Assert.Equal("Namespace1.GenericClass", property1.Type.ContainedTypes[0].Name);
            Assert.Empty(property1.Type.ContainedTypes[0].Constrains);
            Assert.Equal(3, property1.Type.ContainedTypes[0].ContainedTypes.Count);
            var containedType1 = property1.Type.ContainedTypes[0].ContainedTypes[0];
            Assert.Equal("string", containedType1.Name);
            Assert.Empty(containedType1.Constrains);
            Assert.Empty(containedType1.ContainedTypes);

            var containedType2 = property1.Type.ContainedTypes[0].ContainedTypes[1];
            Assert.Equal("int", containedType2.Name);
            Assert.Empty(containedType2.Constrains);
            Assert.Empty(containedType2.ContainedTypes);

            var containedType3 = property1.Type.ContainedTypes[0].ContainedTypes[2];
            Assert.Equal("double", containedType3.Name);
            Assert.Empty(containedType3.Constrains);
            Assert.Empty(containedType3.ContainedTypes);

            var property2 = classModel.Properties[1];
            Assert.Equal(
                "Namespace1.GenericClass<Namespace1.GenericClass<int, string, double>, char, Namespace1.GenericClass<long, string, float>>",
                property2.Type.Name);
            Assert.Equal(1, property2.Type.ContainedTypes.Count);
            Assert.Equal("Namespace1.GenericClass", property2.Type.ContainedTypes[0].Name);
            Assert.Empty(property2.Type.ContainedTypes[0].Constrains);
            Assert.Equal(3, property2.Type.ContainedTypes[0].ContainedTypes.Count);
            var containedType4 = property2.Type.ContainedTypes[0].ContainedTypes[0];
            Assert.Equal("Namespace1.GenericClass", containedType4.Name);
            Assert.Empty(containedType4.Constrains);
            Assert.Equal(3, containedType4.ContainedTypes.Count);

            var containedType7 = containedType4.ContainedTypes[0];
            Assert.Equal("int", containedType7.Name);
            Assert.Empty(containedType7.Constrains);
            Assert.Empty(containedType7.ContainedTypes);

            var containedType8 = containedType4.ContainedTypes[1];
            Assert.Equal("string", containedType8.Name);
            Assert.Empty(containedType8.Constrains);
            Assert.Empty(containedType8.ContainedTypes);

            var containedType9 = containedType4.ContainedTypes[2];
            Assert.Equal("double", containedType9.Name);
            Assert.Empty(containedType9.Constrains);
            Assert.Empty(containedType9.ContainedTypes);


            var containedType5 = property2.Type.ContainedTypes[0].ContainedTypes[1];
            Assert.Equal("char", containedType5.Name);
            Assert.Empty(containedType5.Constrains);
            Assert.Empty(containedType5.ContainedTypes);


            var containedType6 = property2.Type.ContainedTypes[0].ContainedTypes[2];
            Assert.Equal("Namespace1.GenericClass", containedType6.Name);
            Assert.Empty(containedType6.Constrains);
            Assert.Equal(3, containedType6.ContainedTypes.Count);

            var containedType10 = containedType6.ContainedTypes[0];
            Assert.Equal("long", containedType10.Name);
            Assert.Empty(containedType10.Constrains);
            Assert.Empty(containedType10.ContainedTypes);

            var containedType11 = containedType6.ContainedTypes[1];
            Assert.Equal("string", containedType11.Name);
            Assert.Empty(containedType11.Constrains);
            Assert.Empty(containedType11.ContainedTypes);

            var containedType12 = containedType6.ContainedTypes[2];
            Assert.Equal("float", containedType12.Name);
            Assert.Empty(containedType12.Constrains);
            Assert.Empty(containedType12.ContainedTypes);
        }
    }
}

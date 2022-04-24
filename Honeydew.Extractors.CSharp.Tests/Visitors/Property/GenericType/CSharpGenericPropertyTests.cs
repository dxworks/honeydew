using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Property.GenericType;

public class CSharpGenericPropertyTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpGenericPropertyTests()
    {
        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpClassSetterCompilationUnitVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new CSharpPropertySetterClassVisitor(_loggerMock.Object, new List<ITypeVisitor<IPropertyType>>
                        {
                            new PropertyInfoVisitor()
                        })
                    })
            });

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [InlineData("class")]
    [InlineData("record")]
    [InlineData("struct")]
    public void Extract_ShouldHavePropertyOfGenericType_WhenProvidedDifferentClassType(string classType)
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

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];

        Assert.Equal(2, classModel.Properties.Count);

        var property1 = classModel.Properties[0];
        Assert.Equal("Namespace1.GenericClass<string>", property1.Type.Name);
        Assert.Equal(1, property1.Type.FullType.ContainedTypes.Count);
        Assert.Equal("Namespace1.GenericClass", property1.Type.FullType.Name);
        Assert.Equal(1, property1.Type.FullType.ContainedTypes.Count);
        Assert.Equal("string", property1.Type.FullType.ContainedTypes[0].Name);
        Assert.Empty(property1.Type.FullType.ContainedTypes[0].ContainedTypes);

        var property2 = classModel.Properties[1];
        Assert.Equal("System.Func<int>", property2.Type.Name);
        Assert.Equal(1, property2.Type.FullType.ContainedTypes.Count);
        Assert.Equal("System.Func", property2.Type.FullType.Name);
        Assert.Equal(1, property2.Type.FullType.ContainedTypes.Count);
        Assert.Equal("int", property2.Type.FullType.ContainedTypes[0].Name);
        Assert.Empty(property2.Type.FullType.ContainedTypes[0].ContainedTypes);
    }


    [Theory]
    [InlineData("class")]
    [InlineData("record")]
    [InlineData("struct")]
    public void Extract_ShouldHavePropertyOfGenericTypeWithMultipleContainedTypes_WhenProvidedDifferentClassType(
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

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];

        Assert.Equal(2, classModel.Properties.Count);

        var property1 = classModel.Properties[0];
        Assert.Equal("Namespace1.GenericClass<string, int, double>", property1.Type.Name);
        Assert.Equal("Namespace1.GenericClass", property1.Type.FullType.Name);
        Assert.Equal(3, property1.Type.FullType.ContainedTypes.Count);
        var containedType1 = property1.Type.FullType.ContainedTypes[0];
        Assert.Equal("string", containedType1.Name);
        Assert.Empty(containedType1.ContainedTypes);

        var containedType2 = property1.Type.FullType.ContainedTypes[1];
        Assert.Equal("int", containedType2.Name);
        Assert.Empty(containedType2.ContainedTypes);

        var containedType3 = property1.Type.FullType.ContainedTypes[2];
        Assert.Equal("double", containedType3.Name);
        Assert.Empty(containedType3.ContainedTypes);

        var property2 = classModel.Properties[1];
        Assert.Equal(
            "Namespace1.GenericClass<Namespace1.GenericClass<int, string, double>, char, Namespace1.GenericClass<long, string, float>>",
            property2.Type.Name);
        Assert.Equal("Namespace1.GenericClass", property2.Type.FullType.Name);
        Assert.Equal(3, property2.Type.FullType.ContainedTypes.Count);
        var containedType4 = property2.Type.FullType.ContainedTypes[0];
        Assert.Equal("Namespace1.GenericClass", containedType4.Name);
        Assert.Equal(3, containedType4.ContainedTypes.Count);

        var containedType7 = containedType4.ContainedTypes[0];
        Assert.Equal("int", containedType7.Name);
        Assert.Empty(containedType7.ContainedTypes);

        var containedType8 = containedType4.ContainedTypes[1];
        Assert.Equal("string", containedType8.Name);
        Assert.Empty(containedType8.ContainedTypes);

        var containedType9 = containedType4.ContainedTypes[2];
        Assert.Equal("double", containedType9.Name);
        Assert.Empty(containedType9.ContainedTypes);


        var containedType5 = property2.Type.FullType.ContainedTypes[1];
        Assert.Equal("char", containedType5.Name);
        Assert.Empty(containedType5.ContainedTypes);


        var containedType6 = property2.Type.FullType.ContainedTypes[2];
        Assert.Equal("Namespace1.GenericClass", containedType6.Name);
        Assert.Equal(3, containedType6.ContainedTypes.Count);

        var containedType10 = containedType6.ContainedTypes[0];
        Assert.Equal("long", containedType10.Name);
        Assert.Empty(containedType10.ContainedTypes);

        var containedType11 = containedType6.ContainedTypes[1];
        Assert.Equal("string", containedType11.Name);
        Assert.Empty(containedType11.ContainedTypes);

        var containedType12 = containedType6.ContainedTypes[2];
        Assert.Equal("float", containedType12.Name);
        Assert.Empty(containedType12.ContainedTypes);
    }

    [Theory]
    [InlineData("class")]
    [InlineData("record")]
    [InlineData("struct")]
    public void
        Extract_ShouldHavePropertyOfGenericTypeWithMultipleNullableContainedTypes_WhenProvidedDifferentClassType(
            string classType)
    {
        var fileContent = $@"namespace Namespace1
{{
    public {classType} Class1
    {{
        public GenericClass<string?, int?, double?>? GField {{ get; set; }}

        public GenericClass<GenericClass<int?, string?, double?>?, char?, GenericClass<long?, string?, float?>?>? FField {{ get; set; }}        
    }}

    public class GenericClass<T, R, K> {{ }}
}}";

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (ClassModel)classTypes[0];

        Assert.Equal(2, classModel.Properties.Count);

        var property1 = classModel.Properties[0];
        Assert.True(property1.IsNullable);
        Assert.Equal("Namespace1.GenericClass<string?, int?, double?>?", property1.Type.Name);
        Assert.Equal("Namespace1.GenericClass", property1.Type.FullType.Name);
        Assert.True(property1.Type.FullType.IsNullable);
        Assert.Equal(3, property1.Type.FullType.ContainedTypes.Count);
        var containedType1 = property1.Type.FullType.ContainedTypes[0];
        Assert.Equal("string", containedType1.Name);
        Assert.True(containedType1.IsNullable);
        Assert.Empty(containedType1.ContainedTypes);

        var containedType2 = property1.Type.FullType.ContainedTypes[1];
        Assert.Equal("int", containedType2.Name);
        Assert.True(containedType2.IsNullable);
        Assert.Empty(containedType2.ContainedTypes);

        var containedType3 = property1.Type.FullType.ContainedTypes[2];
        Assert.Equal("double", containedType3.Name);
        Assert.True(containedType3.IsNullable);
        Assert.Empty(containedType3.ContainedTypes);

        var property2 = classModel.Properties[1];
        Assert.True(property2.IsNullable);
        Assert.Equal(
            "Namespace1.GenericClass<Namespace1.GenericClass<int?, string?, double?>?, char?, Namespace1.GenericClass<long?, string?, float?>?>?",
            property2.Type.Name);
        Assert.Equal("Namespace1.GenericClass", property2.Type.FullType.Name);
        Assert.True(property2.Type.FullType.IsNullable);
        Assert.Equal(3, property2.Type.FullType.ContainedTypes.Count);
        var containedType4 = property2.Type.FullType.ContainedTypes[0];
        Assert.Equal("Namespace1.GenericClass", containedType4.Name);
        Assert.True(containedType4.IsNullable);
        Assert.Equal(3, containedType4.ContainedTypes.Count);

        var containedType7 = containedType4.ContainedTypes[0];
        Assert.Equal("int", containedType7.Name);
        Assert.True(containedType7.IsNullable);
        Assert.Empty(containedType7.ContainedTypes);

        var containedType8 = containedType4.ContainedTypes[1];
        Assert.Equal("string", containedType8.Name);
        Assert.True(containedType8.IsNullable);
        Assert.Empty(containedType8.ContainedTypes);

        var containedType9 = containedType4.ContainedTypes[2];
        Assert.Equal("double", containedType9.Name);
        Assert.True(containedType9.IsNullable);
        Assert.Empty(containedType9.ContainedTypes);


        var containedType5 = property2.Type.FullType.ContainedTypes[1];
        Assert.Equal("char", containedType5.Name);
        Assert.True(containedType5.IsNullable);
        Assert.Empty(containedType5.ContainedTypes);


        var containedType6 = property2.Type.FullType.ContainedTypes[2];
        Assert.Equal("Namespace1.GenericClass", containedType6.Name);
        Assert.True(containedType6.IsNullable);
        Assert.Equal(3, containedType6.ContainedTypes.Count);

        var containedType10 = containedType6.ContainedTypes[0];
        Assert.Equal("long", containedType10.Name);
        Assert.True(containedType10.IsNullable);
        Assert.Empty(containedType10.ContainedTypes);

        var containedType11 = containedType6.ContainedTypes[1];
        Assert.Equal("string", containedType11.Name);
        Assert.True(containedType11.IsNullable);
        Assert.Empty(containedType11.ContainedTypes);

        var containedType12 = containedType6.ContainedTypes[2];
        Assert.Equal("float", containedType12.Name);
        Assert.True(containedType12.IsNullable);
        Assert.Empty(containedType12.ContainedTypes);
    }
}

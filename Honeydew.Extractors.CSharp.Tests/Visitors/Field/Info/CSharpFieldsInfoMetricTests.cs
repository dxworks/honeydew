using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Dotnet;
using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Field.Info;

public class CSharpFieldsInfoMetricTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly DotnetSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpFieldsInfoMetricTests()
    {
        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpClassSetterVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new CSharpFieldSetterClassVisitor(_loggerMock.Object, new List<ITypeVisitor<IFieldType>>
                        {
                            new FieldInfoVisitor()
                        })
                    })
            });


        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [FileData("TestData/InterfaceWithNoFields.txt")]
    public void Extract_ShouldHaveNoFields_WhenGivenAnInterface(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(2, classTypes.Count);

        foreach (var classType in classTypes)
        {
            var classModel = (CSharpClassModel)classType;
            Assert.Empty(classModel.Fields);
        }
    }

    [Theory]
    [FileData("TestData/ClassWithFieldsWithModifiers.txt")]
    public void Extract_ShouldHavePrivateFieldsWithModifiers_WhenGivenClassWithFieldsAndModifiersWithDefaultAccess(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);


        var fieldTypes = ((CSharpClassModel)classTypes[0]).Fields;

        Assert.Equal(3, fieldTypes.Count);

        Assert.Equal("A", fieldTypes[0].Name);
        Assert.Equal("int", fieldTypes[0].Type.Name);
        Assert.Equal("readonly", fieldTypes[0].Modifier);
        Assert.Equal("private", fieldTypes[0].AccessModifier);
        Assert.False(((CSharpFieldModel)fieldTypes[0]).IsEvent);

        Assert.Equal("X", fieldTypes[1].Name);
        Assert.Equal("float", fieldTypes[1].Type.Name);
        Assert.Equal("volatile", fieldTypes[1].Modifier);
        Assert.Equal("private", fieldTypes[1].AccessModifier);
        Assert.False(((CSharpFieldModel)fieldTypes[1]).IsEvent);

        Assert.Equal("Y", fieldTypes[2].Name);
        Assert.Equal("string", fieldTypes[2].Type.Name);
        Assert.Equal("static", fieldTypes[2].Modifier);
        Assert.Equal("private", fieldTypes[2].AccessModifier);
        Assert.False(((CSharpFieldModel)fieldTypes[2]).IsEvent);
    }

    [Theory]
    [InlineData("public")]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("internal")]
    [InlineData("protected internal")]
    [InlineData("private protected")]
    public void Extract_ShouldHaveFieldsWithNoOtherModifiers_WhenGivenClassWithOnlyFieldsAndTheirModifier(
        string modifier)
    {
        var fileContent = $@"using System;
                                      using HoneydewCore.Extractors;
                                      namespace TopLevel
                                      {{
                                          public class Foo {{ {modifier} int AnimalNest; {modifier} float X,Yaz_fafa; {modifier} string _zxy; {modifier} CSharpMetricExtractor extractor;}}                                        
                                      }}";

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var fieldTypes = ((CSharpClassModel)classTypes[0]).Fields;

        Assert.Equal(5, fieldTypes.Count);

        Assert.Equal("AnimalNest", fieldTypes[0].Name);
        Assert.Equal("int", fieldTypes[0].Type.Name);
        Assert.Equal("", fieldTypes[0].Modifier);
        Assert.Equal(modifier, fieldTypes[0].AccessModifier);
        Assert.False(((CSharpFieldModel)fieldTypes[0]).IsEvent);

        Assert.Equal("X", fieldTypes[1].Name);
        Assert.Equal("float", fieldTypes[1].Type.Name);
        Assert.Equal("", fieldTypes[1].Modifier);
        Assert.Equal(modifier, fieldTypes[1].AccessModifier);
        Assert.False(((CSharpFieldModel)fieldTypes[1]).IsEvent);

        Assert.Equal("Yaz_fafa", fieldTypes[2].Name);
        Assert.Equal("float", fieldTypes[2].Type.Name);
        Assert.Equal("", fieldTypes[2].Modifier);
        Assert.Equal(modifier, fieldTypes[2].AccessModifier);
        Assert.False(((CSharpFieldModel)fieldTypes[2]).IsEvent);

        Assert.Equal("_zxy", fieldTypes[3].Name);
        Assert.Equal("string", fieldTypes[3].Type.Name);
        Assert.Equal("", fieldTypes[3].Modifier);
        Assert.Equal(modifier, fieldTypes[3].AccessModifier);
        Assert.False(((CSharpFieldModel)fieldTypes[3]).IsEvent);

        Assert.Equal("extractor", fieldTypes[4].Name);
        Assert.Equal("CSharpMetricExtractor", fieldTypes[4].Type.Name);
        Assert.Equal("", fieldTypes[4].Modifier);
        Assert.Equal(modifier, fieldTypes[4].AccessModifier);
        Assert.False(((CSharpFieldModel)fieldTypes[4]).IsEvent);
    }

    [Theory]
    [InlineData("public")]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("internal")]
    [InlineData("protected internal")]
    [InlineData("private protected")]
    public void Extract_ShouldHaveFieldsWithNoOtherModifiers_WhenGivenClassWithOnlyEventFieldsAndTheirModifier(
        string visibility)
    {
        var fileContent = $@"using System;
                                      using HoneydewCore.Extractors;
                                      namespace SomeNamespace
                                      {{
                                          public class Foo {{ {visibility} event CSharpMetricExtractor extractor; {visibility} event int _some_event; {visibility} event Action MyAction1,MyAction2;}}                                        
                                      }}";

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var fieldTypes = ((CSharpClassModel)classTypes[0]).Fields;

        Assert.Equal(4, fieldTypes.Count);

        Assert.Equal("extractor", fieldTypes[0].Name);
        Assert.Equal("CSharpMetricExtractor", fieldTypes[0].Type.Name);
        Assert.Equal("", fieldTypes[0].Modifier);
        Assert.Equal(visibility, fieldTypes[0].AccessModifier);
        Assert.True(((CSharpFieldModel)fieldTypes[0]).IsEvent);

        Assert.Equal("_some_event", fieldTypes[1].Name);
        Assert.Equal("int", fieldTypes[1].Type.Name);
        Assert.Equal("", fieldTypes[1].Modifier);
        Assert.Equal(visibility, fieldTypes[1].AccessModifier);
        Assert.True(((CSharpFieldModel)fieldTypes[1]).IsEvent);

        Assert.Equal("MyAction1", fieldTypes[2].Name);
        Assert.Equal("System.Action", fieldTypes[2].Type.Name);
        Assert.Equal("", fieldTypes[2].Modifier);
        Assert.Equal(visibility, fieldTypes[2].AccessModifier);
        Assert.True(((CSharpFieldModel)fieldTypes[2]).IsEvent);

        Assert.Equal("MyAction2", fieldTypes[3].Name);
        Assert.Equal("System.Action", fieldTypes[3].Type.Name);
        Assert.Equal("", fieldTypes[3].Modifier);
        Assert.Equal(visibility, fieldTypes[3].AccessModifier);
        Assert.True(((CSharpFieldModel)fieldTypes[3]).IsEvent);
    }

    [Theory]
    [InlineData("static")]
    [InlineData("volatile")]
    [InlineData("readonly")]
    public void Extract_ShouldHaveFieldsWithNoModifiers_WhenGivenClassWithFieldsAndTheirVisibilityAndMethods(
        string modifier)
    {
        var fileContent = $@"using System;
                                      using HoneydewCore.Extractors;
                                      namespace TopLevel
                                      {{
                                          public class Foo {{ {modifier} public int AnimalNest; protected {modifier} float X,Yaz_fafa; {modifier} string _zxy; {modifier} CSharpMetricExtractor extractor;
                                              void f() {{ AnimalNest=0;}}
                                              }}                                        
                                      }}";

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var fieldTypes = ((CSharpClassModel)classTypes[0]).Fields;

        Assert.Equal(5, fieldTypes.Count);

        Assert.Equal("AnimalNest", fieldTypes[0].Name);
        Assert.Equal("int", fieldTypes[0].Type.Name);
        Assert.Equal(modifier, fieldTypes[0].Modifier);
        Assert.Equal("public", fieldTypes[0].AccessModifier);
        Assert.False(((CSharpFieldModel)fieldTypes[0]).IsEvent);

        Assert.Equal("X", fieldTypes[1].Name);
        Assert.Equal("float", fieldTypes[1].Type.Name);
        Assert.Equal(modifier, fieldTypes[1].Modifier);
        Assert.Equal("protected", fieldTypes[1].AccessModifier);
        Assert.False(((CSharpFieldModel)fieldTypes[1]).IsEvent);

        Assert.Equal("Yaz_fafa", fieldTypes[2].Name);
        Assert.Equal("float", fieldTypes[2].Type.Name);
        Assert.Equal(modifier, fieldTypes[2].Modifier);
        Assert.Equal("protected", fieldTypes[2].AccessModifier);
        Assert.False(((CSharpFieldModel)fieldTypes[2]).IsEvent);

        Assert.Equal("_zxy", fieldTypes[3].Name);
        Assert.Equal("string", fieldTypes[3].Type.Name);
        Assert.Equal(modifier, fieldTypes[3].Modifier);
        Assert.Equal("private", fieldTypes[3].AccessModifier);
        Assert.False(((CSharpFieldModel)fieldTypes[3]).IsEvent);

        Assert.Equal("extractor", fieldTypes[4].Name);
        Assert.Equal("CSharpMetricExtractor", fieldTypes[4].Type.Name);
        Assert.Equal(modifier, fieldTypes[4].Modifier);
        Assert.Equal("private", fieldTypes[4].AccessModifier);
        Assert.False(((CSharpFieldModel)fieldTypes[4]).IsEvent);
    }
}

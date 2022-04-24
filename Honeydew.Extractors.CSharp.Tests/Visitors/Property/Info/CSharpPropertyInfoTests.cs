using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Property.Info;

public class CSharpPropertyInfoTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpPropertyInfoTests()
    {
        var calledMethodSetterVisitor = new CSharpCalledMethodSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IMethodCallType>>
            {
                new MethodCallInfoVisitor()
            });
        var methodInfoVisitor = new MethodInfoVisitor();

        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpClassSetterCompilationUnitVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new CSharpPropertySetterClassVisitor(_loggerMock.Object, new List<ITypeVisitor<IPropertyType>>
                        {
                            new PropertyInfoVisitor(),
                            new CSharpAccessorMethodSetterPropertyVisitor(_loggerMock.Object,
                                new List<ITypeVisitor<IAccessorMethodType>>
                                {
                                    methodInfoVisitor,
                                    calledMethodSetterVisitor
                                })
                        })
                    })
            });

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [InlineData("class")]
    [InlineData("interface")]
    [InlineData("record")]
    [InlineData("struct")]
    public void Extract_ShouldHavePropertyInfo_WhenGivenTypeWithProperty(string entityType)
    {
        var fileContent = $@"namespace Models.Main.Items
{{
    public {entityType} MainItem
    {{
        public int Value {{get;set;}}
    }}
}}";

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal(1, classModel.Properties.Count);
        var property = classModel.Properties[0];
        Assert.Equal("Value", property.Name);
        Assert.Equal("public", property.AccessModifier);
        Assert.Equal("int", property.Type.Name);
        Assert.False(property.IsEvent);
        Assert.Equal(2, property.Accessors.Count);
        Assert.Equal("get", property.Accessors[0].Name);
        Assert.Equal("set", property.Accessors[1].Name);
    }

    [Theory]
    [InlineData("class")]
    [InlineData("interface")]
    [InlineData("record")]
    [InlineData("struct")]
    public void Extract_ShouldHaveEventPropertyInfo_WhenGivenTypeWithProperty(string entityType)
    {
        var fileContent = $@"namespace Models.Main.Items
{{
    public {entityType} MainItem
    {{
        public event System.Func<int> Value {{add{{}} remove{{}}}}
    }}
}}";
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal(1, classModel.Properties.Count);
        var property = classModel.Properties[0];
        Assert.Equal("Value", property.Name);
        Assert.Equal("public", property.AccessModifier);
        Assert.Equal("System.Func<int>", property.Type.Name);
        Assert.True(property.IsEvent);
        Assert.Equal(2, property.Accessors.Count);
        Assert.Equal("add", property.Accessors[0].Name);
        Assert.Equal("remove", property.Accessors[1].Name);
    }

    [Theory]
    [FileData("TestData/PropertyInInterface.txt")]
    public void Extract_ShouldHaveAbstractModifier_WhenGivenPropertyInInterface(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal(1, classModel.Properties.Count);
        var property = classModel.Properties[0];
        Assert.Equal("Value", property.Name);
        Assert.Equal("public", property.AccessModifier);
        Assert.Equal("abstract", property.Modifier);
    }

    [Theory]
    [FileData("TestData/InterfaceWithProperty.txt")]
    public void Extract_ShouldHaveProperties_WhenGivenAnInterface(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var propertyModel = ((CSharpClassModel)classTypes[0]).Properties[0];
        Assert.Equal("Value", propertyModel.Name);
        Assert.Equal("abstract", propertyModel.Modifier);
        Assert.Equal("public", propertyModel.AccessModifier);
        Assert.Equal("int", propertyModel.Type.Name);
        Assert.False(propertyModel.IsEvent);
        foreach (var accessor in propertyModel.Accessors)
        {
            Assert.Empty(accessor.CalledMethods);
        }
    }

    [Theory]
    [FileData("TestData/ClassWithPropertiesWithNoAccessModifier.txt")]
    public void
        Extract_ShouldHavePrivatePropertiesWithModifiers_WhenGivenClassWithPropertiesAndModifiersWithDefaultAccess(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var propertyTypes = ((CSharpClassModel)classTypes[0]).Properties;

        Assert.Equal(2, propertyTypes.Count);

        Assert.Equal("A", propertyTypes[0].Name);
        Assert.Equal("int", propertyTypes[0].Type.Name);
        Assert.Equal("static", propertyTypes[0].Modifier);
        Assert.Equal("private", propertyTypes[0].AccessModifier);
        Assert.False(propertyTypes[0].IsEvent);
        foreach (var accessor in propertyTypes[0].Accessors)
        {
            Assert.Empty(accessor.CalledMethods);
        }

        Assert.Equal("X", propertyTypes[1].Name);
        Assert.Equal("float", propertyTypes[1].Type.Name);
        Assert.Equal("", propertyTypes[1].Modifier);
        Assert.Equal("private", propertyTypes[1].AccessModifier);
        Assert.False(propertyTypes[1].IsEvent);
        foreach (var accessor in propertyTypes[1].Accessors)
        {
            Assert.Empty(accessor.CalledMethods);
        }
    }

    [Theory]
    [InlineData("public")]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("internal")]
    [InlineData("protected internal")]
    [InlineData("private protected")]
    public void Extract_ShouldHavePropertiesWithNoOtherModifiers_WhenGivenClassWithOnlyPropertiesAndTheirModifier(
        string modifier)
    {
        var fileContent = $@"using System;
                                      using HoneydewCore.Extractors;
                                      namespace TopLevel
                                      {{
                                          public class Foo {{ {modifier} int AnimalNest{{get;set;}} {modifier} float X {{get;set;}} {modifier} CSharpMetricExtractor extractor{{get;init;}}}}                                        
                                      }}";

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var propertyModels = ((CSharpClassModel)classTypes[0]).Properties;

        Assert.Equal(3, propertyModels.Count);

        Assert.Equal("AnimalNest", propertyModels[0].Name);
        Assert.Equal("int", propertyModels[0].Type.Name);
        Assert.Equal("", propertyModels[0].Modifier);
        Assert.Equal(modifier, propertyModels[0].AccessModifier);
        Assert.False(propertyModels[0].IsEvent);
        foreach (var accessor in propertyModels[0].Accessors)
        {
            Assert.Empty(accessor.CalledMethods);
        }

        Assert.Equal("X", propertyModels[1].Name);
        Assert.Equal("float", propertyModels[1].Type.Name);
        Assert.Equal("", propertyModels[1].Modifier);
        Assert.Equal(modifier, propertyModels[1].AccessModifier);
        Assert.False(propertyModels[1].IsEvent);
        foreach (var accessor in propertyModels[1].Accessors)
        {
            Assert.Empty(accessor.CalledMethods);
        }

        Assert.Equal("extractor", propertyModels[2].Name);
        Assert.Equal("CSharpMetricExtractor", propertyModels[2].Type.Name);
        Assert.Equal("", propertyModels[2].Modifier);
        Assert.Equal(modifier, propertyModels[2].AccessModifier);
        Assert.False(propertyModels[2].IsEvent);
        foreach (var accessor in propertyModels[2].Accessors)
        {
            Assert.Empty(accessor.CalledMethods);
        }
    }

    [Theory]
    [InlineData("public")]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("internal")]
    [InlineData("protected internal")]
    [InlineData("private protected")]
    public void
        Extract_ShouldHavePropertiesWithNoOtherModifiers_WhenGivenClassWithOnlyEventPropertiesAndTheirModifier(
            string visibility)
    {
        var fileContent = $@"using System;
                                      using HoneydewCore.Extractors;
                                      namespace SomeNamespace
                                      {{
                                          public class Foo {{ {visibility} event CSharpMetricExtractor extractor {{add{{}} remove{{}} }} {visibility} event int _some_event{{add{{}} remove{{}} }} {visibility} event Action MyAction1{{add{{}} remove{{}} }}}}                                        
                                      }}";

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var propertyModels = ((CSharpClassModel)classTypes[0]).Properties;

        Assert.Equal(3, propertyModels.Count);

        Assert.Equal("extractor", propertyModels[0].Name);
        Assert.Equal("CSharpMetricExtractor", propertyModels[0].Type.Name);
        Assert.Equal("", propertyModels[0].Modifier);
        Assert.Equal(visibility, propertyModels[0].AccessModifier);
        Assert.True(propertyModels[0].IsEvent);
        foreach (var accessor in propertyModels[0].Accessors)
        {
            Assert.Empty(accessor.CalledMethods);
        }

        Assert.Equal("_some_event", propertyModels[1].Name);
        Assert.Equal("int", propertyModels[1].Type.Name);
        Assert.Equal("", propertyModels[1].Modifier);
        Assert.Equal(visibility, propertyModels[1].AccessModifier);
        Assert.True(propertyModels[1].IsEvent);
        foreach (var accessor in propertyModels[1].Accessors)
        {
            Assert.Empty(accessor.CalledMethods);
        }

        Assert.Equal("MyAction1", propertyModels[2].Name);
        Assert.Equal("System.Action", propertyModels[2].Type.Name);
        Assert.Equal("", propertyModels[2].Modifier);
        Assert.Equal(visibility, propertyModels[2].AccessModifier);
        Assert.True(propertyModels[2].IsEvent);
        foreach (var accessor in propertyModels[2].Accessors)
        {
            Assert.Empty(accessor.CalledMethods);
        }
    }

    [Theory]
    [FileData("TestData/ClassWithPropertiesWithDifferentAccessors.txt")]
    public void Extract_ShouldHavePropertiesWithAccessors_WhenGivenClassWithPropertiesWithDifferentAccessors(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var propertyModels = ((CSharpClassModel)classTypes[0]).Properties;

        Assert.Equal(5, propertyModels.Count);

        var propertyModel0 = propertyModels[0];
        Assert.Equal("Value", propertyModel0.Name);
        Assert.False(propertyModel0.IsEvent);
        Assert.Equal(2, propertyModel0.Accessors.Count);
        Assert.Equal("get", propertyModel0.Accessors[0].Name);
        Assert.Equal("public", propertyModel0.Accessors[0].AccessModifier);
        Assert.Equal("", propertyModel0.Accessors[0].Modifier);

        Assert.Equal("set", propertyModel0.Accessors[1].Name);
        Assert.Equal("private", propertyModel0.Accessors[1].AccessModifier);
        Assert.Equal("", propertyModel0.Accessors[1].Modifier);

        var propertyModel1 = propertyModels[1];
        Assert.Equal("Name", propertyModel1.Name);
        Assert.False(propertyModel1.IsEvent);
        Assert.Equal(2, propertyModel1.Accessors.Count);
        Assert.Equal("get", propertyModel1.Accessors[0].Name);
        Assert.Equal("public", propertyModel1.Accessors[0].AccessModifier);
        Assert.Equal("", propertyModel1.Accessors[0].Modifier);

        Assert.Equal("init", propertyModel1.Accessors[1].Name);
        Assert.Equal("protected", propertyModel1.Accessors[1].AccessModifier);
        Assert.Equal("", propertyModel1.Accessors[1].Modifier);

        var propertyModel2 = propertyModels[2];
        Assert.Equal("FullName", propertyModel2.Name);
        Assert.False(propertyModel2.IsEvent);
        Assert.Equal(1, propertyModel2.Accessors.Count);
        Assert.Equal("get", propertyModel2.Accessors[0].Name);
        Assert.Equal("public", propertyModel2.Accessors[0].AccessModifier);
        Assert.Equal("", propertyModel2.Accessors[0].Modifier);

        var propertyModel3 = propertyModels[3];
        Assert.Equal("IsHere", propertyModel3.Name);
        Assert.False(propertyModel3.IsEvent);
        Assert.Equal(1, propertyModel3.Accessors.Count);
        Assert.Equal("set", propertyModel3.Accessors[0].Name);
        Assert.Equal("public", propertyModel3.Accessors[0].AccessModifier);
        Assert.Equal("", propertyModel3.Accessors[0].Modifier);

        var propertyModel4 = propertyModels[4];
        Assert.Equal("IntEvent", propertyModel4.Name);
        Assert.True(propertyModel4.IsEvent);
        Assert.Equal(2, propertyModel4.Accessors.Count);
        Assert.Equal("add", propertyModel4.Accessors[0].Name);
        Assert.Equal("public", propertyModel4.Accessors[0].AccessModifier);
        Assert.Equal("", propertyModel4.Accessors[0].Modifier);

        Assert.Equal("remove", propertyModel4.Accessors[1].Name);
        Assert.Equal("public", propertyModel4.Accessors[1].AccessModifier);
        Assert.Equal("", propertyModel4.Accessors[1].Modifier);
    }

    [Theory]
    [FileData("TestData/ClassWithComputedProperty.txt")]
    public void
        Extract_ShouldHaveProperties_WhenGivenClassWithComputedEmptyProperties(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(2, classTypes.Count);

        var propertyModels = ((CSharpClassModel)classTypes[1]).Properties;

        Assert.Equal(1, propertyModels.Count);

        Assert.Equal("Value", propertyModels[0].Name);
        Assert.Equal("TopLevel.Bar", propertyModels[0].Type.Name);
        Assert.Equal("", propertyModels[0].Modifier);
        Assert.Equal("protected", propertyModels[0].AccessModifier);
        Assert.False(propertyModels[0].IsEvent);
        foreach (var accessor in propertyModels[0].Accessors)
        {
            Assert.Empty(accessor.CalledMethods);
        }
    }

    [Theory]
    [FileData("TestData/ClassWithPropertyThatCallsInnerMethods.txt")]
    public void Extract_ShouldHaveProperties_WhenGivenClassWithComputedPropertyThatCallsInnerMethods(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var propertyModels = ((CSharpClassModel)classTypes[0]).Properties;

        Assert.Equal(1, propertyModels.Count);

        Assert.Equal("Value", propertyModels[0].Name);
        Assert.Equal("int", propertyModels[0].Type.Name);
        Assert.Equal("", propertyModels[0].Modifier);
        Assert.Equal("public", propertyModels[0].AccessModifier);
        Assert.False(propertyModels[0].IsEvent);

        var getAccessor = propertyModels[0].Accessors[0];
        Assert.Equal("get", getAccessor.Name);
        Assert.Equal(1, getAccessor.CalledMethods.Count);
        Assert.Equal("Triple", getAccessor.CalledMethods[0].Name);
        Assert.Equal("TopLevel.Foo", getAccessor.CalledMethods[0].DefinitionClassName);
        Assert.Equal("TopLevel.Foo", getAccessor.CalledMethods[0].LocationClassName);
        Assert.Equal(1, getAccessor.CalledMethods[0].ParameterTypes.Count);
        var parameterModel1 = (CSharpParameterModel)getAccessor.CalledMethods[0].ParameterTypes[0];
        Assert.Equal("", parameterModel1.Modifier);
        Assert.Equal("int", parameterModel1.Type.Name);
        Assert.Null(parameterModel1.DefaultValue);

        var setAccessor = propertyModels[0].Accessors[1];
        Assert.Equal("set", setAccessor.Name);
        Assert.Equal(1, setAccessor.CalledMethods.Count);
        Assert.Equal("Double", setAccessor.CalledMethods[0].Name);
        Assert.Equal("TopLevel.Foo", setAccessor.CalledMethods[0].DefinitionClassName);
        Assert.Equal("TopLevel.Foo", setAccessor.CalledMethods[0].LocationClassName);
        Assert.Equal(1, setAccessor.CalledMethods[0].ParameterTypes.Count);
        var parameterModel2 = (CSharpParameterModel)setAccessor.CalledMethods[0].ParameterTypes[0];
        Assert.Equal("", parameterModel2.Modifier);
        Assert.Equal("int", parameterModel2.Type.Name);
        Assert.Null(parameterModel2.DefaultValue);
    }

    [Theory]
    [FileData("TestData/ClassWithPropertyThatCallsMethodFromAnotherClass.txt")]
    public void
        Extract_ShouldHaveProperties_WhenGivenClassWithComputedPropertyThatCallsMethodsFromOtherClassFromTheSameNamespace(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(2, classTypes.Count);

        var propertyModels = ((CSharpClassModel)classTypes[1]).Properties;

        Assert.Equal(1, propertyModels.Count);

        Assert.Equal("Value", propertyModels[0].Name);
        Assert.Equal("int", propertyModels[0].Type.Name);
        Assert.Equal("", propertyModels[0].Modifier);
        Assert.Equal("public", propertyModels[0].AccessModifier);
        Assert.False(propertyModels[0].IsEvent);

        var getAccessor = propertyModels[0].Accessors[0];
        Assert.Equal("get", getAccessor.Name);
        Assert.Equal(1, getAccessor.CalledMethods.Count);
        Assert.Equal("Triple", getAccessor.CalledMethods[0].Name);
        Assert.Equal("TopLevel.Bar", getAccessor.CalledMethods[0].DefinitionClassName);
        Assert.Equal("TopLevel.Bar", getAccessor.CalledMethods[0].LocationClassName);
        Assert.Equal(1, getAccessor.CalledMethods[0].ParameterTypes.Count);
        var parameterModel1 = (CSharpParameterModel)getAccessor.CalledMethods[0].ParameterTypes[0];
        Assert.Equal("", parameterModel1.Modifier);
        Assert.Equal("int", parameterModel1.Type.Name);
        Assert.Null(parameterModel1.DefaultValue);

        var setAccessor = propertyModels[0].Accessors[1];
        Assert.Equal("set", setAccessor.Name);
        Assert.Equal(1, setAccessor.CalledMethods.Count);
        Assert.Equal("Double", setAccessor.CalledMethods[0].Name);
        Assert.Equal("TopLevel.Bar", setAccessor.CalledMethods[0].DefinitionClassName);
        Assert.Equal("TopLevel.Bar", setAccessor.CalledMethods[0].LocationClassName);
        Assert.Equal(1, setAccessor.CalledMethods[0].ParameterTypes.Count);
        var parameterModel2 = (CSharpParameterModel)setAccessor.CalledMethods[0].ParameterTypes[0];
        Assert.Equal("", parameterModel2.Modifier);
        Assert.Equal("int", parameterModel2.Type.Name);
        Assert.Null(parameterModel2.DefaultValue);
    }

    [Theory]
    [FileData("TestData/ClassWithPropertyThatCallsMethodFromExternClass.txt")]
    public void
        Extract_ShouldHaveProperties_WhenGivenClassWithComputedPropertyThatCallsStaticMethodsFromUnknownClass(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var propertyModels = ((CSharpClassModel)classTypes[0]).Properties;

        Assert.Equal(1, propertyModels.Count);

        Assert.Equal("Value", propertyModels[0].Name);
        Assert.Equal("int", propertyModels[0].Type.Name);
        Assert.Equal("", propertyModels[0].Modifier);
        Assert.Equal("public", propertyModels[0].AccessModifier);
        Assert.False(propertyModels[0].IsEvent);

        var getAccessor = propertyModels[0].Accessors[0];
        Assert.Equal("get", getAccessor.Name);
        Assert.Equal(1, getAccessor.CalledMethods.Count);
        Assert.Equal("Triple", getAccessor.CalledMethods[0].Name);
        Assert.Equal("ExternClass", getAccessor.CalledMethods[0].DefinitionClassName);
        Assert.Equal("ExternClass", getAccessor.CalledMethods[0].LocationClassName);
        Assert.Equal(1, getAccessor.CalledMethods[0].ParameterTypes.Count);
        var parameterModel1 = (CSharpParameterModel)getAccessor.CalledMethods[0].ParameterTypes[0];
        Assert.Equal("", parameterModel1.Modifier);
        Assert.Equal("int", parameterModel1.Type.Name);
        Assert.Null(parameterModel1.DefaultValue);

        var setAccessor = propertyModels[0].Accessors[1];
        Assert.Equal("set", setAccessor.Name);
        Assert.Equal(1, setAccessor.CalledMethods.Count);
        Assert.Equal("Double", setAccessor.CalledMethods[0].Name);
        Assert.Equal("ExternClass", setAccessor.CalledMethods[0].DefinitionClassName);
        Assert.Equal("ExternClass", setAccessor.CalledMethods[0].LocationClassName);
        Assert.Equal(1, setAccessor.CalledMethods[0].ParameterTypes.Count);
        var parameterModel2 = (CSharpParameterModel)setAccessor.CalledMethods[0].ParameterTypes[0];
        Assert.Equal("", parameterModel2.Modifier);
        Assert.Equal("int", parameterModel2.Type.Name);
        Assert.Null(parameterModel2.DefaultValue);
    }

    [Theory]
    [FileData("TestData/ClassWithEventPropertyThatCallsInnerMethods.txt")]
    public void
        Extract_ShouldHaveEventProperties_WhenGivenClassWithComputedPropertyThatCallsInnerMethods(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var propertyModels = ((CSharpClassModel)classTypes[0]).Properties;

        Assert.Equal(1, propertyModels.Count);

        Assert.Equal("Value", propertyModels[0].Name);
        Assert.Equal("System.Func<double>", propertyModels[0].Type.Name);
        Assert.Equal("", propertyModels[0].Modifier);
        Assert.Equal("public", propertyModels[0].AccessModifier);
        Assert.True(propertyModels[0].IsEvent);

        var addAccessor = propertyModels[0].Accessors[0];
        Assert.Equal("add", addAccessor.Name);
        Assert.Equal(1, addAccessor.CalledMethods.Count);
        Assert.Equal("Triple", addAccessor.CalledMethods[0].Name);
        Assert.Equal("TopLevel.Foo", addAccessor.CalledMethods[0].DefinitionClassName);
        Assert.Equal("TopLevel.Foo", addAccessor.CalledMethods[0].LocationClassName);
        Assert.Equal(1, addAccessor.CalledMethods[0].ParameterTypes.Count);
        var parameterModel1 = (CSharpParameterModel)addAccessor.CalledMethods[0].ParameterTypes[0];
        Assert.Equal("", parameterModel1.Modifier);
        Assert.Equal("double", parameterModel1.Type.Name);
        Assert.Null(parameterModel1.DefaultValue);

        var removeAccessor = propertyModels[0].Accessors[1];
        Assert.Equal("remove", removeAccessor.Name);
        Assert.Equal(1, removeAccessor.CalledMethods.Count);
        Assert.Equal("Double", removeAccessor.CalledMethods[0].Name);
        Assert.Equal("TopLevel.Foo", removeAccessor.CalledMethods[0].DefinitionClassName);
        Assert.Equal("TopLevel.Foo", removeAccessor.CalledMethods[0].LocationClassName);
        Assert.Equal(1, removeAccessor.CalledMethods[0].ParameterTypes.Count);
        var parameterModel2 = (CSharpParameterModel)removeAccessor.CalledMethods[0].ParameterTypes[0];
        Assert.Equal("", parameterModel2.Modifier);
        Assert.Equal("double", parameterModel2.Type.Name);
        Assert.Null(parameterModel2.DefaultValue);
    }

    [Theory]
    [FileData("TestData/ClassWithEventPropertyThatCallsMethodFromAnotherClass.txt")]
    public void
        Extract_ShouldHaveEventProperties_WhenGivenClassWithComputedPropertyThatCallsMethodsFromOtherClassFromTheSameNamespace(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(2, classTypes.Count);

        var propertyModels = ((CSharpClassModel)classTypes[1]).Properties;

        Assert.Equal(1, propertyModels.Count);

        Assert.Equal("Value", propertyModels[0].Name);
        Assert.Equal("System.Func<string>", propertyModels[0].Type.Name);
        Assert.Equal("", propertyModels[0].Modifier);
        Assert.Equal("public", propertyModels[0].AccessModifier);
        Assert.True(propertyModels[0].IsEvent);

        var addAccessor = propertyModels[0].Accessors[0];
        Assert.Equal("add", addAccessor.Name);
        Assert.Equal(1, addAccessor.CalledMethods.Count);
        Assert.Equal("Convert", addAccessor.CalledMethods[0].Name);
        Assert.Equal("TopLevel.Bar", addAccessor.CalledMethods[0].DefinitionClassName);
        Assert.Equal("TopLevel.Bar", addAccessor.CalledMethods[0].LocationClassName);
        Assert.Equal(1, addAccessor.CalledMethods[0].ParameterTypes.Count);
        var parameterModel1 = (CSharpParameterModel)addAccessor.CalledMethods[0].ParameterTypes[0];
        Assert.Equal("", parameterModel1.Modifier);
        Assert.Equal("int", parameterModel1.Type.Name);
        Assert.Null(parameterModel1.DefaultValue);

        var removeAccessor = propertyModels[0].Accessors[1];
        Assert.Equal("remove", removeAccessor.Name);
        Assert.Equal(1, removeAccessor.CalledMethods.Count);
        Assert.Equal("Cut", removeAccessor.CalledMethods[0].Name);
        Assert.Equal("TopLevel.Bar", removeAccessor.CalledMethods[0].DefinitionClassName);
        Assert.Equal("TopLevel.Bar", removeAccessor.CalledMethods[0].LocationClassName);
        Assert.Equal(1, removeAccessor.CalledMethods[0].ParameterTypes.Count);
        var parameterModel2 = (CSharpParameterModel)removeAccessor.CalledMethods[0].ParameterTypes[0];
        Assert.Equal("", parameterModel2.Modifier);
        Assert.Equal("System.Func<string>", parameterModel2.Type.Name);
        Assert.Null(parameterModel2.DefaultValue);
    }

    [Theory]
    [FileData("TestData/ClassWithEventPropertyThatCallsMethodFromExternClass.txt")]
    public void
        Extract_ShouldHaveEventProperties_WhenGivenClassWithComputedPropertyThatCallsStaticMethodsFromUnknownClass(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var propertyModels = ((CSharpClassModel)classTypes[0]).Properties;

        Assert.Equal(1, propertyModels.Count);

        Assert.Equal("Value", propertyModels[0].Name);
        Assert.Equal("System.Func<int>", propertyModels[0].Type.Name);
        Assert.Equal("", propertyModels[0].Modifier);
        Assert.Equal("public", propertyModels[0].AccessModifier);
        Assert.True(propertyModels[0].IsEvent);

        var addAccessor = propertyModels[0].Accessors[0];
        Assert.Equal("add", addAccessor.Name);
        Assert.Equal(1, addAccessor.CalledMethods.Count);
        Assert.Equal("Triple", addAccessor.CalledMethods[0].Name);
        Assert.Equal("ExternClass", addAccessor.CalledMethods[0].DefinitionClassName);
        Assert.Equal("ExternClass", addAccessor.CalledMethods[0].DefinitionClassName);
        Assert.Equal(1, addAccessor.CalledMethods[0].ParameterTypes.Count);
        var parameterModel1 = (CSharpParameterModel)addAccessor.CalledMethods[0].ParameterTypes[0];
        Assert.Equal("", parameterModel1.Modifier);
        Assert.Equal("int", parameterModel1.Type.Name);
        Assert.Null(parameterModel1.DefaultValue);

        var removeAccessor = propertyModels[0].Accessors[1];
        Assert.Equal("remove", removeAccessor.Name);
        Assert.Equal(1, removeAccessor.CalledMethods.Count);
        Assert.Equal("Double", removeAccessor.CalledMethods[0].Name);
        Assert.Equal("ExternClass", removeAccessor.CalledMethods[0].DefinitionClassName);
        Assert.Equal("ExternClass", removeAccessor.CalledMethods[0].LocationClassName);
        Assert.Equal(1, removeAccessor.CalledMethods[0].ParameterTypes.Count);
        var parameterModel2 = (CSharpParameterModel)removeAccessor.CalledMethods[0].ParameterTypes[0];
        Assert.Equal("", parameterModel2.Modifier);
        Assert.Equal("System.Func<int>", parameterModel2.Type.Name);
        Assert.Null(parameterModel2.DefaultValue);
    }

    [Theory]
    [FileData("TestData/ClassWithEventPropertyThatCallsMethodFromAnotherClassAsProperty.txt")]
    public void
        Extract_ShouldHaveEventProperties_WhenGivenClassWithComputedPropertyThatCallsMethodsFromOtherClassFromTheSameNamespaceAsProperty(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(2, classTypes.Count);

        var propertyModels = ((CSharpClassModel)classTypes[1]).Properties;

        Assert.Equal(2, propertyModels.Count);

        var propertyModel = propertyModels[1];
        Assert.Equal("Value", propertyModel.Name);
        Assert.Equal("System.Func<string>", propertyModel.Type.Name);
        Assert.Equal("", propertyModel.Modifier);
        Assert.Equal("public", propertyModel.AccessModifier);
        Assert.True(propertyModel.IsEvent);

        var addAccessor = propertyModels[1].Accessors[0];
        Assert.Equal("add", addAccessor.Name);
        Assert.Equal(1, addAccessor.CalledMethods.Count);
        Assert.Equal("Convert", addAccessor.CalledMethods[0].Name);
        Assert.Equal("TopLevel.Bar", addAccessor.CalledMethods[0].DefinitionClassName);
        Assert.Equal("TopLevel.Bar", addAccessor.CalledMethods[0].LocationClassName);
        Assert.Equal(1, addAccessor.CalledMethods[0].ParameterTypes.Count);
        var parameterModel1 = (CSharpParameterModel)addAccessor.CalledMethods[0].ParameterTypes[0];
        Assert.Equal("", parameterModel1.Modifier);
        Assert.Equal("int", parameterModel1.Type.Name);
        Assert.Null(parameterModel1.DefaultValue);

        var removeAccessor = propertyModels[1].Accessors[1];
        Assert.Equal("remove", removeAccessor.Name);
        Assert.Equal(1, removeAccessor.CalledMethods.Count);
        Assert.Equal("Cut", removeAccessor.CalledMethods[0].Name);
        Assert.Equal("TopLevel.Bar", removeAccessor.CalledMethods[0].DefinitionClassName);
        Assert.Equal("TopLevel.Bar", removeAccessor.CalledMethods[0].LocationClassName);
        Assert.Equal(1, removeAccessor.CalledMethods[0].ParameterTypes.Count);
        var parameterModel2 = (CSharpParameterModel)removeAccessor.CalledMethods[0].ParameterTypes[0];
        Assert.Equal("", parameterModel2.Modifier);
        Assert.Equal("System.Func<string>", parameterModel2.Type.Name);
        Assert.Null(parameterModel2.DefaultValue);
    }

    [Theory]
    [FileData("TestData/MethodCallFromExternClass.txt")]
    public void Extract_ShouldHaveNoMethodDefinitionNames_GivenExternClass(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var propertyAccessor = ((CSharpClassModel)classTypes[0]).Properties[0].Accessors[0];

        Assert.Equal(1, propertyAccessor.CalledMethods.Count);

        Assert.Equal("Method", propertyAccessor.CalledMethods[0].Name);
        Assert.Equal("Extern", propertyAccessor.CalledMethods[0].DefinitionClassName);
        Assert.Equal("Extern", propertyAccessor.CalledMethods[0].LocationClassName);
        Assert.Empty(propertyAccessor.CalledMethods[0].MethodDefinitionNames);
    }
}

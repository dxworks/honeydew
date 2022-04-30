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

namespace Honeydew.Extractors.CSharp.Tests.FactExtractor;

public class CSharpClassFactExtractorsTests
{
    private readonly CSharpFactExtractor _sut;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly DotnetSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpClassFactExtractorsTests()
    {
        var calledMethodSetterVisitor = new CSharpCalledMethodSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IMethodCallType>>
            {
                new MethodCallInfoVisitor()
            });
        var parameterSetterVisitor = new CSharpParameterSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IParameterType>>
            {
                new ParameterInfoVisitor()
            });
        var returnValueSetterVisitor = new CSharpReturnValueSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IReturnValueType>>
            {
                new ReturnValueInfoVisitor()
            });
        var methodInfoVisitor = new MethodInfoVisitor();

        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpClassSetterVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new CSharpMethodSetterClassVisitor(_loggerMock.Object, new List<ITypeVisitor<IMethodType>>
                        {
                            methodInfoVisitor,
                            calledMethodSetterVisitor,
                            returnValueSetterVisitor,
                            parameterSetterVisitor,
                        }),
                        new CSharpConstructorSetterVisitor(_loggerMock.Object,
                            new List<ITypeVisitor<IConstructorType>>
                            {
                                new ConstructorInfoVisitor(),
                                calledMethodSetterVisitor,
                                parameterSetterVisitor,
                            }),
                        new CSharpFieldSetterClassVisitor(_loggerMock.Object, new List<ITypeVisitor<IFieldType>>
                        {
                            new FieldInfoVisitor()
                        }),
                        new CSharpPropertySetterClassVisitor(_loggerMock.Object, new List<ITypeVisitor<IPropertyType>>
                        {
                            new PropertyInfoVisitor(),
                            new CSharpAccessorMethodSetterPropertyVisitor(_loggerMock.Object,
                                new List<ITypeVisitor<IAccessorMethodType>>
                                {
                                    methodInfoVisitor,
                                    calledMethodSetterVisitor,
                                    returnValueSetterVisitor,
                                })
                        })
                    })
            });

        _sut = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData(null)]
    public void Extract_ShouldThrowEmptyContentException_WhenTryingToExtractFromEmptyString(string emptyContent)
    {
        var extractionException =
            Assert.Throws<ExtractionException>(() => _syntacticModelCreator.Create(emptyContent));
        Assert.Equal("Empty Content", extractionException.Message);
    }

    [Theory]
    [InlineData(@"namespace Models
                                     {
                                       public class Item
                                       
                                       }
                                     }
                                     ")]
    [InlineData(@"namespace Models
                                     {
                                       publizzc class Item
                                       {
                                             void a(){ }
                                       }
                                     }
                                     ")]
    [InlineData(@"namespace Models
                                     {
                                       public class Item
                                       {
                                             void a(){ int c}
                                       }
                                     }
                                     ")]
    public void Extract_ShouldThrowExtractionException_WhenParsingTextWithParsingErrors(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        Assert.Throws<ExtractionException>(() => _sut.Extract(syntaxTree, semanticModel).ClassTypes);
    }

    [Theory]
    [InlineData("class")]
    [InlineData("interface")]
    [InlineData("record")]
    [InlineData("struct")]
    public void Extract_ShouldSetClassNameAndNamespace_WhenParsingTextWithOneEntity(string entityType)
    {
        var fileContent = $@"        
                                     namespace Models.Main.Items
                                     {{
                                       public {entityType} MainItem
                                       {{
                                       }}
                                     }}
                                     ";

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        foreach (var classType in classTypes)
        {
            var classModel = (CSharpClassModel)classType;
            Assert.Equal(entityType, classModel.ClassType);
            Assert.Equal("Models.Main.Items", classModel.ContainingNamespaceName);
            Assert.Equal("Models.Main.Items.MainItem", classModel.Name);
        }
    }

    [Theory]
    [InlineData("public", "static")]
    [InlineData("private protected", "sealed")]
    [InlineData("protected internal", "")]
    [InlineData("private", "")]
    [InlineData("protected", "abstract")]
    [InlineData("internal", "new")]
    public void Extract_ShouldSetClassModifiers_WhenParsingTextWithOneEntity(string accessModifier, string modifier)
    {
        var fileContent = $@"        
                                     namespace Models.Main.Items
                                     {{
                                       {accessModifier} {modifier} class MainItem
                                       {{
                                       }}
                                     }}";

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        foreach (var classType in classTypes)
        {
            var classModel = (CSharpClassModel)classType;
            Assert.Equal("Models.Main.Items", classModel.ContainingNamespaceName);
            Assert.Equal("Models.Main.Items.MainItem", classModel.Name);
            Assert.Equal(accessModifier, classModel.AccessModifier);
            Assert.Equal(modifier, classModel.Modifier);
        }
    }

    [Theory]
    [InlineData("in")]
    [InlineData("out")]
    [InlineData("ref")]
    public void Extract_ShouldSetParameters_WhenParsingTextWithOneClassWithMethodWithParameterWithModifiers(
        string parameterModifier)
    {
        var fileContent = $@"        
                                     namespace Models.Main.Items
                                     {{
                                       public class MainItem
                                       {{
                                             public void Method({parameterModifier} int a) {{}}

                                             public MainItem({parameterModifier} int a) {{ }}
                                       }}
                                     }}";

        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        foreach (var classType in classTypes)
        {
            var classModel = (CSharpClassModel)classType;
            Assert.Equal("Models.Main.Items", classModel.ContainingNamespaceName);
            Assert.Equal("Models.Main.Items.MainItem", classModel.Name);

            Assert.Equal(1, classModel.Methods.Count);
            Assert.Equal(1, classModel.Methods[0].ParameterTypes.Count);
            var parameterModel = (CSharpParameterModel)classModel.Methods[0].ParameterTypes[0];
            Assert.Equal("int", parameterModel.Type.Name);
            Assert.Equal(parameterModifier, parameterModel.Modifier);
            Assert.Null(parameterModel.DefaultValue);

            Assert.Equal(1, classModel.Constructors.Count);
            Assert.Equal(1, classModel.Constructors[0].ParameterTypes.Count);
            var parameterModelConstructor = (CSharpParameterModel)classModel.Constructors[0].ParameterTypes[0];
            Assert.Equal("int", parameterModelConstructor.Type.Name);
            Assert.Equal(parameterModifier, parameterModelConstructor.Modifier);
            Assert.Null(parameterModelConstructor.DefaultValue);
        }
    }

    [Theory]
    [FileData("TestData/ClassWithOneExtensionMethod.txt")]
    public void Extract_ShouldSetParameters_WhenParsingTextWithOneClassWithExtensionMethod(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        foreach (var classType in classTypes)
        {
            var classModel = (CSharpClassModel)classType;
            Assert.Equal("Models.Main.Items", classModel.ContainingNamespaceName);
            Assert.Equal("Models.Main.Items.MainItem", classModel.Name);
            Assert.Equal(1, classModel.Methods.Count);
            Assert.Equal(1, classModel.Methods[0].ParameterTypes.Count);
            var parameterModel = (CSharpParameterModel)classModel.Methods[0].ParameterTypes[0];
            Assert.Equal("int", parameterModel.Type.Name);
            Assert.Equal("this", parameterModel.Modifier);
            Assert.Null(parameterModel.DefaultValue);
        }
    }

    [Theory]
    [FileData("TestData/ClassWithOneMethodWithParams.txt")]
    public void Extract_ShouldSetParameters_WhenParsingTextWithOneClassWithMethodWithParameterWithParamsModifiers(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        foreach (var classType in classTypes)
        {
            var classModel = (CSharpClassModel)classType;
            Assert.Equal("Models.Main.Items", classModel.ContainingNamespaceName);
            Assert.Equal("Models.Main.Items.MainItem", classModel.Name);

            Assert.Equal(1, classModel.Methods.Count);
            Assert.Equal(1, classModel.Methods[0].ParameterTypes.Count);
            var parameterModel = (CSharpParameterModel)classModel.Methods[0].ParameterTypes[0];
            Assert.Equal("int[]", parameterModel.Type.Name);
            Assert.Equal("params", parameterModel.Modifier);
            Assert.Null(parameterModel.DefaultValue);

            Assert.Equal(1, classModel.Constructors.Count);
            Assert.Equal(1, classModel.Constructors[0].ParameterTypes.Count);
            var parameterModelConstructor = (CSharpParameterModel)classModel.Constructors[0].ParameterTypes[0];
            Assert.Equal("int[]", parameterModelConstructor.Type.Name);
            Assert.Equal("params", parameterModelConstructor.Modifier);
            Assert.Null(parameterModelConstructor.DefaultValue);
        }
    }

    [Theory]
    [FileData("TestData/ClassWithMethodsWithDefaultValue.txt")]
    public void Extract_ShouldSetParameters_WhenParsingTextWithOneClassWithMethodWithParameterWithDefaultValues(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        foreach (var classType in classTypes)
        {
            var classModel = (CSharpClassModel)classType;
            Assert.Equal(6, classModel.Methods.Count);

            Assert.Empty(classModel.Methods[0].ParameterTypes);

            Assert.Equal(1, classModel.Methods[1].ParameterTypes.Count);
            var method1Parameter = (CSharpParameterModel)classModel.Methods[1].ParameterTypes[0];
            Assert.Equal("object", method1Parameter.Type.Name);
            Assert.Equal("", method1Parameter.Modifier);
            Assert.Equal("null", method1Parameter.DefaultValue);

            Assert.Equal(1, classModel.Methods[2].ParameterTypes.Count);
            var method2Parameter = (CSharpParameterModel)classModel.Methods[2].ParameterTypes[0];
            Assert.Equal("int", method2Parameter.Type.Name);
            Assert.Equal("", method2Parameter.Modifier);
            Assert.Equal("15", method2Parameter.DefaultValue);

            Assert.Equal(2, classModel.Methods[3].ParameterTypes.Count);
            foreach (var parameterType in classModel.Methods[3].ParameterTypes)
            {
                var parameterModel = (CSharpParameterModel)parameterType;
                Assert.Equal("int", parameterModel.Type.Name);
                Assert.Equal("", parameterModel.Modifier);
                Assert.Null(parameterModel.DefaultValue);
            }

            Assert.Equal(3, classModel.Methods[4].ParameterTypes.Count);
            var method4Parameter0 = (CSharpParameterModel)classModel.Methods[4].ParameterTypes[0];
            Assert.Equal("int", method4Parameter0.Type.Name);
            Assert.Equal("", method4Parameter0.Modifier);
            Assert.Null(method4Parameter0.DefaultValue);

            var method4Parameter1 = (CSharpParameterModel)classModel.Methods[4].ParameterTypes[1];
            Assert.Equal("int", method4Parameter1.Type.Name);
            Assert.Equal("in", method4Parameter1.Modifier);
            Assert.Equal("15", method4Parameter1.DefaultValue);

            var method4Parameter2 = (CSharpParameterModel)classModel.Methods[4].ParameterTypes[2];
            Assert.Equal("string", method4Parameter2.Type.Name);
            Assert.Equal("", method4Parameter2.Modifier);
            Assert.Equal("\"\"", method4Parameter2.DefaultValue);

            Assert.Equal(1, classModel.Methods[5].ParameterTypes.Count);
            var method5Parameter = (CSharpParameterModel)classModel.Methods[5].ParameterTypes[0];
            Assert.Equal("string", method5Parameter.Type.Name);
            Assert.Equal("", method5Parameter.Modifier);
            Assert.Equal("\"null\"", method5Parameter.DefaultValue);
        }
    }


    [Theory]
    [FileData("TestData/ClassWithOneMethodWithParams.txt")]
    [FileData("TestData/InterfaceWithMethods.txt")]
    public void Extract_ShouldNotHaveMetrics_WhenGivenAnEmptyListOfMetrics_ForOneClass(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

        foreach (var classType in classTypes)
        {
            var classModel = (CSharpClassModel)classType;
            Assert.Equal(typeof(CSharpClassModel), classModel.GetType());

            Assert.Empty(classModel.Metrics);
        }
    }

    [Theory]
    [FileData("TestData/NamespaceWithClassAndInterface.txt")]
    public void Extract_ShouldSetClassNameAndInterfaceAndNamespace_WhenParsingTextWithOneClassAndOneInterface(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(2, classTypes.Count);

        foreach (var classType in classTypes)
        {
            var classModel = (CSharpClassModel)classType;
            Assert.Equal(typeof(CSharpClassModel), classModel.GetType());
        }

        Assert.Equal("Models.Main.Items", ((CSharpClassModel)classTypes[0]).ContainingNamespaceName);
        Assert.Equal("Models.Main.Items.MainItem", classTypes[0].Name);

        Assert.Equal("Models.Main.Items", ((CSharpClassModel)classTypes[1]).ContainingNamespaceName);
        Assert.Equal("Models.Main.Items.IInterface", classTypes[1].Name);
    }

    [Theory]
    [FileData("TestData/NamespaceWithMultipleClassesWithNoFields.txt")]
    [FileData("TestData/NamespaceWithMultipleInterfaces.txt")]
    public void Extract_ShouldHaveNoFields_WhenGivenAnInterface(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(3, classTypes.Count);

        foreach (var classType in classTypes)
        {
            var classModel = (CSharpClassModel)classType;
            Assert.Empty(classModel.Fields);
        }
    }

    [Theory]
    [FileData("TestData/ClassWithFieldsWithNoAccessModifier.txt")]
    public void Extract_ShouldHavePrivateFieldsWithModifiers_WhenGivenClassWithFieldsAndModifiersWithDefaultAccess(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var fieldInfos = ((CSharpClassModel)classTypes[0]).Fields;

        Assert.Equal(3, fieldInfos.Count);

        Assert.Equal("A", fieldInfos[0].Name);
        Assert.Equal("int", fieldInfos[0].Type.Name);
        Assert.Equal("readonly", fieldInfos[0].Modifier);
        Assert.Equal("private", fieldInfos[0].AccessModifier);
        Assert.False(((CSharpFieldModel)fieldInfos[0]).IsEvent);

        Assert.Equal("X", fieldInfos[1].Name);
        Assert.Equal("float", fieldInfos[1].Type.Name);
        Assert.Equal("volatile", fieldInfos[1].Modifier);
        Assert.Equal("private", fieldInfos[1].AccessModifier);
        Assert.False(((CSharpFieldModel)fieldInfos[1]).IsEvent);

        Assert.Equal("Y", fieldInfos[2].Name);
        Assert.Equal("string", fieldInfos[2].Type.Name);
        Assert.Equal("static", fieldInfos[2].Modifier);
        Assert.Equal("private", fieldInfos[2].AccessModifier);
        Assert.False(((CSharpFieldModel)fieldInfos[2]).IsEvent);
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

        var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var fieldInfos = ((CSharpClassModel)classTypes[0]).Fields;

        Assert.Equal(5, fieldInfos.Count);

        Assert.Equal("AnimalNest", fieldInfos[0].Name);
        Assert.Equal("int", fieldInfos[0].Type.Name);
        Assert.Equal("", fieldInfos[0].Modifier);
        Assert.Equal(modifier, fieldInfos[0].AccessModifier);
        Assert.False(((CSharpFieldModel)fieldInfos[0]).IsEvent);

        Assert.Equal("X", fieldInfos[1].Name);
        Assert.Equal("float", fieldInfos[1].Type.Name);
        Assert.Equal("", fieldInfos[1].Modifier);
        Assert.Equal(modifier, fieldInfos[1].AccessModifier);
        Assert.False(((CSharpFieldModel)fieldInfos[1]).IsEvent);

        Assert.Equal("Yaz_fafa", fieldInfos[2].Name);
        Assert.Equal("float", fieldInfos[2].Type.Name);
        Assert.Equal("", fieldInfos[2].Modifier);
        Assert.Equal(modifier, fieldInfos[2].AccessModifier);
        Assert.False(((CSharpFieldModel)fieldInfos[2]).IsEvent);

        Assert.Equal("_zxy", fieldInfos[3].Name);
        Assert.Equal("string", fieldInfos[3].Type.Name);
        Assert.Equal("", fieldInfos[3].Modifier);
        Assert.Equal(modifier, fieldInfos[3].AccessModifier);
        Assert.False(((CSharpFieldModel)fieldInfos[3]).IsEvent);

        Assert.Equal("extractor", fieldInfos[4].Name);
        Assert.Equal("CSharpMetricExtractor", fieldInfos[4].Type.Name);
        Assert.Equal("", fieldInfos[4].Modifier);
        Assert.Equal(modifier, fieldInfos[4].AccessModifier);
        Assert.False(((CSharpFieldModel)fieldInfos[4]).IsEvent);
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

        var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var fieldInfos = ((CSharpClassModel)classTypes[0]).Fields;

        Assert.Equal(4, fieldInfos.Count);

        Assert.Equal("extractor", fieldInfos[0].Name);
        Assert.Equal("CSharpMetricExtractor", fieldInfos[0].Type.Name);
        Assert.Equal("", fieldInfos[0].Modifier);
        Assert.Equal(visibility, fieldInfos[0].AccessModifier);
        Assert.True(((CSharpFieldModel)fieldInfos[0]).IsEvent);

        Assert.Equal("_some_event", fieldInfos[1].Name);
        Assert.Equal("int", fieldInfos[1].Type.Name);
        Assert.Equal("", fieldInfos[1].Modifier);
        Assert.Equal(visibility, fieldInfos[1].AccessModifier);
        Assert.True(((CSharpFieldModel)fieldInfos[1]).IsEvent);

        Assert.Equal("MyAction1", fieldInfos[2].Name);
        Assert.Equal("System.Action", fieldInfos[2].Type.Name);
        Assert.Equal("", fieldInfos[2].Modifier);
        Assert.Equal(visibility, fieldInfos[2].AccessModifier);
        Assert.True(((CSharpFieldModel)fieldInfos[2]).IsEvent);

        Assert.Equal("MyAction2", fieldInfos[3].Name);
        Assert.Equal("System.Action", fieldInfos[3].Type.Name);
        Assert.Equal("", fieldInfos[3].Modifier);
        Assert.Equal(visibility, fieldInfos[3].AccessModifier);
        Assert.True(((CSharpFieldModel)fieldInfos[3]).IsEvent);
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

        var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var fieldInfos = ((CSharpClassModel)classTypes[0]).Fields;

        Assert.Equal(5, fieldInfos.Count);

        Assert.Equal("AnimalNest", fieldInfos[0].Name);
        Assert.Equal("int", fieldInfos[0].Type.Name);
        Assert.Equal(modifier, fieldInfos[0].Modifier);
        Assert.Equal("public", fieldInfos[0].AccessModifier);
        Assert.False(((CSharpFieldModel)fieldInfos[0]).IsEvent);

        Assert.Equal("X", fieldInfos[1].Name);
        Assert.Equal("float", fieldInfos[1].Type.Name);
        Assert.Equal(modifier, fieldInfos[1].Modifier);
        Assert.Equal("protected", fieldInfos[1].AccessModifier);
        Assert.False(((CSharpFieldModel)fieldInfos[1]).IsEvent);

        Assert.Equal("Yaz_fafa", fieldInfos[2].Name);
        Assert.Equal("float", fieldInfos[2].Type.Name);
        Assert.Equal(modifier, fieldInfos[2].Modifier);
        Assert.Equal("protected", fieldInfos[2].AccessModifier);
        Assert.False(((CSharpFieldModel)fieldInfos[2]).IsEvent);

        Assert.Equal("_zxy", fieldInfos[3].Name);
        Assert.Equal("string", fieldInfos[3].Type.Name);
        Assert.Equal(modifier, fieldInfos[3].Modifier);
        Assert.Equal("private", fieldInfos[3].AccessModifier);
        Assert.False(((CSharpFieldModel)fieldInfos[3]).IsEvent);

        Assert.Equal("extractor", fieldInfos[4].Name);
        Assert.Equal("CSharpMetricExtractor", fieldInfos[4].Type.Name);
        Assert.Equal(modifier, fieldInfos[4].Modifier);
        Assert.Equal("private", fieldInfos[4].AccessModifier);
        Assert.False(((CSharpFieldModel)fieldInfos[4]).IsEvent);
    }

    [Theory]
    [FileData("TestData/StructWithNoMethods.txt")]
    [FileData("TestData/RecordWithNoMethods.txt")]
    public void Extract_ShouldHaveNoMethods_WhenGivenClassTypeWithNoMethods(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);
        Assert.Empty(((CSharpClassModel)classTypes[0]).Methods);
    }

    [Theory]
    [FileData("TestData/InterfaceWithMethods.txt")]
    public void Extract_ShouldHaveMethods_WhenGivenAnInterfaceWithMethods(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);
        var classModel = (CSharpClassModel)classTypes[0];
        Assert.Equal(3, classModel.Methods.Count);

        var methodF = classModel.Methods[0];
        Assert.Equal("f", methodF.Name);
        Assert.Equal("CSharpExtractor", methodF.ReturnValue.Type.Name);
        Assert.Equal("", ((CSharpReturnValueModel)methodF.ReturnValue).Modifier);
        Assert.Equal(1, methodF.ParameterTypes.Count);
        var parameterModel1 = (CSharpParameterModel)methodF.ParameterTypes[0];
        Assert.Equal("int", parameterModel1.Type.Name);
        Assert.Equal("", parameterModel1.Modifier);
        Assert.Null(parameterModel1.DefaultValue);
        Assert.Equal("public", methodF.AccessModifier);
        Assert.Equal("abstract", methodF.Modifier);
        Assert.Empty(methodF.CalledMethods);

        var methodG = classModel.Methods[1];
        Assert.Equal("g", methodG.Name);
        Assert.Equal("int", methodG.ReturnValue.Type.Name);
        Assert.Equal("", ((CSharpReturnValueModel)methodG.ReturnValue).Modifier);
        Assert.Equal(1, methodG.ParameterTypes.Count);
        var parameterModel2 = (CSharpParameterModel)methodG.ParameterTypes[0];
        Assert.Equal("CSharpExtractor", parameterModel2.Type.Name);
        Assert.Equal("", parameterModel2.Modifier);
        Assert.Null(parameterModel2.DefaultValue);
        Assert.Equal("public", methodG.AccessModifier);
        Assert.Equal("abstract", methodG.Modifier);
        Assert.Empty(methodG.CalledMethods);

        var methodH = classModel.Methods[2];
        Assert.Equal("h", methodH.Name);
        Assert.Equal("string", methodH.ReturnValue.Type.Name);
        Assert.Equal("", ((CSharpReturnValueModel)methodH.ReturnValue).Modifier);
        Assert.Equal(2, methodH.ParameterTypes.Count);
        var parameterModel3 = (CSharpParameterModel)methodH.ParameterTypes[0];
        Assert.Equal("float", parameterModel3.Type.Name);
        Assert.Equal("", parameterModel3.Modifier);
        Assert.Null(parameterModel3.DefaultValue);
        var parameterModel4 = (CSharpParameterModel)methodH.ParameterTypes[1];
        Assert.Equal("CSharpExtractor", parameterModel4.Type.Name);
        Assert.Equal("", parameterModel4.Modifier);
        Assert.Null(parameterModel4.DefaultValue);
        Assert.Equal("public", methodH.AccessModifier);
        Assert.Equal("abstract", methodH.Modifier);
        Assert.Empty(methodH.CalledMethods);
    }

    [Theory]
    [FileData("TestData/ClassWithMethods.txt")]
    public void Extract_ShouldHaveMethods_WhenGivenAClassWithMethods(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);
        var classModel = (CSharpClassModel)classTypes[0];
        Assert.Equal(3, classModel.Methods.Count);

        Assert.Equal("f", classModel.Methods[0].Name);
        Assert.Equal("void", classModel.Methods[0].ReturnValue.Type.Name);
        Assert.Equal("", ((CSharpReturnValueModel)classModel.Methods[0].ReturnValue).Modifier);
        Assert.Equal(1, classModel.Methods[0].ParameterTypes.Count);
        var parameterModel1 = (CSharpParameterModel)classModel.Methods[0].ParameterTypes[0];
        Assert.Equal("int", parameterModel1.Type.Name);
        Assert.Equal("", parameterModel1.Modifier);
        Assert.Null(parameterModel1.DefaultValue);
        Assert.Equal("public", classModel.Methods[0].AccessModifier);
        Assert.Equal("static", classModel.Methods[0].Modifier);
        Assert.Empty(classModel.Methods[0].CalledMethods);

        Assert.Equal("g", classModel.Methods[1].Name);
        Assert.Equal("int", classModel.Methods[1].ReturnValue.Type.Name);
        Assert.Equal("", ((CSharpReturnValueModel)classModel.Methods[1].ReturnValue).Modifier);
        Assert.Equal(1, classModel.Methods[1].ParameterTypes.Count);
        var parameterModel2 = (CSharpParameterModel)classModel.Methods[1].ParameterTypes[0];
        Assert.Equal("CSharpExtractor", parameterModel2.Type.Name);
        Assert.Equal("", parameterModel2.Modifier);
        Assert.Null(parameterModel2.DefaultValue);
        Assert.Equal("private", classModel.Methods[1].AccessModifier);
        Assert.Equal("", classModel.Methods[1].Modifier);
        Assert.Equal(1, classModel.Methods[1].CalledMethods.Count);
        Assert.Equal("f", classModel.Methods[1].CalledMethods[0].Name);
        Assert.Equal("TopLevel.Foo", classModel.Methods[1].CalledMethods[0].DefinitionClassName);
        Assert.Equal("TopLevel.Foo", classModel.Methods[1].CalledMethods[0].LocationClassName);

        Assert.Equal("h", classModel.Methods[2].Name);
        Assert.Equal("string", classModel.Methods[2].ReturnValue.Type.Name);
        Assert.Equal("", ((CSharpReturnValueModel)classModel.Methods[2].ReturnValue).Modifier);
        Assert.Equal(2, classModel.Methods[2].ParameterTypes.Count);
        var parameterModel3 = (CSharpParameterModel)classModel.Methods[2].ParameterTypes[0];
        Assert.Equal("float", parameterModel3.Type.Name);
        Assert.Equal("", parameterModel3.Modifier);
        Assert.Null(parameterModel3.DefaultValue);
        var parameterModel4 = (CSharpParameterModel)classModel.Methods[2].ParameterTypes[1];
        Assert.Equal("CSharpExtractor", parameterModel4.Type.Name);
        Assert.Equal("", parameterModel4.Modifier);
        Assert.Null(parameterModel4.DefaultValue);
        Assert.Equal("protected", classModel.Methods[2].AccessModifier);
        Assert.Equal("", classModel.Methods[2].Modifier);
        Assert.Equal(2, classModel.Methods[2].CalledMethods.Count);
        Assert.Equal("g", classModel.Methods[2].CalledMethods[0].Name);
        Assert.Equal("TopLevel.Foo", classModel.Methods[2].CalledMethods[0].DefinitionClassName);
        Assert.Equal("TopLevel.Foo", classModel.Methods[2].CalledMethods[0].LocationClassName);
        Assert.Equal("f", classModel.Methods[2].CalledMethods[1].Name);
        Assert.Equal("TopLevel.Foo", classModel.Methods[2].CalledMethods[1].DefinitionClassName);
        Assert.Equal("TopLevel.Foo", classModel.Methods[2].CalledMethods[1].LocationClassName);
    }

    [Theory]
    [FileData("TestData/ReadonlyStructs.txt")]
    public void Extract_ShouldHaveReadonlyStructs_WhenGivenPathToAFileWithReadonlyStructs(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(2, classTypes.Count);
        Assert.Equal("Points.ReadonlyPoint3D", classTypes[0].Name);
        Assert.Equal("Points.ReadonlyPoint2D", classTypes[1].Name);

        foreach (var classType in classTypes)
        {
            var classModel = (CSharpClassModel)classType;
            Assert.Equal("struct", classModel.ClassType);
            Assert.Equal("public", classModel.AccessModifier);
            Assert.Equal("readonly", classModel.Modifier);
        }
    }

    [Theory]
    [FileData("TestData/MutableStructWithReadonlyMembers.txt")]
    public void Extract_ShouldHaveReadonlyStructMembers_WhenGivenPathToAFileWithMutableStructWithReadonlyMembers(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classModels = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classModels.Count);
        var classModel = (CSharpClassModel)classModels[0];
        Assert.Equal("Geometry.Point2D", classModel.Name);

        Assert.Equal("struct", classModel.ClassType);
        Assert.Equal("public", classModel.AccessModifier);
        Assert.Equal("", classModel.Modifier);

        Assert.Equal(1, classModel.Methods.Count);
        Assert.Equal("ToString", classModel.Methods[0].Name);
        Assert.Equal("string", classModel.Methods[0].ReturnValue.Type.Name);
        Assert.Equal("", ((CSharpReturnValueModel)classModel.Methods[0].ReturnValue).Modifier);
        Assert.Equal("public", classModel.Methods[0].AccessModifier);
        Assert.Equal("readonly override", classModel.Methods[0].Modifier);

        Assert.Equal(3, classModel.Properties.Count);

        foreach (var propertyModel in classModel.Properties)
        {
            Assert.Equal("double", propertyModel.Type.Name);
            Assert.Equal("public", propertyModel.AccessModifier);
        }

        Assert.Equal("X", classModel.Properties[0].Name);
        Assert.Equal("", classModel.Properties[0].Modifier);

        Assert.Equal(2, classModel.Properties[0].Accessors.Count);
        Assert.Equal("get", classModel.Properties[0].Accessors[0].Name);
        Assert.Equal("readonly", classModel.Properties[0].Accessors[0].Modifier);
        Assert.Equal("public", classModel.Properties[0].Accessors[0].AccessModifier);
        Assert.Equal("double", classModel.Properties[0].Accessors[0].ReturnValue.Type.Name);

        Assert.Equal("set", classModel.Properties[0].Accessors[1].Name);
        Assert.Equal("", classModel.Properties[0].Accessors[1].Modifier);
        Assert.Equal("public", classModel.Properties[0].Accessors[1].AccessModifier);
        Assert.Equal("void", classModel.Properties[0].Accessors[1].ReturnValue.Type.Name);

        Assert.Equal("Y", classModel.Properties[1].Name);
        Assert.Equal("", classModel.Properties[1].Modifier);
        Assert.Equal(2, classModel.Properties[1].Accessors.Count);
        Assert.Equal("get", classModel.Properties[1].Accessors[0].Name);
        Assert.Equal("readonly", classModel.Properties[1].Accessors[0].Modifier);
        Assert.Equal("public", classModel.Properties[1].Accessors[0].AccessModifier);
        Assert.Equal("double", classModel.Properties[1].Accessors[0].ReturnValue.Type.Name);

        Assert.Equal("set", classModel.Properties[1].Accessors[1].Name);
        Assert.Equal("", classModel.Properties[1].Accessors[1].Modifier);
        Assert.Equal("public", classModel.Properties[1].Accessors[1].AccessModifier);
        Assert.Equal("void", classModel.Properties[1].Accessors[1].ReturnValue.Type.Name);

        Assert.Equal("Distance", classModel.Properties[2].Name);
        Assert.Equal("readonly", classModel.Properties[2].Modifier);
        Assert.Equal(1, classModel.Properties[2].Accessors.Count);

        Assert.Equal("get", classModel.Properties[2].Accessors[0].Name);
        Assert.Equal("", classModel.Properties[2].Accessors[0].Modifier);
        Assert.Equal("public", classModel.Properties[2].Accessors[0].AccessModifier);
        Assert.Equal("double", classModel.Properties[2].Accessors[0].ReturnValue.Type.Name);
    }

    [Theory]
    [FileData("TestData/StructWithRefReadonlyStaticMembers.txt")]
    public void
        Extract_ShouldHaveReadonlyStaticStructMembers_WhenGivenPathToAFileWithStructWithStaticReadonlyMembers(
            string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);
        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal("Geometry.Point", classModel.Name);
        Assert.Equal("struct", classModel.ClassType);
        Assert.Equal("public", classModel.AccessModifier);
        Assert.Equal("", classModel.Modifier);

        Assert.Equal(2, classModel.Properties.Count);

        foreach (var propertyModel in classModel.Properties)
        {
            Assert.Equal("Geometry.Point", propertyModel.Type.Name);
            Assert.Equal("public", propertyModel.AccessModifier);
            Assert.Equal(1, propertyModel.Accessors.Count);
            Assert.Equal("get", propertyModel.Accessors[0].Name);
            Assert.Equal("Geometry.Point", propertyModel.Accessors[0].ReturnValue.Type.Name);
        }

        Assert.Equal("Origin", classModel.Properties[0].Name);
        Assert.Equal("ref", classModel.Properties[0].Modifier);

        Assert.Equal("Origin2", classModel.Properties[1].Name);
        Assert.Equal("static ref readonly", classModel.Properties[1].Modifier);
    }

    [Theory]
    [FileData("TestData/RefStructs.txt")]
    public void Extract_ShouldHaveRefStructs_WhenGivenPathToAFileWithRefStructs(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(2, classTypes.Count);

        var classModel1 = (CSharpClassModel)classTypes[0];
        Assert.Equal("Namespace1.CustomRef", classModel1.Name);
        Assert.Equal("struct", classModel1.ClassType);
        Assert.Equal("public", classModel1.AccessModifier);
        Assert.Equal("ref", classModel1.Modifier);

        Assert.Equal(2, classModel1.Fields.Count);

        foreach (var fieldModel in classModel1.Fields)
        {
            Assert.Equal("public", fieldModel.AccessModifier);
            Assert.Equal("System.Span<int>", fieldModel.Type.Name);
        }

        var classModel2 = (CSharpClassModel)classTypes[1];
        Assert.Equal("Namespace1.ConversionRequest", classModel2.Name);
        Assert.Equal("struct", classModel2.ClassType);
        Assert.Equal("public", classModel2.AccessModifier);
        Assert.Equal("readonly ref", classModel2.Modifier);

        Assert.Equal(2, classModel2.Properties.Count);

        foreach (var propertyModel in classModel2.Properties)
        {
            Assert.Equal("public", propertyModel.AccessModifier);
            Assert.Equal("", propertyModel.Modifier);
            Assert.Equal(1, propertyModel.Accessors.Count);
            Assert.Equal("get", propertyModel.Accessors[0].Name);
        }

        Assert.Equal("Rate", classModel2.Properties[0].Name);
        Assert.Equal("double", classModel2.Properties[0].Type.Name);

        Assert.Equal("Values", classModel2.Properties[1].Name);
        Assert.Equal("System.ReadOnlySpan<double>", classModel2.Properties[1].Type.Name);
    }

    [Theory]
    [FileData("TestData/RefReturnMethods.txt")]
    public void Extract_ShouldHaveRefReturnMethod_WhenGivenPathToAFileWithRefStructs(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(1, classTypes.Count);

        var classModel = (CSharpClassModel)classTypes[0];
        Assert.Equal("Namespace1.Class1", classModel.Name);

        Assert.Equal(2, classModel.Methods.Count);

        foreach (var methodModel in classModel.Methods)
        {
            Assert.Equal("Find", methodModel.Name);
            Assert.Equal("static", methodModel.Modifier);
            Assert.Equal("public", methodModel.AccessModifier);
            Assert.Equal(2, methodModel.ParameterTypes.Count);
            Assert.Equal("int[*,*]", methodModel.ParameterTypes[0].Type.Name);
            Assert.Equal("int", methodModel.ReturnValue.Type.Name);
        }

        Assert.Equal("ref", ((CSharpReturnValueModel)classModel.Methods[0].ReturnValue).Modifier);
        Assert.Equal("System.Func<int, bool>", classModel.Methods[0].ParameterTypes[1].Type.Name);

        Assert.Equal("ref readonly", ((CSharpReturnValueModel)classModel.Methods[1].ReturnValue).Modifier);
        Assert.Equal("bool", classModel.Methods[1].ParameterTypes[1].Type.Name);
    }

    [Theory]
    [FileData("TestData/ClassWithNullableEntities.txt")]
    public void Extract_ShouldHaveNullableEntities_WhenProvidedWithClassWithNullableEntities(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal("Namespace1.Class1", classModel.Name);

        var types = new[]
        {
            classModel.Fields[0].Type,
            classModel.Properties[0].Type,
            classModel.Methods[0].ReturnValue.Type,
            classModel.Methods[0].ParameterTypes[0].Type,
            classModel.Constructors[0].ParameterTypes[0].Type,
        };

        foreach (var type in types)
        {
            Assert.Equal("int?", type.Name);
            Assert.Equal("int", type.FullType.Name);
            Assert.True(type.FullType.IsNullable);
        }

        Assert.True(classModel.Fields[0].IsNullable);
        Assert.True(classModel.Properties[0].IsNullable);
        Assert.True(classModel.Methods[0].ReturnValue.IsNullable);
        Assert.True(classModel.Methods[0].ParameterTypes[0].IsNullable);
        Assert.True(classModel.Constructors[0].ParameterTypes[0].IsNullable);
    }

    [Theory]
    [FileData("TestData/ClassWithNullableClassEntities.txt")]
    public void Extract_ShouldHaveNullableEntities_WhenProvidedWithClassWithNullableClassEntities(
        string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal("Namespace1.Class1", classModel.Name);

        var types = new[]
        {
            classModel.Fields[0].Type,
            classModel.Properties[0].Type,
            classModel.Methods[0].ReturnValue.Type,
            classModel.Methods[0].ParameterTypes[0].Type,
            classModel.Constructors[0].ParameterTypes[0].Type,
        };

        foreach (var type in types)
        {
            Assert.Equal("Namespace1.Class2?", type.Name);
            Assert.Equal("Namespace1.Class2", type.FullType.Name);
            Assert.True(type.FullType.IsNullable);
        }

        Assert.True(classModel.Fields[0].IsNullable);
        Assert.True(classModel.Properties[0].IsNullable);
        Assert.True(classModel.Methods[0].ReturnValue.IsNullable);
        Assert.True(classModel.Methods[0].ParameterTypes[0].IsNullable);
        Assert.True(classModel.Constructors[0].ParameterTypes[0].IsNullable);
    }


    [Theory]
    [FileData("TestData/ClassWithNestedClasses.txt")]
    public void Extract_ShouldHaveContainingClassNames_GivenNestedClasses(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);

        var classTypes = _sut.Extract(syntaxTree, semanticModel).ClassTypes;

        Assert.Equal(5, classTypes.Count);

        Assert.Equal("Namespace1", classTypes[0].ContainingNamespaceName);
        Assert.Equal("", classTypes[0].ContainingClassName);

        Assert.Equal("Namespace1", classTypes[1].ContainingNamespaceName);
        Assert.Equal("Namespace1.Class1", classTypes[1].ContainingClassName);

        Assert.Equal("Namespace1", classTypes[2].ContainingNamespaceName);
        Assert.Equal("Namespace1.Class1.Class2", classTypes[2].ContainingClassName);

        Assert.Equal("Namespace1", classTypes[3].ContainingNamespaceName);
        Assert.Equal("Namespace1.Class1.Class2", classTypes[3].ContainingClassName);

        Assert.Equal("Namespace1", classTypes[4].ContainingNamespaceName);
        Assert.Equal("Namespace1.Class1.Class2.S1", classTypes[4].ContainingClassName);
    }
}

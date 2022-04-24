using System.Collections.Generic;
using Honeydew.Extractors.CSharp.Visitors.Concrete;
using Honeydew.Extractors.CSharp.Visitors.Setters;
using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Moq;
using Xunit;

namespace Honeydew.Extractors.CSharp.Tests.Visitors.Class.GenericType;

public class CSharpGenericClassTests
{
    private readonly CSharpFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly CSharpSyntacticModelCreator _syntacticModelCreator = new();
    private readonly CSharpSemanticModelCreator _semanticModelCreator = new(new CSharpCompilationMaker());

    public CSharpGenericClassTests()
    {
        var compositeVisitor = new CSharpCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new CSharpClassSetterCompilationUnitVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new BaseTypesClassVisitor(),
                        new CSharpGenericParameterSetterVisitor(_loggerMock.Object,
                            new List<ITypeVisitor<IGenericParameterType>>
                            {
                                new GenericParameterInfoVisitor()
                            })
                    })
            });

        _factExtractor = new CSharpFactExtractor(compositeVisitor);
    }

    [Theory]
    [InlineData("class")]
    [InlineData("record")]
    [InlineData("struct")]
    [InlineData("interface")]
    public void Extract_ShouldHaveClassNameOfGenericType_WhenProvidedDifferentClassType(string classType)
    {
        var fileContent = $@"namespace Namespace1
{{
    public {classType} Class1<T>  {{ }}
}}";
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal("Namespace1.Class1<T>", classModel.Name);
        Assert.Equal(1, classModel.GenericParameters.Count);
        Assert.Equal("T", classModel.GenericParameters[0].Name);
        Assert.Equal("", classModel.GenericParameters[0].Modifier);
        Assert.Empty(classModel.GenericParameters[0].Constraints);
    }


    [Theory]
    [InlineData("class")]
    [InlineData("record")]
    [InlineData("struct")]
    [InlineData("interface")]
    public void Extract_ShouldHaveClassNameGenericTypeWithMultipleContainedTypes_WhenProvidedDifferentClassType(
        string classType)
    {
        var fileContent = $@"namespace Namespace1
{{
    public {classType} Class1<T,R,K> {{ }}
}}";
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal("Namespace1.Class1<T,R,K>", classModel.Name);
        Assert.Equal(3, classModel.GenericParameters.Count);
        Assert.Equal("T", classModel.GenericParameters[0].Name);
        Assert.Equal("R", classModel.GenericParameters[1].Name);
        Assert.Equal("K", classModel.GenericParameters[2].Name);
    }

    [Theory]
    [FileData("TestData/SimpleGenericClassAndInterface.txt")]
    public void Extract_ShouldHaveOneBaseGenericType_WhenProvidedWitClass(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal(2, classModel.BaseTypes.Count);
        Assert.Equal("object", classModel.BaseTypes[0].Type.Name);

        Assert.Equal("Namespace1.BaseType<T>", classModel.BaseTypes[1].Type.Name);
        Assert.Equal("Namespace1.BaseType", classModel.BaseTypes[1].Type.FullType.Name);
        Assert.Equal(1, classModel.BaseTypes[1].Type.FullType.ContainedTypes.Count);
        Assert.Equal("T", classModel.BaseTypes[1].Type.FullType.ContainedTypes[0].Name);
    }

    [Theory]
    [FileData("TestData/SimpleGenericStructAndInterface.txt")]
    public void Extract_ShouldHaveOneBaseGenericType_WhenProvidedWitStruct(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal(2, classModel.BaseTypes.Count);
        Assert.Equal("System.ValueType", classModel.BaseTypes[0].Type.Name);

        Assert.Equal("Namespace1.BaseType<T>", classModel.BaseTypes[1].Type.Name);
        Assert.Equal("Namespace1.BaseType", classModel.BaseTypes[1].Type.FullType.Name);
        Assert.Equal(1, classModel.BaseTypes[1].Type.FullType.ContainedTypes.Count);
        Assert.Equal("T", classModel.BaseTypes[1].Type.FullType.ContainedTypes[0].Name);
    }

    [Theory]
    [FileData("TestData/SimpleGenericRecordAndInterface.txt")]
    public void Extract_ShouldHaveOneBaseGenericType_WhenProvidedWithRecord(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal(3, classModel.BaseTypes.Count);

        Assert.Equal("object", classModel.BaseTypes[0].Type.Name);

        Assert.Equal("Namespace1.BaseType<T>", classModel.BaseTypes[1].Type.Name);
        Assert.Equal("Namespace1.BaseType", classModel.BaseTypes[1].Type.FullType.Name);
        Assert.Equal(1, classModel.BaseTypes[1].Type.FullType.ContainedTypes.Count);
        Assert.Equal("T", classModel.BaseTypes[1].Type.FullType.ContainedTypes[0].Name);

        Assert.Equal("System.IEquatable<Namespace1.Class1<T>>", classModel.BaseTypes[2].Type.Name);
        Assert.Equal("System.IEquatable", classModel.BaseTypes[2].Type.FullType.Name);
        Assert.Equal(1, classModel.BaseTypes[2].Type.FullType.ContainedTypes.Count);
        Assert.Equal("Namespace1.Class1", classModel.BaseTypes[2].Type.FullType.ContainedTypes[0].Name);
        Assert.Equal("T", classModel.BaseTypes[2].Type.FullType.ContainedTypes[0].ContainedTypes[0].Name);
    }


    [Theory]
    [FileData("TestData/SimpleGenericInterfaceAndInterface.txt")]
    public void Extract_ShouldHaveOneBaseGenericType_WhenProvidedWithInterface(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal(1, classModel.BaseTypes.Count);
        Assert.Equal("Namespace1.BaseType<T>", classModel.BaseTypes[0].Type.Name);
        Assert.Equal("Namespace1.BaseType", classModel.BaseTypes[0].Type.FullType.Name);
        Assert.Equal(1, classModel.BaseTypes[0].Type.FullType.ContainedTypes.Count);
        Assert.Equal("T", classModel.BaseTypes[0].Type.FullType.ContainedTypes[0].Name);
    }

    [Theory]
    [FileData("TestData/ClassWithMultipleGenericParameters.txt")]
    public void Extract_ShouldHaveMultipleBaseGenericTypes_WhenProvidedWithClass(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal("Namespace1.Class1<T,R,K>", classModel.Name);

        Assert.Equal(4, classModel.BaseTypes.Count);

        var baseType1 = classModel.BaseTypes[0].Type;
        Assert.Equal("Base1<T>", baseType1.Name);
        Assert.Equal("Base1", baseType1.FullType.Name);
        Assert.Equal(1, baseType1.FullType.ContainedTypes.Count);
        Assert.Equal("T", baseType1.FullType.ContainedTypes[0].Name);
        Assert.Empty(baseType1.FullType.ContainedTypes[0].ContainedTypes);

        var baseType2 = classModel.BaseTypes[1].Type;
        Assert.Equal("Base2", baseType2.Name);
        Assert.Equal("Base2", baseType2.FullType.Name);
        Assert.Empty(baseType2.FullType.ContainedTypes);

        var baseType3 = classModel.BaseTypes[2].Type;
        Assert.Equal("Base3<R, K>", baseType3.Name);
        Assert.Equal("Base3", baseType3.FullType.Name);
        Assert.Equal(2, baseType3.FullType.ContainedTypes.Count);
        Assert.Equal("R", baseType3.FullType.ContainedTypes[0].Name);
        Assert.Empty(baseType3.FullType.ContainedTypes[0].ContainedTypes);
        Assert.Equal("K", baseType3.FullType.ContainedTypes[1].Name);
        Assert.Empty(baseType3.FullType.ContainedTypes[1].ContainedTypes);

        var baseType4 = classModel.BaseTypes[3].Type;
        Assert.Equal("Base4<C<T, R>, K>", baseType4.Name);
        Assert.Equal("Base4", baseType4.FullType.Name);
        Assert.Equal(2, baseType4.FullType.ContainedTypes.Count);
        Assert.Equal("C", baseType4.FullType.ContainedTypes[0].Name);
        Assert.Equal(2, baseType4.FullType.ContainedTypes[0].ContainedTypes.Count);
        Assert.Equal("T", baseType4.FullType.ContainedTypes[0].ContainedTypes[0].Name);
        Assert.Empty(baseType4.FullType.ContainedTypes[0].ContainedTypes[0].ContainedTypes);
        Assert.Equal("R", baseType4.FullType.ContainedTypes[0].ContainedTypes[1].Name);
        Assert.Empty(baseType4.FullType.ContainedTypes[0].ContainedTypes[1].ContainedTypes);

        Assert.Equal("K", baseType4.FullType.ContainedTypes[1].Name);
        Assert.Empty(baseType4.FullType.ContainedTypes[1].ContainedTypes);
    }

    [Theory]
    [FileData("TestData/GenericBaseTypeWithConcreteType.txt")]
    public void Extract_ShouldHaveMultipleBaseConcreteGenericTypes_WhenProvidedWithClass(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal("Namespace1.Class1", classModel.Name);

        Assert.Equal(2, classModel.BaseTypes.Count);

        var baseType1 = classModel.BaseTypes[0].Type;
        Assert.Equal("Namespace1.GenericClass<string>", baseType1.Name);
        Assert.Equal("Namespace1.GenericClass", baseType1.FullType.Name);
        Assert.Equal(1, baseType1.FullType.ContainedTypes.Count);
        Assert.Equal("string", baseType1.FullType.ContainedTypes[0].Name);
        Assert.Empty(baseType1.FullType.ContainedTypes[0].ContainedTypes);

        var baseType2 = classModel.BaseTypes[1].Type;
        Assert.Equal("Namespace1.IInterface<Namespace1.Class1, ExternClass>", baseType2.Name);
        Assert.Equal("Namespace1.IInterface", baseType2.FullType.Name);
        Assert.Equal(2, baseType2.FullType.ContainedTypes.Count);
        Assert.Equal("Namespace1.Class1", baseType2.FullType.ContainedTypes[0].Name);
        Assert.Empty(baseType2.FullType.ContainedTypes[0].ContainedTypes);
        Assert.Equal("ExternClass", baseType2.FullType.ContainedTypes[1].Name);
        Assert.Empty(baseType2.FullType.ContainedTypes[1].ContainedTypes);
    }

    [Theory]
    [FileData("TestData/GenericInterfaceWithModifiers.txt")]
    public void Extract_ShouldHaveGenericModifiers_WhenProvidedWithGenericInterface(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal("Namespace1.IInterface<out T, in TK>", classModel.Name);
        Assert.Equal(2, classModel.GenericParameters.Count);
        Assert.Equal("T", classModel.GenericParameters[0].Name);
        Assert.Equal("out", classModel.GenericParameters[0].Modifier);
        Assert.Empty(classModel.GenericParameters[0].Constraints);

        Assert.Equal("TK", classModel.GenericParameters[1].Name);
        Assert.Equal("in", classModel.GenericParameters[1].Modifier);
        Assert.Empty(classModel.GenericParameters[1].Constraints);
    }

    [Theory]
    [FileData("TestData/GenericTypeWithPredefinedConstrains.txt")]
    public void Extract_ShouldHaveGenericTypesWithPredefinedConstrains_WhenProvidedWithClass(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel1 = (CSharpClassModel)classTypes[0];
        var classModel2 = (CSharpClassModel)classTypes[1];

        Assert.Equal("Namespace1.Class1<T, TK, TR, TP>", classModel1.Name);
        Assert.Equal(4, classModel1.GenericParameters.Count);
        Assert.Equal("T", classModel1.GenericParameters[0].Name);
        Assert.Equal(1, classModel1.GenericParameters[0].Constraints.Count);
        Assert.Equal("struct", classModel1.GenericParameters[0].Constraints[0].Name);

        Assert.Equal("TK", classModel1.GenericParameters[1].Name);
        Assert.Equal(1, classModel1.GenericParameters[1].Constraints.Count);
        Assert.Equal("class?", classModel1.GenericParameters[1].Constraints[0].Name);
        Assert.Equal("class", classModel1.GenericParameters[1].Constraints[0].FullType.Name);
        Assert.True(classModel1.GenericParameters[1].Constraints[0].FullType.IsNullable);

        Assert.Equal("TR", classModel1.GenericParameters[2].Name);
        Assert.Equal(1, classModel1.GenericParameters[2].Constraints.Count);
        Assert.Equal("notnull", classModel1.GenericParameters[2].Constraints[0].Name);

        Assert.Equal("TP", classModel1.GenericParameters[3].Name);
        Assert.Equal(1, classModel1.GenericParameters[3].Constraints.Count);
        Assert.Equal("Namespace1.IInterface2<T, Namespace1.IInterface2<T, TK>>",
            classModel1.GenericParameters[3].Constraints[0].Name);
        Assert.Equal("Namespace1.IInterface2", classModel1.GenericParameters[3].Constraints[0].FullType.Name);
        Assert.Equal(2, classModel1.GenericParameters[3].Constraints[0].FullType.ContainedTypes.Count);
        Assert.Equal("T", classModel1.GenericParameters[3].Constraints[0].FullType.ContainedTypes[0].Name);
        Assert.Equal("Namespace1.IInterface2",
            classModel1.GenericParameters[3].Constraints[0].FullType.ContainedTypes[1].Name);
        Assert.Equal(2,
            classModel1.GenericParameters[3].Constraints[0].FullType.ContainedTypes[1].ContainedTypes.Count);
        Assert.Equal("T",
            classModel1.GenericParameters[3].Constraints[0].FullType.ContainedTypes[1].ContainedTypes[0].Name);
        Assert.Equal("TK",
            classModel1.GenericParameters[3].Constraints[0].FullType.ContainedTypes[1].ContainedTypes[1].Name);


        Assert.Equal("Namespace1.IInterface<T, TK>", classModel2.Name);

        Assert.Equal("T", classModel2.GenericParameters[0].Name);
        Assert.Equal(1, classModel2.GenericParameters[0].Constraints.Count);
        Assert.Equal("new()", classModel2.GenericParameters[0].Constraints[0].Name);
    }

    [Theory]
    [FileData("TestData/GenericTypeWithMultipleConstrains.txt")]
    public void Extract_ShouldHaveGenericTypesWithMultipleConstrains_WhenProvidedWithClass(string fileContent)
    {
        var syntaxTree = _syntacticModelCreator.Create(fileContent);
        var semanticModel = _semanticModelCreator.Create(syntaxTree);
        var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;

        var classModel = (CSharpClassModel)classTypes[0];

        Assert.Equal("Namespace1.Class1<T, TK, TR>", classModel.Name);
        Assert.Equal(3, classModel.GenericParameters.Count);
        Assert.Equal("T", classModel.GenericParameters[0].Name);
        Assert.Equal(2, classModel.GenericParameters[0].Constraints.Count);
        Assert.Equal("Namespace1.IInterface", classModel.GenericParameters[0].Constraints[0].Name);
        Assert.Equal("Namespace1.IInterface2<TK, TR>", classModel.GenericParameters[0].Constraints[1].Name);
        Assert.Equal("Namespace1.IInterface2", classModel.GenericParameters[0].Constraints[1].FullType.Name);
        Assert.Equal(2, classModel.GenericParameters[0].Constraints[1].FullType.ContainedTypes.Count);
        Assert.Equal("TK", classModel.GenericParameters[0].Constraints[1].FullType.ContainedTypes[0].Name);
        Assert.Equal("TR", classModel.GenericParameters[0].Constraints[1].FullType.ContainedTypes[1].Name);
    }
}

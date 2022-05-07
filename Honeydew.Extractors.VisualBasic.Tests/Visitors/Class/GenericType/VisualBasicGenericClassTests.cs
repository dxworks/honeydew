using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Class.GenericType;

public class VisualBasicGenericClassTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicGenericClassTests()
    {
        var visualBasicGenericParameterSetterVisitor = new VisualBasicGenericParameterSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<IGenericParameterType>>
            {
                new GenericParameterInfoVisitor()
            });
        var baseInfoClassVisitor = new BaseInfoClassVisitor();
        var baseTypesClassVisitor = new BaseTypesClassVisitor();
        var compositeVisitor = new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new VisualBasicClassSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMembersClassType>>
                {
                    baseInfoClassVisitor,
                    baseTypesClassVisitor,
                    visualBasicGenericParameterSetterVisitor
                }),
                new VisualBasicInterfaceSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMembersClassType>>
                {
                    baseInfoClassVisitor,
                    baseTypesClassVisitor,
                    visualBasicGenericParameterSetterVisitor,
                }),
                new VisualBasicStructureSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMembersClassType>>
                {
                    baseInfoClassVisitor,
                    baseTypesClassVisitor,
                    visualBasicGenericParameterSetterVisitor
                })
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

//     [Theory]
//     [InlineData("class")]
//     [InlineData("record")]
//     [InlineData("struct")]
//     [InlineData("interface")]
//     public async Task Extract_ShouldHaveClassNameOfGenericType_WhenProvidedDifferentClassType(string classType)
//     {
//         var filePath = $@"namespace Namespace1
// {{
//     public {classType} Class1<T>  {{ }}
// }}";
//         var syntaxTree = _syntacticModelCreator.Create(filePath);
//         var semanticModel = _semanticModelCreator.Create(syntaxTree);
//         var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;
//
//         var classModel = (VisualBasicClassModel)classTypes[0];
//
//         Assert.Equal("Namespace1.Class1<T>", classModel.Name);
//         Assert.Equal(1, classModel.GenericParameters.Count);
//         Assert.Equal("T", classModel.GenericParameters[0].Name);
//         Assert.Equal("", classModel.GenericParameters[0].Modifier);
//         Assert.Empty(classModel.GenericParameters[0].Constraints);
//     }
//
//
//     [Theory]
//     [InlineData("class")]
//     [InlineData("record")]
//     [InlineData("struct")]
//     [InlineData("interface")]
//     public async Task Extract_ShouldHaveClassNameGenericTypeWithMultipleContainedTypes_WhenProvidedDifferentClassType(
//         string classType)
//     {
//         var filePath = $@"namespace Namespace1
// {{
//     public {classType} Class1<T,R,K> {{ }}
// }}";
//         var syntaxTree = _syntacticModelCreator.Create(filePath);
//         var semanticModel = _semanticModelCreator.Create(syntaxTree);
//         var classTypes = _factExtractor.Extract(syntaxTree, semanticModel).ClassTypes;
//
//         var classModel = (VisualBasicClassModel)classTypes[0];
//
//         Assert.Equal("Namespace1.Class1<T,R,K>", classModel.Name);
//         Assert.Equal(3, classModel.GenericParameters.Count);
//         Assert.Equal("T", classModel.GenericParameters[0].Name);
//         Assert.Equal("R", classModel.GenericParameters[1].Name);
//         Assert.Equal("K", classModel.GenericParameters[2].Name);
//     }

    [Theory]
    [FilePath("TestData/SimpleGenericClassAndInterface.txt")]
    public async Task Extract_ShouldHaveOneBaseGenericType_WhenProvidedWitClass(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = classTypes[0];

        Assert.Equal(1, classModel.BaseTypes.Count);

        Assert.Equal("Namespace1.BaseType(Of T)", classModel.BaseTypes[0].Type.Name);
        Assert.Equal("Namespace1.BaseType", classModel.BaseTypes[0].Type.FullType.Name);
        Assert.Equal(1, classModel.BaseTypes[0].Type.FullType.ContainedTypes.Count);
        Assert.Equal("T", classModel.BaseTypes[0].Type.FullType.ContainedTypes[0].Name);
    }

    [Theory]
    [FilePath("TestData/SimpleGenericStructAndInterface.txt")]
    public async Task Extract_ShouldHaveOneBaseGenericType_WhenProvidedWitStruct(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[1];

        Assert.Equal(1, classModel.BaseTypes.Count);
        
        Assert.Equal("Namespace1.BaseType(Of T)", classModel.BaseTypes[0].Type.Name);
        Assert.Equal("Namespace1.BaseType", classModel.BaseTypes[0].Type.FullType.Name);
        Assert.Equal(1, classModel.BaseTypes[0].Type.FullType.ContainedTypes.Count);
        Assert.Equal("T", classModel.BaseTypes[0].Type.FullType.ContainedTypes[0].Name);
    }

    [Theory]
    [FilePath("TestData/SimpleGenericInterfaceAndInterface.txt")]
    public async Task Extract_ShouldHaveOneBaseGenericType_WhenProvidedWithInterface(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];

        Assert.Equal(1, classModel.BaseTypes.Count);
        Assert.Equal("Namespace1.BaseType(Of T)", classModel.BaseTypes[0].Type.Name);
        Assert.Equal("Namespace1.BaseType", classModel.BaseTypes[0].Type.FullType.Name);
        Assert.Equal(1, classModel.BaseTypes[0].Type.FullType.ContainedTypes.Count);
        Assert.Equal("T", classModel.BaseTypes[0].Type.FullType.ContainedTypes[0].Name);
    }

    [Theory]
    [FilePath("TestData/ClassWithMultipleGenericParameters.txt")]
    public async Task Extract_ShouldHaveMultipleBaseGenericTypes_WhenProvidedWithClass(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];

        Assert.Equal("Namespace1.Class1(Of T, R, K)", classModel.Name);

        Assert.Equal(4, classModel.BaseTypes.Count);

        var baseType1 = classModel.BaseTypes[0].Type;
        Assert.Equal("Base1(Of T)", baseType1.Name);
        Assert.Equal("Base1(Of T)", baseType1.FullType.Name);
        Assert.Equal(1, baseType1.FullType.ContainedTypes.Count);
        Assert.Equal("T", baseType1.FullType.ContainedTypes[0].Name);
        Assert.Empty(baseType1.FullType.ContainedTypes[0].ContainedTypes);

        var baseType2 = classModel.BaseTypes[1].Type;
        Assert.Equal("Base2", baseType2.Name);
        Assert.Equal("Base2", baseType2.FullType.Name);
        Assert.Empty(baseType2.FullType.ContainedTypes);

        var baseType3 = classModel.BaseTypes[2].Type;
        Assert.Equal("Base3(Of R, K)", baseType3.Name);
        Assert.Equal("Base3(Of R, K)", baseType3.FullType.Name);
        Assert.Equal(2, baseType3.FullType.ContainedTypes.Count);
        Assert.Equal("R", baseType3.FullType.ContainedTypes[0].Name);
        Assert.Empty(baseType3.FullType.ContainedTypes[0].ContainedTypes);
        Assert.Equal("K", baseType3.FullType.ContainedTypes[1].Name);
        Assert.Empty(baseType3.FullType.ContainedTypes[1].ContainedTypes);

        var baseType4 = classModel.BaseTypes[3].Type;
        Assert.Equal("Base4(Of C(Of T, R), K)", baseType4.Name);
        Assert.Equal("Base4(Of C(Of T, R), K)", baseType4.FullType.Name);
        Assert.Equal(2, baseType4.FullType.ContainedTypes.Count);
        Assert.Equal("C(Of T, R)", baseType4.FullType.ContainedTypes[0].Name);
        Assert.Equal(2, baseType4.FullType.ContainedTypes[0].ContainedTypes.Count);
        Assert.Equal("T", baseType4.FullType.ContainedTypes[0].ContainedTypes[0].Name);
        Assert.Empty(baseType4.FullType.ContainedTypes[0].ContainedTypes[0].ContainedTypes);
        Assert.Equal("R", baseType4.FullType.ContainedTypes[0].ContainedTypes[1].Name);
        Assert.Empty(baseType4.FullType.ContainedTypes[0].ContainedTypes[1].ContainedTypes);

        Assert.Equal("K", baseType4.FullType.ContainedTypes[1].Name);
        Assert.Empty(baseType4.FullType.ContainedTypes[1].ContainedTypes);
    }

    [Theory]
    [FilePath("TestData/GenericBaseTypeWithConcreteType.txt")]
    public async Task Extract_ShouldHaveMultipleBaseConcreteGenericTypes_WhenProvidedWithClass(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;
        var classModel = (VisualBasicClassModel)classTypes[0];

        Assert.Equal("Namespace1.Class1", classModel.Name);

        Assert.Equal(2, classModel.BaseTypes.Count);

        var baseType1 = classModel.BaseTypes[0].Type;
        Assert.Equal("Namespace1.GenericClass(Of String)", baseType1.Name);
        Assert.Equal("Namespace1.GenericClass", baseType1.FullType.Name);
        Assert.Equal(1, baseType1.FullType.ContainedTypes.Count);
        Assert.Equal("String", baseType1.FullType.ContainedTypes[0].Name);
        Assert.Empty(baseType1.FullType.ContainedTypes[0].ContainedTypes);

        var baseType2 = classModel.BaseTypes[1].Type;
        Assert.Equal("Namespace1.IInterface(Of Namespace1.Class1, ExternClass)", baseType2.Name);
        Assert.Equal("Namespace1.IInterface", baseType2.FullType.Name);
        Assert.Equal(2, baseType2.FullType.ContainedTypes.Count);
        Assert.Equal("Namespace1.Class1", baseType2.FullType.ContainedTypes[0].Name);
        Assert.Empty(baseType2.FullType.ContainedTypes[0].ContainedTypes);
        Assert.Equal("ExternClass", baseType2.FullType.ContainedTypes[1].Name);
        Assert.Empty(baseType2.FullType.ContainedTypes[1].ContainedTypes);
    }

    [Theory]
    [FilePath("TestData/GenericTypeWithPredefinedConstrains.txt")]
    public async Task Extract_ShouldHaveGenericTypesWithPredefinedConstrains_WhenProvidedWithClass(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel1 = (VisualBasicClassModel)classTypes[0];
        var classModel2 = (VisualBasicClassModel)classTypes[1];

        Assert.Equal("Namespace1.Class1(Of T As Structure, TK As Class, TR As notnull, TP As IInterface2(Of T, IInterface2(Of T, TK)))", classModel1.Name);
        Assert.Equal(4, classModel1.GenericParameters.Count);
        Assert.Equal("T", classModel1.GenericParameters[0].Name);
        Assert.Equal(1, classModel1.GenericParameters[0].Constraints.Count);
        Assert.Equal("Structure", classModel1.GenericParameters[0].Constraints[0].Name);

        Assert.Equal("TK", classModel1.GenericParameters[1].Name);
        Assert.Equal(1, classModel1.GenericParameters[1].Constraints.Count);
        Assert.Equal("Class", classModel1.GenericParameters[1].Constraints[0].Name);
        Assert.Equal("Class", classModel1.GenericParameters[1].Constraints[0].FullType.Name);
        Assert.False(classModel1.GenericParameters[1].Constraints[0].FullType.IsNullable);

        Assert.Equal("TR", classModel1.GenericParameters[2].Name);
        Assert.Equal(1, classModel1.GenericParameters[2].Constraints.Count);
        Assert.Equal("notnull", classModel1.GenericParameters[2].Constraints[0].Name);

        Assert.Equal("TP", classModel1.GenericParameters[3].Name);
        Assert.Equal(1, classModel1.GenericParameters[3].Constraints.Count);
        Assert.Equal("Namespace1.IInterface2(Of T As Structure, Namespace1.IInterface2(Of T, TK))",
            classModel1.GenericParameters[3].Constraints[0].Name);
        Assert.Equal("Namespace1.IInterface2", classModel1.GenericParameters[3].Constraints[0].FullType.Name);
        Assert.Equal(2, classModel1.GenericParameters[3].Constraints[0].FullType.ContainedTypes.Count);
        Assert.Equal("T As Structure", classModel1.GenericParameters[3].Constraints[0].FullType.ContainedTypes[0].Name);
        Assert.Equal("Namespace1.IInterface2",
            classModel1.GenericParameters[3].Constraints[0].FullType.ContainedTypes[1].Name);
        Assert.Equal(2,
            classModel1.GenericParameters[3].Constraints[0].FullType.ContainedTypes[1].ContainedTypes.Count);
        Assert.Equal("T",
            classModel1.GenericParameters[3].Constraints[0].FullType.ContainedTypes[1].ContainedTypes[0].Name);
        Assert.Equal("TK",
            classModel1.GenericParameters[3].Constraints[0].FullType.ContainedTypes[1].ContainedTypes[1].Name);


        Assert.Equal("Namespace1.IInterface(Of T As New, TK)", classModel2.Name);

        Assert.Equal("T", classModel2.GenericParameters[0].Name);
        Assert.Equal(1, classModel2.GenericParameters[0].Constraints.Count);
        Assert.Equal("New", classModel2.GenericParameters[0].Constraints[0].Name);
    }

    [Theory]
    [FilePath("TestData/GenericTypeWithMultipleConstrains.txt")]
    public async Task Extract_ShouldHaveGenericTypesWithMultipleConstrains_WhenProvidedWithClass(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];

        Assert.Equal("Namespace1.Class1(Of T As {IInterface, IInterface2(Of TK, TR)}, TK, TR)", classModel.Name);
        Assert.Equal(3, classModel.GenericParameters.Count);
        Assert.Equal("T", classModel.GenericParameters[0].Name);
        Assert.Equal(2, classModel.GenericParameters[0].Constraints.Count);
        Assert.Equal("Namespace1.IInterface", classModel.GenericParameters[0].Constraints[0].Name);
        Assert.Equal("Namespace1.IInterface2(Of TK, TR)", classModel.GenericParameters[0].Constraints[1].Name);
        Assert.Equal("Namespace1.IInterface2", classModel.GenericParameters[0].Constraints[1].FullType.Name);
        Assert.Equal(2, classModel.GenericParameters[0].Constraints[1].FullType.ContainedTypes.Count);
        Assert.Equal("TK", classModel.GenericParameters[0].Constraints[1].FullType.ContainedTypes[0].Name);
        Assert.Equal("TR", classModel.GenericParameters[0].Constraints[1].FullType.ContainedTypes[1].Name);
    }
}

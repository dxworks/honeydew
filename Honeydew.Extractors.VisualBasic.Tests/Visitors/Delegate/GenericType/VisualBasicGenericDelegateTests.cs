using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Delegate.GenericType;

public class VisualBasicGenericDelegateTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();


    public VisualBasicGenericDelegateTests()
    {
        var compositeVisitor = new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new VisualBasicDelegateSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IDelegateType>>
                {
                    new BaseInfoDelegateVisitor(),
                    new VisualBasicGenericParameterSetterVisitor(_loggerMock.Object,
                        new List<ITypeVisitor<IGenericParameterType>>
                        {
                            new GenericParameterInfoVisitor()
                        })
                })
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

    [Theory]
    [FilePath("TestData/DelegateWithOneGenericParam.txt")]
    public async Task Extract_ShouldHaveDelegateNameOfGenericType_WhenProvidedWithGenericDelegate(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicDelegateModel)classTypes[0];

        Assert.Equal("Namespace1.Delegate1(Of T)", classModel.Name);
        Assert.Equal(1, classModel.GenericParameters.Count);
        Assert.Equal("T", classModel.GenericParameters[0].Name);
        Assert.Equal("", classModel.GenericParameters[0].Modifier);
        Assert.Empty(classModel.GenericParameters[0].Constraints);
    }

    [Theory]
    [FilePath("TestData/DelegateWithMultipleGenericParams.txt")]
    public async Task
        Extract_ShouldHaveDelegateNameGenericTypeWithMultipleContainedTypes_WhenProvidedWithGenericDelegate(
            string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicDelegateModel)classTypes[0];

        Assert.Equal("Namespace1.Delegate1(Of T, R, K)", classModel.Name);
        Assert.Equal(3, classModel.GenericParameters.Count);
        Assert.Equal("T", classModel.GenericParameters[0].Name);
        Assert.Equal("", classModel.GenericParameters[0].Modifier);
        Assert.Equal("R", classModel.GenericParameters[1].Name);
        Assert.Equal("", classModel.GenericParameters[1].Modifier);
        Assert.Equal("K", classModel.GenericParameters[2].Name);
        Assert.Equal("", classModel.GenericParameters[2].Modifier);
    }

    [Theory]
    [FilePath("TestData/DelegateWithMultipleGenericParamsWithModifiers.txt")]
    public async Task Extract_ShouldHaveGenericModifiers_WhenProvidedWithGenericDelegate(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicDelegateModel)classTypes[0];

        Assert.Equal("Namespace1.Delegate1(Of Out T, In TR, In TK)", classModel.Name);
        Assert.Equal(3, classModel.GenericParameters.Count);
        Assert.Equal("T", classModel.GenericParameters[0].Name);
        Assert.Equal("Out", classModel.GenericParameters[0].Modifier);
        Assert.Equal("TR", classModel.GenericParameters[1].Name);
        Assert.Equal("In", classModel.GenericParameters[1].Modifier);
        Assert.Equal("TK", classModel.GenericParameters[2].Name);
        Assert.Equal("In", classModel.GenericParameters[2].Modifier);
    }
}

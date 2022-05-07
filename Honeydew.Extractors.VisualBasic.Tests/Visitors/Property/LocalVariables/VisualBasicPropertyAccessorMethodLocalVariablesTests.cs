using Honeydew.Extractors.Dotnet;
using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Property.LocalVariables;

public class VisualBasicPropertyAccessorMethodLocalVariablesTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicPropertyAccessorMethodLocalVariablesTests()
    {
        var compositeVisitor = new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new VisualBasicClassSetterVisitor(_loggerMock.Object,
                    new List<ITypeVisitor<IMembersClassType>>
                    {
                        new BaseInfoClassVisitor(),
                        new VisualBasicPropertySetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IPropertyType>>
                        {
                            new PropertyInfoVisitor(),
                            new VisualBasicAccessorMethodSetterVisitor(_loggerMock.Object,
                                new List<ITypeVisitor<IAccessorMethodType>>
                                {
                                    new MethodInfoVisitor(),
                                    new VisualBasicLocalVariablesSetterVisitor(_loggerMock.Object,
                                        new List<ITypeVisitor<ILocalVariableType>>
                                        {
                                            new LocalVariableInfoVisitor()
                                        })
                                })
                        })
                    })
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

    [Theory]
    [FilePath("TestData/MethodAccessorWithPrimitiveLocalVariables.txt")]
    public async Task Extract_ShouldExtractLocalVariables_WhenProvidedWithPrimitiveTypes(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;
        
        var classModel = (VisualBasicClassModel)classTypes[0];
        Assert.Equal(1, classModel.Properties.Count);

        foreach (var propertyType in classModel.Properties)
        {
            foreach (var accessor in propertyType.Accessors)
            {
                Assert.Equal(4, accessor.LocalVariableTypes.Count);
                Assert.Equal("Integer", accessor.LocalVariableTypes[0].Type.Name);
                Assert.Equal("Integer", accessor.LocalVariableTypes[1].Type.Name);
                Assert.Equal("Integer", accessor.LocalVariableTypes[2].Type.Name);
                Assert.Equal("String", accessor.LocalVariableTypes[3].Type.Name);
            }
        }
    }

    [Theory]
    [FilePath("TestData/MethodAccessorWithCustomClassLocalVariables.txt")]
    public async Task Extract_ShouldExtractLocalVariables_WhenProvidedWithCustomClassTypes(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];
        Assert.Equal(1, classModel.Properties.Count);

        foreach (var property in classModel.Properties)
        {
            foreach (var accessor in property.Accessors)
            {
                Assert.Equal(5, accessor.LocalVariableTypes.Count);
                Assert.Equal("Namespace1.Parent", accessor.LocalVariableTypes[0].Type.Name);
                Assert.Equal("Namespace1.Class2", accessor.LocalVariableTypes[3].Type.Name);
                Assert.Equal("Namespace1.Class3", accessor.LocalVariableTypes[4].Type.Name);
            }
        }


        Assert.Equal("Namespace1.Parent", classModel.Properties[0].Accessors[0].LocalVariableTypes[1].Type.Name);
        Assert.Equal("Namespace1.Parent", classModel.Properties[0].Accessors[0].LocalVariableTypes[2].Type.Name);
    }

    [Theory]
    [FilePath("TestData/MethodAccessorWithExternClassLocalVariables.txt")]
    public async Task Extract_ShouldExtractLocalVariables_WhenProvidedWithExternClassTypes(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;
        
        var classModel = (VisualBasicClassModel)classTypes[0];

        Assert.Equal(1, classModel.Properties.Count);

        foreach (var property in classModel.Properties)
        {
            foreach (var accessor in property.Accessors)
            {
                Assert.Equal(3, accessor.LocalVariableTypes.Count);
                foreach (var localVariableType in accessor.LocalVariableTypes)
                {
                    Assert.Equal("ExternClass", localVariableType.Type.Name);
                }
            }
        }
    }

    [Theory]
    [FilePath("TestData/MethodAccessorWithArrayLocalVariable.txt")]
    public async Task Extract_ShouldExtractLocalVariables_WhenProvidedWithArrayLocalVariable(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());
        var classTypes = compilationUnitType.ClassTypes;

        var classModel = (VisualBasicClassModel)classTypes[0];
        Assert.Equal(1, classModel.Properties.Count);

        foreach (var property in classModel.Properties)
        {
            foreach (var accessor in property.Accessors)
            {
                Assert.Equal(3, accessor.LocalVariableTypes.Count);
                Assert.Equal("Integer()", accessor.LocalVariableTypes[0].Type.Name);
                Assert.Equal("Namespace1.Class2()", accessor.LocalVariableTypes[1].Type.Name);
                Assert.Equal("ExternClass()", accessor.LocalVariableTypes[2].Type.Name);
            }
        }
    }
}

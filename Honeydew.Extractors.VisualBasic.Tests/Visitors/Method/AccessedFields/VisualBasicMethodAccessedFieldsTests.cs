﻿using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.VisualBasic.Visitors.Concrete;
using Honeydew.Extractors.VisualBasic.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Moq;
using Xunit;

namespace Honeydew.Extractors.VisualBasic.Tests.Visitors.Method.AccessedFields;

public class VisualBasicMethodAccessedFieldsTests
{
    private readonly VisualBasicFactExtractor _factExtractor;
    private readonly Mock<ILogger> _loggerMock = new();

    public VisualBasicMethodAccessedFieldsTests()
    {
        var accessedFieldsSetterVisitor = new VisualBasicAccessedFieldsSetterVisitor(_loggerMock.Object,
            new List<ITypeVisitor<AccessedField>>
            {
                new AccessFieldVisitor()
            });
        var compositeVisitor = new VisualBasicCompilationUnitCompositeVisitor(_loggerMock.Object,
            new List<ITypeVisitor<ICompilationUnitType>>
            {
                new VisualBasicClassSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMembersClassType>>
                {
                    new BaseInfoClassVisitor(),
                    new VisualBasicMethodSetterVisitor(_loggerMock.Object, new List<ITypeVisitor<IMethodType>>
                    {
                        new MethodInfoVisitor(),
                        accessedFieldsSetterVisitor
                    })
                }),
            });

        _factExtractor = new VisualBasicFactExtractor(compositeVisitor);
    }

    [Theory]
    [FilePath("TestData/MethodWithAccessedFields.txt")]
    public async Task Extract_ShouldHaveAccessedFields_WhenGivenMethod(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classType = (VisualBasicClassModel)compilationUnitType.ClassTypes[0];
        Assert.Single(compilationUnitType.ClassTypes);
        Assert.Equal("class", classType.ClassType);

        for (var i = 0; i < 2; i++)
        {
            Assert.Equal(4, classType.Methods[i].AccessedFields.Count);

            var accessedField1 = classType.Methods[i].AccessedFields[0];
            Assert.Equal("Prop2", accessedField1.Name);
            Assert.Equal(AccessedField.AccessKind.Getter, accessedField1.Kind);
            Assert.Equal("Namespace1.Module1.User", accessedField1.LocationClassName);
            Assert.Equal("Namespace1.Module1.User", accessedField1.DefinitionClassName);

            var accessedField2 = classType.Methods[i].AccessedFields[1];
            Assert.Equal("_Prop2", accessedField2.Name);
            Assert.Equal(AccessedField.AccessKind.Getter, accessedField2.Kind);
            Assert.Equal("Namespace1.Module1.User", accessedField2.LocationClassName);
            Assert.Equal("Namespace1.Module1.User", accessedField2.DefinitionClassName);

            var accessedField3 = classType.Methods[i].AccessedFields[2];
            Assert.Equal("Prop2", accessedField3.Name);
            Assert.Equal(AccessedField.AccessKind.Setter, accessedField3.Kind);
            Assert.Equal("Namespace1.Module1.User", accessedField3.LocationClassName);
            Assert.Equal("Namespace1.Module1.User", accessedField3.DefinitionClassName);

            var accessedField4 = classType.Methods[i].AccessedFields[3];
            Assert.Equal("_Prop2", accessedField4.Name);
            Assert.Equal(AccessedField.AccessKind.Setter, accessedField4.Kind);
            Assert.Equal("Namespace1.Module1.User", accessedField4.LocationClassName);
            Assert.Equal("Namespace1.Module1.User", accessedField4.DefinitionClassName);
        }
    }

    [Theory]
    [FilePath("TestData/MethodAccessedFieldThatAccessesMember.txt")]
    public async Task Extract_ShouldHaveAccessedField_WhenGivenFiledThatCallsMethod(string filePath)
    {
        var compilationUnitType = await _factExtractor.Extract(filePath, It.IsAny<CancellationToken>());

        var classModel = (VisualBasicClassModel)compilationUnitType.ClassTypes[1];
        var methodType = classModel.Methods[0];

        Assert.Equal(5, methodType.AccessedFields.Count);

        Assert.Equal("c1", methodType.AccessedFields[0].Name);
        Assert.Equal("Namespace1.Class2", methodType.AccessedFields[0].DefinitionClassName);
        Assert.Equal("Namespace1.Class2", methodType.AccessedFields[0].LocationClassName);
        Assert.Equal(AccessedField.AccessKind.Getter, methodType.AccessedFields[0].Kind);

        Assert.Equal("c2", methodType.AccessedFields[1].Name);
        Assert.Equal("Namespace1.Class2", methodType.AccessedFields[1].DefinitionClassName);
        Assert.Equal("Namespace1.Class2", methodType.AccessedFields[1].LocationClassName);
        Assert.Equal(AccessedField.AccessKind.Getter, methodType.AccessedFields[1].Kind);

        Assert.Equal("Field", methodType.AccessedFields[2].Name);
        Assert.Equal("Namespace1.Class1", methodType.AccessedFields[2].DefinitionClassName);
        Assert.Equal("Namespace1.Class1", methodType.AccessedFields[2].LocationClassName);
        Assert.Equal(AccessedField.AccessKind.Setter, methodType.AccessedFields[2].Kind);

        Assert.Equal("c1", methodType.AccessedFields[3].Name);
        Assert.Equal("Namespace1.Class2", methodType.AccessedFields[3].DefinitionClassName);
        Assert.Equal("Namespace1.Class2", methodType.AccessedFields[3].LocationClassName);
        Assert.Equal(AccessedField.AccessKind.Getter, methodType.AccessedFields[3].Kind);

        Assert.Equal("X", methodType.AccessedFields[4].Name);
        Assert.Equal("Namespace1.Class1", methodType.AccessedFields[4].DefinitionClassName);
        Assert.Equal("Namespace1.Class1", methodType.AccessedFields[4].LocationClassName);
        Assert.Equal(AccessedField.AccessKind.Getter, methodType.AccessedFields[4].Kind);
    }
}

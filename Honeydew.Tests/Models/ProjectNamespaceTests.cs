using System.Collections.Generic;
using Honeydew.Models;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Xunit;

namespace Honeydew.Tests.Models;

public class ProjectNamespaceTests
{
    private readonly NamespaceModel _sut;

    public ProjectNamespaceTests()
    {
        _sut = new NamespaceModel();
    }

    [Fact]
    public void Add_ShouldAddNewClass_WhenClassIsFromDefaultNamespace()
    {
        _sut.Add(new ClassModel
        {
            Name = "GlobalClass",
            ContainingNamespaceName = "",
        });

        Assert.Equal("", _sut.Name);
        Assert.Equal(1, _sut.ClassNames.Count);
        Assert.Equal("GlobalClass", _sut.ClassNames[0]);
    }

    [Fact]
    public void Add_ShouldAddNewClass_WhenANewClassModelIsAdded()
    {
        _sut.Add(new ClassModel
        {
            Name = "Models.Class1",
            ContainingNamespaceName = "Models",
        });

        Assert.Equal("Models", _sut.Name);
        Assert.Equal(1, _sut.ClassNames.Count);
        Assert.Equal("Models.Class1", _sut.ClassNames[0]);
    }

    [Fact]
    public void Add_ShouldAddNewClasses_WhenMultipleClassNamesAreAdded()
    {
        _sut.Add(new ClassModel
        {
            Name = "Models.Class1",
            ContainingNamespaceName = "Models",
        });
        _sut.Add(new ClassModel
        {
            Name = "Models.Class2",
            ContainingNamespaceName = "Models",
        });
        _sut.Add(new ClassModel
        {
            Name = "Models.Class3",
            ContainingNamespaceName = "Models",
        });

        Assert.Equal("Models", _sut.Name);
        Assert.Equal(3, _sut.ClassNames.Count);
        Assert.Equal("Models.Class1", _sut.ClassNames[0]);
        Assert.Equal("Models.Class2", _sut.ClassNames[1]);
        Assert.Equal("Models.Class3", _sut.ClassNames[2]);
    }

    [Fact]
    public void Add_ShouldAddNewClassOnce_WhenTheSameClassNamesAdded()
    {
        _sut.Add(new ClassModel
        {
            Name = "Models.Class1",
            ContainingNamespaceName = "Models",
        });
        _sut.Add(new ClassModel
        {
            Name = "Models.Class2",
            ContainingNamespaceName = "Models",
        });
        _sut.Add(new ClassModel
        {
            Name = "Models.Class1",
            ContainingNamespaceName = "Models",
        });

        Assert.Equal("Models", _sut.Name);
        Assert.Equal(2, _sut.ClassNames.Count);
        Assert.Equal("Models.Class1", _sut.ClassNames[0]);
        Assert.Equal("Models.Class2", _sut.ClassNames[1]);
    }

    [Fact]
    public void Add_ShouldNotAddClassOnce_WhenNamespaceIsDifferentInClass()
    {
        _sut.Add(new ClassModel
        {
            Name = "Models.Class1",
            ContainingNamespaceName = "Models",
        });
        _sut.Add(new ClassModel
        {
            Name = "Models.Items.Class2",
            ContainingNamespaceName = "Models.Items",
        });

        Assert.Equal("Models", _sut.Name);
        Assert.Equal(1, _sut.ClassNames.Count);
        Assert.Equal("Models.Class1", _sut.ClassNames[0]);
    }

    [Fact]
    public void Add_ShouldAddClassModelWithAllAttributes_WhenMultipleClassNamesAreAdded()
    {
        var classModel1 = new ClassModel
        {
            Name = "Items.Pencil"
        };
        classModel1.Metrics.Add(new MetricModel
        (
            "MetricName",
            "MetricExtractor",
            typeof(int).FullName ?? nameof(System.Int32),
            0
        ));

        _sut.Add(classModel1);

        var classModel2 = new ClassModel
        {
            Name = "Items.Notebook"
        };
        classModel2.Metrics.Add(new MetricModel
        (
            Value: new List<IBaseType>
            {
                new BaseTypeModel
                {
                    Type = new EntityTypeModel
                    {
                        Name = "Object"
                    },
                    Kind = "class"
                }
            },
            ExtractorName: "BaseTypesExtractor",
            ValueType: typeof(List<IBaseType>).FullName ?? nameof(List<IBaseType>),
            Name: "MetricName2"
        ));

        _sut.Add(classModel2);

        var classModel3 = new ClassModel
        {
            Name = "Items.IItemService"
        };

        classModel3.Metrics.Add(new MetricModel
        (
            Value: 0,
            ExtractorName: "MetricExtractor",
            ValueType: typeof(int).FullName ?? nameof(System.Int32),
            Name: "MetricName"
        ));
        classModel3.Metrics.Add(new MetricModel
        (
            Value: new List<IBaseType>
            {
                new BaseTypeModel
                {
                    Type = new EntityTypeModel
                    {
                        Name = "BaseService"
                    },
                    Kind = "class"
                },
                new BaseTypeModel
                {
                    Type = new EntityTypeModel
                    {
                        Name = "IService"
                    },
                    Kind = "interface"
                }
            },
            ExtractorName: "BaseTypesExtractor",
            ValueType: typeof(List<IBaseType>).FullName ?? nameof(List<IBaseType>),
            Name: "MetricName3"
        ));


        _sut.Add(classModel3);

        Assert.Equal(3, _sut.ClassNames.Count);

        Assert.Equal("Items.Pencil", _sut.ClassNames[0]);
        Assert.Equal("Items.Notebook", _sut.ClassNames[1]);
        Assert.Equal("Items.IItemService", _sut.ClassNames[2]);
    }
}

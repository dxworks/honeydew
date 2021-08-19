using System.Collections.Generic;
using HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnitLevel;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Xunit;

namespace HoneydewCoreIntegrationTests.Models
{
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
            _sut.Add(new ClassModel { Name = "GlobalClass" });

            Assert.Equal("", _sut.Name);
            Assert.Equal(1, _sut.ClassModels.Count);
            Assert.Equal("GlobalClass", _sut.ClassModels[0].Name);
        }

        [Fact]
        public void Add_ShouldAddNewClass_WhenANewClassModelIsAdded()
        {
            _sut.Add(new ClassModel { Name = "Models.Class1" });

            Assert.Equal("Models", _sut.Name);
            Assert.Equal(1, _sut.ClassModels.Count);
            Assert.Equal("Models.Class1", _sut.ClassModels[0].Name);
        }

        [Fact]
        public void Add_ShouldAddNewClasses_WhenMultipleClassModelsAreAdded()
        {
            _sut.Add(new ClassModel { Name = "Models.Class1" });
            _sut.Add(new ClassModel { Name = "Models.Class2" });
            _sut.Add(new ClassModel { Name = "Models.Class3" });

            Assert.Equal("Models", _sut.Name);
            Assert.Equal(3, _sut.ClassModels.Count);
            Assert.Equal("Models.Class1", _sut.ClassModels[0].Name);
            Assert.Equal("Models.Class2", _sut.ClassModels[1].Name);
            Assert.Equal("Models.Class3", _sut.ClassModels[2].Name);
        }

        [Fact]
        public void Add_ShouldAddNewClassOnce_WhenTheSameClassModelsAdded()
        {
            _sut.Add(new ClassModel { Name = "Models.Class1" });
            _sut.Add(new ClassModel { Name = "Models.Class2" });
            _sut.Add(new ClassModel { Name = "Models.Class1" });

            Assert.Equal("Models", _sut.Name);
            Assert.Equal(2, _sut.ClassModels.Count);
            Assert.Equal("Models.Class1", _sut.ClassModels[0].Name);
            Assert.Equal("Models.Class2", _sut.ClassModels[1].Name);
        }

        [Fact]
        public void Add_ShouldNotAddClassOnce_WhenNamespaceIsDifferentInClass()
        {
            _sut.Add(new ClassModel { Name = "Models.Class1" });
            _sut.Add(new ClassModel { Name = "Models.Items.Class2" });

            Assert.Equal("Models", _sut.Name);
            Assert.Equal(1, _sut.ClassModels.Count);
            Assert.Equal("Models.Class1", _sut.ClassModels[0].Name);
        }

        [Fact]
        public void Add_ShouldAddClassModelWithAllAttributes_WhenANewClassModelIsAdded()
        {
            var classModel = new ClassModel
            {
                Name = "Items.Pencil"
            };
            classModel.Metrics.Add(new MetricModel
            {
                Value = 0,
                ExtractorName = typeof(CSharpUsingsCountMetric).FullName,
                ValueType = typeof(int).FullName
            });

            _sut.Add(classModel);

            Assert.Equal(1, _sut.ClassModels.Count);
            var sutClassModel = _sut.ClassModels[0];

            Assert.Equal("Items.Pencil", sutClassModel.Name);

            Assert.Equal(1, sutClassModel.Metrics.Count);

            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnitLevel.CSharpUsingsCountMetric",
                sutClassModel.Metrics[0].ExtractorName);
            Assert.Equal("System.Int32", sutClassModel.Metrics[0].ValueType);
            Assert.Equal(0, (int)sutClassModel.Metrics[0].Value);
        }

        [Fact]
        public void Add_ShouldAddClassModelWithAllAttributes_WhenMultipleClassModelsAreAdded()
        {
            var classModel1 = new ClassModel
            {
                Name = "Items.Pencil"
            };
            classModel1.Metrics.Add(new MetricModel
            {
                Value = 0,
                ExtractorName = typeof(CSharpUsingsCountMetric).FullName,
                ValueType = typeof(int).FullName
            });

            _sut.Add(classModel1);

            var classModel2 = new ClassModel
            {
                Name = "Items.Notebook"
            };
            classModel2.Metrics.Add(new MetricModel
            {
                Value = new List<IBaseType>
                {
                    new BaseTypeModel
                    {
                        Name = "Object",
                        ClassType = "class"
                    }
                },
                ExtractorName = typeof(CSharpBaseClassMetric).FullName,
                ValueType = typeof(List<IBaseType>).FullName
            });

            _sut.Add(classModel2);

            var classModel3 = new ClassModel
            {
                Name = "Items.IItemService"
            };
            classModel3.Metrics.Add(new MetricModel
            {
                Value = 0,
                ExtractorName = typeof(CSharpUsingsCountMetric).FullName,
                ValueType = typeof(int).FullName
            });
            classModel3.Metrics.Add(new MetricModel
            {
                Value = new List<IBaseType>
                {
                    new BaseTypeModel
                    {
                        Name = "BaseService",
                        ClassType = "class"
                    },
                    new BaseTypeModel
                    {
                        Name = "IService",
                        ClassType = "interface"
                    }
                },
                ExtractorName = typeof(CSharpBaseClassMetric).FullName,
                ValueType = typeof(List<IBaseType>).FullName
            });


            _sut.Add(classModel3);

            Assert.Equal(3, _sut.ClassModels.Count);

            Assert.Equal("Items.Pencil", _sut.ClassModels[0].Name);
            Assert.Equal(1, _sut.ClassModels[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnitLevel.CSharpUsingsCountMetric",
                _sut.ClassModels[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Int32", _sut.ClassModels[0].Metrics[0].ValueType);
            Assert.Equal(0, (int)_sut.ClassModels[0].Metrics[0].Value);

            Assert.Equal("Items.Notebook", _sut.ClassModels[1].Name);
            Assert.Equal(1, _sut.ClassModels[1].Metrics.Count);
            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.CSharpBaseClassMetric",
                _sut.ClassModels[1].Metrics[0].ExtractorName);
            Assert.Equal(typeof(List<IBaseType>).FullName,
                _sut.ClassModels[1].Metrics[0].ValueType);
            Assert.Equal(typeof(List<IBaseType>), _sut.ClassModels[1].Metrics[0].Value.GetType());
            var baseTypes = (List<IBaseType>)_sut.ClassModels[1].Metrics[0].Value;
            Assert.Single(baseTypes);
            Assert.Equal("Object", baseTypes[0].Name);
            Assert.Equal("class", baseTypes[0].ClassType);


            Assert.Equal("Items.IItemService", _sut.ClassModels[2].Name);
            Assert.Equal(2, _sut.ClassModels[2].Metrics.Count);
            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnitLevel.CSharpUsingsCountMetric",
                _sut.ClassModels[2].Metrics[0].ExtractorName);
            Assert.Equal("System.Int32", _sut.ClassModels[2].Metrics[0].ValueType);
            Assert.Equal(0, (int)_sut.ClassModels[2].Metrics[0].Value);


            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.CSharpBaseClassMetric",
                _sut.ClassModels[2].Metrics[1].ExtractorName);
            Assert.Equal(typeof(List<IBaseType>).FullName,
                _sut.ClassModels[2].Metrics[1].ValueType);
            Assert.Equal(typeof(List<IBaseType>), _sut.ClassModels[2].Metrics[1].Value.GetType());
            var baseTypes2 = (List<IBaseType>)_sut.ClassModels[2].Metrics[1].Value;
            Assert.Equal(2, baseTypes2.Count);
            Assert.Equal("BaseService", baseTypes2[0].Name);
            Assert.Equal("class", baseTypes2[0].ClassType);

            Assert.Equal("IService", baseTypes2[1].Name);
            Assert.Equal("interface", baseTypes2[1].ClassType);
        }
    }
}

using HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel;
using HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnitLevel;
using HoneydewModels;
using HoneydewModels.CSharp;
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
            _sut.Add(new ClassModel {FullName = "GlobalClass"});

            Assert.Equal("", _sut.Name);
            Assert.Equal(1, _sut.ClassModels.Count);
            Assert.Equal("GlobalClass", _sut.ClassModels[0].FullName);
        }

        [Fact]
        public void Add_ShouldAddNewClass_WhenANewClassModelIsAdded()
        {
            _sut.Add(new ClassModel {FullName = "Models.Class1"});

            Assert.Equal("Models", _sut.Name);
            Assert.Equal(1, _sut.ClassModels.Count);
            Assert.Equal("Models.Class1", _sut.ClassModels[0].FullName);
        }

        [Fact]
        public void Add_ShouldAddNewClasses_WhenMultipleClassModelsAreAdded()
        {
            _sut.Add(new ClassModel {FullName = "Models.Class1"});
            _sut.Add(new ClassModel {FullName = "Models.Class2"});
            _sut.Add(new ClassModel {FullName = "Models.Class3"});

            Assert.Equal("Models", _sut.Name);
            Assert.Equal(3, _sut.ClassModels.Count);
            Assert.Equal("Models.Class1", _sut.ClassModels[0].FullName);
            Assert.Equal("Models.Class2", _sut.ClassModels[1].FullName);
            Assert.Equal("Models.Class3", _sut.ClassModels[2].FullName);
        }

        [Fact]
        public void Add_ShouldAddNewClassOnce_WhenTheSameClassModelsAdded()
        {
            _sut.Add(new ClassModel {FullName = "Models.Class1"});
            _sut.Add(new ClassModel {FullName = "Models.Class2"});
            _sut.Add(new ClassModel {FullName = "Models.Class1"});

            Assert.Equal("Models", _sut.Name);
            Assert.Equal(2, _sut.ClassModels.Count);
            Assert.Equal("Models.Class1", _sut.ClassModels[0].FullName);
            Assert.Equal("Models.Class2", _sut.ClassModels[1].FullName);
        }

        [Fact]
        public void Add_ShouldNotAddClassOnce_WhenNamespaceIsDifferentInClass()
        {
            _sut.Add(new ClassModel {FullName = "Models.Class1"});
            _sut.Add(new ClassModel {FullName = "Models.Items.Class2"});

            Assert.Equal("Models", _sut.Name);
            Assert.Equal(1, _sut.ClassModels.Count);
            Assert.Equal("Models.Class1", _sut.ClassModels[0].FullName);
        }

        [Fact]
        public void Add_ShouldAddClassModelWithAllAttributes_WhenANewClassModelIsAdded()
        {
            var classModel = new ClassModel
            {
                FullName = "Items.Pencil"
            };
            classModel.Metrics.Add(new ClassMetric
            {
                Value = 0,
                ExtractorName = typeof(CSharpUsingsCountMetric).FullName,
                ValueType = typeof(int).FullName
            });

            _sut.Add(classModel);

            Assert.Equal(1, _sut.ClassModels.Count);
            var sutClassModel = _sut.ClassModels[0];

            Assert.Equal("Items.Pencil", sutClassModel.FullName);

            Assert.Equal(1, sutClassModel.Metrics.Count);

            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnitLevel.CSharpUsingsCountMetric",
                sutClassModel.Metrics[0].ExtractorName);
            Assert.Equal("System.Int32", sutClassModel.Metrics[0].ValueType);
            Assert.Equal(0, (int) sutClassModel.Metrics[0].Value);
        }

        [Fact]
        public void Add_ShouldAddClassModelWithAllAttributes_WhenMultipleClassModelsAreAdded()
        {
            var classModel1 = new ClassModel
            {
                FullName = "Items.Pencil"
            };
            classModel1.Metrics.Add(new ClassMetric
            {
                Value = 0,
                ExtractorName = typeof(CSharpUsingsCountMetric).FullName,
                ValueType = typeof(int).FullName
            });

            _sut.Add(classModel1);

            var classModel2 = new ClassModel
            {
                FullName = "Items.Notebook"
            };
            classModel2.Metrics.Add(new ClassMetric
            {
                Value = new CSharpInheritanceMetric
                {
                    BaseClassName = "Object"
                },
                ExtractorName = typeof(CSharpBaseClassMetric).FullName,
                ValueType = typeof(CSharpInheritanceMetric).FullName
            });

            _sut.Add(classModel2);

            var classModel3 = new ClassModel
            {
                FullName = "Items.IItemService"
            };
            classModel3.Metrics.Add(new ClassMetric
            {
                Value = 0,
                ExtractorName = typeof(CSharpUsingsCountMetric).FullName,
                ValueType = typeof(int).FullName
            });
            classModel3.Metrics.Add(new ClassMetric
            {
                Value = new CSharpInheritanceMetric
                {
                    BaseClassName = "BaseService",
                    Interfaces = {"IService"}
                },
                ExtractorName = typeof(CSharpBaseClassMetric).FullName,
                ValueType = typeof(CSharpInheritanceMetric).FullName
            });


            _sut.Add(classModel3);

            Assert.Equal(3, _sut.ClassModels.Count);

            Assert.Equal("Items.Pencil", _sut.ClassModels[0].FullName);
            Assert.Equal(1, _sut.ClassModels[0].Metrics.Count);
            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnitLevel.CSharpUsingsCountMetric",
                _sut.ClassModels[0].Metrics[0].ExtractorName);
            Assert.Equal("System.Int32", _sut.ClassModels[0].Metrics[0].ValueType);
            Assert.Equal(0, (int) _sut.ClassModels[0].Metrics[0].Value);

            Assert.Equal("Items.Notebook", _sut.ClassModels[1].FullName);
            Assert.Equal(1, _sut.ClassModels[1].Metrics.Count);
            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.CSharpBaseClassMetric",
                _sut.ClassModels[1].Metrics[0].ExtractorName);
            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.CSharpInheritanceMetric",
                _sut.ClassModels[1].Metrics[0].ValueType);
            Assert.Equal(typeof(CSharpInheritanceMetric), _sut.ClassModels[1].Metrics[0].Value.GetType());
            var inheritanceMetric1 = (CSharpInheritanceMetric) _sut.ClassModels[1].Metrics[0].Value;
            Assert.Equal(0, inheritanceMetric1.Interfaces.Count);
            Assert.Equal("Object", inheritanceMetric1.BaseClassName);


            Assert.Equal("Items.IItemService", _sut.ClassModels[2].FullName);
            Assert.Equal(2, _sut.ClassModels[2].Metrics.Count);
            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnitLevel.CSharpUsingsCountMetric",
                _sut.ClassModels[2].Metrics[0].ExtractorName);
            Assert.Equal("System.Int32", _sut.ClassModels[2].Metrics[0].ValueType);
            Assert.Equal(0, (int) _sut.ClassModels[2].Metrics[0].Value);


            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.CSharpBaseClassMetric",
                _sut.ClassModels[2].Metrics[1].ExtractorName);
            Assert.Equal("HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel.CSharpInheritanceMetric",
                _sut.ClassModels[2].Metrics[1].ValueType);
            Assert.Equal(typeof(CSharpInheritanceMetric), _sut.ClassModels[2].Metrics[1].Value.GetType());
            var inheritanceMetric2 = (CSharpInheritanceMetric) _sut.ClassModels[2].Metrics[1].Value;
            Assert.Equal("BaseService", inheritanceMetric2.BaseClassName);

            Assert.Equal(1, inheritanceMetric2.Interfaces.Count);
            Assert.Equal("IService", inheritanceMetric2.Interfaces[0]);
        }
    }
}

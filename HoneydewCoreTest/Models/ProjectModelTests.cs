using System.Collections.Generic;
using Honeydew.Models;
using Honeydew.Models.Types;
using Honeydew.Models.CSharp;
using Xunit;

namespace HoneydewCoreTest.Models
{
    public class ProjectModelTests
    {
        private readonly ProjectModel _sut;

        public ProjectModelTests()
        {
            _sut = new ProjectModel();
        }

        [Fact]
        public void Add_ShouldAddNamespace_WhenANewClassModelWithNamespaceIsAdded()
        {
            _sut.Add(new CompilationUnitModel
            {
                ClassTypes = new List<IClassType>
                {
                    new ClassModel
                    {
                        Name = "Models.Class",
                        ContainingNamespaceName = "Models",
                    }
                }
            });

            Assert.Equal(1, _sut.Namespaces.Count);
            Assert.Equal("Models", _sut.Namespaces[0].Name);
        }

        [Fact]
        public void Add_ShouldAddNewClass_WhenClassIsFromDefaultNamespace()
        {
            _sut.Add(new CompilationUnitModel
            {
                ClassTypes = new List<IClassType>
                {
                    new ClassModel
                    {
                        Name = "GlobalClass",
                        ContainingNamespaceName = "",
                    }
                }
            });

            Assert.Equal(1, _sut.Namespaces.Count);
            Assert.Equal("", _sut.Namespaces[0].Name);
        }

        [Fact]
        public void Add_ShouldAddNamespace_WhenMultipleClassModelsWithNamespaceIsAdded()
        {
            _sut.Add(new CompilationUnitModel
            {
                ClassTypes = new List<IClassType>
                {
                    new ClassModel
                    {
                        Name = "Models.M1",
                        ContainingNamespaceName = "Models",
                    }
                }
            });

            _sut.Add(new CompilationUnitModel
            {
                ClassTypes = new List<IClassType>
                {
                    new ClassModel
                    {
                        Name = "Models.Domain.M2",
                        ContainingNamespaceName = "Models.Domain",
                    }
                }
            });

            _sut.Add(new CompilationUnitModel
            {
                ClassTypes = new List<IClassType>
                {
                    new ClassModel
                    {
                        Name = "Items.I1",
                        ContainingNamespaceName = "Items",
                    }
                }
            });

            _sut.Add(new CompilationUnitModel
            {
                ClassTypes = new List<IClassType>
                {
                    new ClassModel
                    {
                        Name = "Services.S1",
                        ContainingNamespaceName = "Services",
                    }
                }
            });

            Assert.Equal(4, _sut.Namespaces.Count);
            Assert.Equal("Models", _sut.Namespaces[0].Name);
            Assert.Equal("Models.Domain", _sut.Namespaces[1].Name);
            Assert.Equal("Items", _sut.Namespaces[2].Name);
            Assert.Equal("Services", _sut.Namespaces[3].Name);
        }

        [Fact]
        public void Add_ShouldAddNamespaceOnce_WhenDifferentClassesAreAddedFromTheSameNamespace()
        {
            _sut.Add(new CompilationUnitModel
            {
                ClassTypes = new List<IClassType>
                {
                    new ClassModel
                    {
                        Name = "Models.M1",
                        ContainingNamespaceName = "Models",
                    }
                }
            });
            _sut.Add(new CompilationUnitModel
            {
                ClassTypes = new List<IClassType>
                {
                    new ClassModel
                    {
                        Name = "Models.M2",
                        ContainingNamespaceName = "Models",
                    }
                }
            });

            Assert.Equal(1, _sut.Namespaces.Count);
            Assert.Equal("Models", _sut.Namespaces[0].Name);
        }

        [Fact]
        public void Add_ShouldAddNamespaceOnce_WhenDifferentClassesAreAddedFromTheSameCompilationUnit()
        {
            _sut.Add(new CompilationUnitModel
            {
                ClassTypes = new List<IClassType>
                {
                    new ClassModel
                    {
                        Name = "Models.M1",
                        ContainingNamespaceName = "Models",
                    },
                    new ClassModel
                    {
                        Name = "Models.M2",
                        ContainingNamespaceName = "Models",
                    }
                }
            });

            Assert.Equal(1, _sut.Namespaces.Count);
            Assert.Equal("Models", _sut.Namespaces[0].Name);
        }
    }
}

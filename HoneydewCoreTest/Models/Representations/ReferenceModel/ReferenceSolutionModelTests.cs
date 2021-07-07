using HoneydewCore.Models.Representations.ReferenceModel;
using Xunit;

namespace HoneydewCoreTest.Models.Representations.ReferenceModel
{
    public class ReferenceSolutionModelTests
    {
        private readonly ReferenceSolutionModel _sut;

        public ReferenceSolutionModelTests()
        {
            _sut = new ReferenceSolutionModel();
        }

        [Theory]
        [InlineData("Project0")]
        [InlineData("Project1")]
        [InlineData("Project0.Namespace0")]
        [InlineData("Project0.Namespace2")]
        [InlineData("Project1.Namespace0")]
        [InlineData("Project0.Namespace0.CountClass")]
        [InlineData("Project1.Namespace0.SomeClass")]
        [InlineData("Calculate")]
        [InlineData("Recalculate")]
        [InlineData("Value")]
        [InlineData("MyMethod")]
        public void FindFirst_ShouldReturnEntityByName_WhenSolutionHasMultipleEntities(
            string entityName)
        {
            AddDataToModel();

            var referenceEntity = _sut.FindFirst(entity => entity.Name == entityName);

            Assert.Equal(entityName, referenceEntity.Name);
        }

        [Theory]
        [InlineData("Project0")]
        [InlineData("Project1")]
        public void FindFirst_ShouldReturnProjectByName_WhenSolutionHasMultipleProject(
            string name)
        {
            AddDataToModel();

            var referenceEntity = _sut.FindFirstProject(projectModel => projectModel.Name == name);

            Assert.Equal(name, referenceEntity.Name);
        }

        [Theory]
        [InlineData("Project0.Namespace0")]
        [InlineData("Project0.Namespace1")]
        [InlineData("Project0.Namespace2")]
        [InlineData("Project1.Namespace0")]
        [InlineData("Project1.Namespace1")]
        public void FindFirst_ShouldReturnNamespaceByName_WhenSolutionHasMultipleNamespaces(
            string name)
        {
            AddDataToModel();

            var referenceEntity = _sut.FindFirstNamespace(namespaceModel => namespaceModel.Name == name);

            Assert.Equal(name, referenceEntity.Name);
        }

        [Theory]
        [InlineData("Project0.Namespace0.CountClass")]
        [InlineData("Project1.Namespace0.SomeClass")]
        [InlineData("Project1.Namespace0.OtherClass")]
        public void FindFirst_ShouldReturnClassByName_WhenSolutionHasMultipleClassesInDifferentProjects(
            string name)
        {
            AddDataToModel();

            var referenceEntity = _sut.FindFirstClass(classModel => classModel.Name == name);

            Assert.Equal(name, referenceEntity.Name);
        }

        [Theory]
        [InlineData("Calculate")]
        [InlineData("Recalculate")]
        [InlineData("MyMethod")]
        public void FindFirst_ShouldReturnMethodByName_WhenSolutionHasMultipleMethodsInDifferentClasses(
            string name)
        {
            AddDataToModel();

            var referenceEntity = _sut.FindFirstMethod(methodModel => methodModel.Name == name);

            Assert.Equal(name, referenceEntity.Name);
        }

        [Theory]
        [InlineData("Count")]
        [InlineData("Value")]
        [InlineData("OtherValue")]
        public void FindFirst_ShouldReturnFieldByName_WhenSolutionHasMultipleFieldsInDifferentClasses(
            string name)
        {
            AddDataToModel();

            var referenceEntity = _sut.FindFirstField(fieldModel => fieldModel.Name == name);

            Assert.Equal(name, referenceEntity.Name);
        }

        private void AddDataToModel()
        {
            _sut.Projects.Add(new ReferenceProjectModel
            {
                Name = "Project0",
                Namespaces =
                {
                    new ReferenceNamespaceModel
                    {
                        Name = "Project0.Namespace0",
                        ClassModels =
                        {
                            new ReferenceClassModel
                            {
                                Name = "Project0.Namespace0.CountClass",
                                Fields =
                                {
                                    new ReferenceFieldModel
                                    {
                                        Name = "Count",
                                        Type = new ReferenceClassModel
                                        {
                                            Name = "int"
                                        }
                                    }
                                },
                                Methods =
                                {
                                    new ReferenceMethodModel
                                    {
                                        Name = "Calculate",
                                        ReturnTypeReferenceClassModel = new ReferenceClassModel
                                        {
                                            Name = "int"
                                        },
                                    },
                                    new ReferenceMethodModel
                                    {
                                        Name = "Recalculate",
                                        ReturnTypeReferenceClassModel = new ReferenceClassModel
                                        {
                                            Name = "int"
                                        },
                                    }
                                }
                            }
                        }
                    },
                    new ReferenceNamespaceModel
                    {
                        Name = "Project0.Namespace1"
                    },
                    new ReferenceNamespaceModel
                    {
                        Name = "Project0.Namespace2"
                    }
                }
            });

            _sut.Projects.Add(new ReferenceProjectModel
            {
                Name = "Project1",
                Namespaces =
                {
                    new ReferenceNamespaceModel
                    {
                        Name = "Project1.Namespace0",
                        ClassModels =
                        {
                            new ReferenceClassModel
                            {
                                Name = "Project1.Namespace0.SomeClass",
                                Fields =
                                {
                                    new ReferenceFieldModel
                                    {
                                        Name = "Value",
                                        Type = new ReferenceClassModel
                                        {
                                            Name = "int"
                                        }
                                    },
                                    new ReferenceFieldModel
                                    {
                                        Name = "OtherValue",
                                        Type = new ReferenceClassModel
                                        {
                                            Name = "float"
                                        }
                                    }
                                },
                                Methods =
                                {
                                    new ReferenceMethodModel
                                    {
                                        Name = "MyMethod",
                                        ReturnTypeReferenceClassModel = new ReferenceClassModel
                                        {
                                            Name = "string"
                                        },
                                        ParameterTypes =
                                        {
                                            new ReferenceParameterModel
                                            {
                                                Type =
                                                    new ReferenceClassModel
                                                    {
                                                        Name = "int"
                                                    }
                                            }
                                        }
                                    }
                                }
                            },
                            new ReferenceClassModel
                            {
                                Name = "Project1.Namespace0.OtherClass"
                            }
                        }
                    },
                    new ReferenceNamespaceModel
                    {
                        Name = "Project1.Namespace1"
                    },
                }
            });
        }
    }
}
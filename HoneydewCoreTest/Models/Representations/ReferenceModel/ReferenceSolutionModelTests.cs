using HoneydewModels.Representations.ReferenceModel;
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

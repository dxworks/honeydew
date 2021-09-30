using System.Collections.Generic;
using HoneydewCore.Processors;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Xunit;

namespace HoneydewCoreIntegrationTests.Processors
{
    public class RepositoryModelToCyclomaticComplexityPerFileProcessorTests
    {
        private readonly RepositoryModelToCyclomaticComplexityPerFileProcessor _sut;

        public RepositoryModelToCyclomaticComplexityPerFileProcessorTests()
        {
            _sut = new RepositoryModelToCyclomaticComplexityPerFileProcessor();
        }

        [Fact]
        public void Process_ShouldReturnEmptyRepresentation_WhenRepositoryModelIsNull()
        {
            var classRelationsRepresentation = _sut.Process(null);
            Assert.Empty(classRelationsRepresentation.File.Concerns);
        }

        [Fact]
        public void Process_ShouldReturnEmptyRepresentation_WhenRepositoryModelIsEmpty()
        {
            var classRelationsRepresentation = _sut.Process(new RepositoryModel());
            Assert.Empty(classRelationsRepresentation.File.Concerns);
        }

        [Fact]
        public void Process_ShouldReturn4ConcernsWith0Strength_WhenRepositoryModelHasOneClassWithNoMethodsOrProperties()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();
            var compilationUnitModel = new CompilationUnitModel();
            var classModel = new ClassModel
            {
                FilePath = "path",
                Fields = new List<IFieldType>
                {
                    new FieldModel
                    {
                        Name = "MyField",
                        Modifier = "static",
                        Type = new EntityTypeModel
                        {
                            Name = "System.Int32"
                        },
                        AccessModifier = "public",
                    }
                }
            };
            compilationUnitModel.ClassTypes.Add(classModel);
            projectModel.CompilationUnits.Add(compilationUnitModel);
            repositoryModel.Solutions.Add(solutionModel);
            repositoryModel.Projects.Add(projectModel);

            var representation = _sut.Process(repositoryModel);
            Assert.Equal(4, representation.File.Concerns.Count);

            foreach (var concern in representation.File.Concerns)
            {
                Assert.Equal("path", concern.Entity);
                Assert.Equal("0", concern.Strength);
            }

            Assert.Equal("maxCyclo", representation.File.Concerns[0].Tag);
            Assert.Equal("minCyclo", representation.File.Concerns[1].Tag);
            Assert.Equal("avgCyclo", representation.File.Concerns[2].Tag);
            Assert.Equal("sumCyclo", representation.File.Concerns[3].Tag);
        }

        [Fact]
        public void Process_ShouldReturn4Concerns_WhenRepositoryModelHasOneClassWithOneMethodInOneFile()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();
            var compilationUnitModel = new CompilationUnitModel();
            var classModel = new ClassModel
            {
                FilePath = "path",
                Methods = new List<IMethodType>
                {
                    new MethodModel
                    {
                        CyclomaticComplexity = 12
                    }
                }
            };
            compilationUnitModel.ClassTypes.Add(classModel);
            projectModel.CompilationUnits.Add(compilationUnitModel);
            repositoryModel.Projects.Add(projectModel);
            repositoryModel.Solutions.Add(solutionModel);

            var representation = _sut.Process(repositoryModel);
            Assert.Equal(4, representation.File.Concerns.Count);

            foreach (var concern in representation.File.Concerns)
            {
                Assert.Equal("path", concern.Entity);
                Assert.Equal("12", concern.Strength);
            }

            Assert.Equal("maxCyclo", representation.File.Concerns[0].Tag);
            Assert.Equal("minCyclo", representation.File.Concerns[1].Tag);
            Assert.Equal("avgCyclo", representation.File.Concerns[2].Tag);
            Assert.Equal("sumCyclo", representation.File.Concerns[3].Tag);
        }

        [Fact]
        public void Process_ShouldReturn4Concerns_WhenRepositoryModelHasMultipleClassesWithOneMethodInOneFile()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();
            var compilationUnitModel = new CompilationUnitModel();
            var classModel1 = new ClassModel
            {
                FilePath = "path",
                Methods = new List<IMethodType>
                {
                    new MethodModel
                    {
                        CyclomaticComplexity = 12
                    }
                }
            };
            var classModel2 = new ClassModel
            {
                FilePath = "path",
                Methods = new List<IMethodType>
                {
                    new MethodModel
                    {
                        CyclomaticComplexity = 6
                    }
                }
            };

            compilationUnitModel.ClassTypes.Add(classModel1);
            compilationUnitModel.ClassTypes.Add(classModel2);
            projectModel.CompilationUnits.Add(compilationUnitModel);
            repositoryModel.Projects.Add(projectModel);
            repositoryModel.Solutions.Add(solutionModel);

            var representation = _sut.Process(repositoryModel);
            Assert.Equal(4, representation.File.Concerns.Count);

            foreach (var concern in representation.File.Concerns)
            {
                Assert.Equal("path", concern.Entity);
            }

            Assert.Equal("maxCyclo", representation.File.Concerns[0].Tag);
            Assert.Equal("12", representation.File.Concerns[0].Strength);

            Assert.Equal("minCyclo", representation.File.Concerns[1].Tag);
            Assert.Equal("6", representation.File.Concerns[1].Strength);

            Assert.Equal("avgCyclo", representation.File.Concerns[2].Tag);
            Assert.Equal("9", representation.File.Concerns[2].Strength);

            Assert.Equal("sumCyclo", representation.File.Concerns[3].Tag);
            Assert.Equal("18", representation.File.Concerns[3].Strength);
        }

        [Fact]
        public void Process_ShouldReturn4Concerns_WhenRepositoryModelHasOneClassWithMultipleMethodInOneFile()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();
            var compilationUnitModel = new CompilationUnitModel();
            var classModel1 = new ClassModel
            {
                FilePath = "path",
                Methods = new List<IMethodType>
                {
                    new MethodModel
                    {
                        CyclomaticComplexity = 12
                    },
                    new MethodModel
                    {
                        CyclomaticComplexity = 7
                    },
                    new MethodModel
                    {
                        CyclomaticComplexity = 1
                    }
                }
            };

            compilationUnitModel.ClassTypes.Add(classModel1);
            projectModel.CompilationUnits.Add(compilationUnitModel);
            repositoryModel.Projects.Add(projectModel);
            repositoryModel.Solutions.Add(solutionModel);

            var representation = _sut.Process(repositoryModel);
            Assert.Equal(4, representation.File.Concerns.Count);

            foreach (var concern in representation.File.Concerns)
            {
                Assert.Equal("path", concern.Entity);
            }

            Assert.Equal("maxCyclo", representation.File.Concerns[0].Tag);
            Assert.Equal("12", representation.File.Concerns[0].Strength);

            Assert.Equal("minCyclo", representation.File.Concerns[1].Tag);
            Assert.Equal("1", representation.File.Concerns[1].Strength);

            Assert.Equal("avgCyclo", representation.File.Concerns[2].Tag);
            Assert.Equal("6", representation.File.Concerns[2].Strength);

            Assert.Equal("sumCyclo", representation.File.Concerns[3].Tag);
            Assert.Equal("20", representation.File.Concerns[3].Strength);
        }

        [Fact]
        public void
            Process_ShouldReturn4Concerns_WhenRepositoryModelHasMultipleClassesWithInMultipleFiles()
        {
            var repositoryModel = new RepositoryModel();
            var solutionModel = new SolutionModel();
            var projectModel = new ProjectModel();
            var compilationUnitModel = new CompilationUnitModel();
            var classModel1 = new ClassModel
            {
                FilePath = "path1",
                Methods = new List<IMethodType>
                {
                    new MethodModel
                    {
                        CyclomaticComplexity = 12
                    },
                    new MethodModel
                    {
                        CyclomaticComplexity = 8
                    }
                }
            };
            var classModel2 = new ClassModel
            {
                FilePath = "path2",
                Properties = new List<IPropertyType>
                {
                    new PropertyModel
                    {
                        CyclomaticComplexity = 1
                    }
                },
                Methods = new List<IMethodType>
                {
                    new MethodModel
                    {
                        CyclomaticComplexity = 6
                    },
                },
                Constructors = new List<IConstructorType>
                {
                    new ConstructorModel
                    {
                        CyclomaticComplexity = 1
                    }
                }
            };

            compilationUnitModel.ClassTypes.Add(classModel1);
            compilationUnitModel.ClassTypes.Add(classModel2);
            projectModel.CompilationUnits.Add(compilationUnitModel);
            repositoryModel.Projects.Add(projectModel);
            repositoryModel.Solutions.Add(solutionModel);

            var representation = _sut.Process(repositoryModel);
            Assert.Equal(8, representation.File.Concerns.Count);


            Assert.Equal("path1", representation.File.Concerns[0].Entity);
            Assert.Equal("maxCyclo", representation.File.Concerns[0].Tag);
            Assert.Equal("12", representation.File.Concerns[0].Strength);

            Assert.Equal("path1", representation.File.Concerns[1].Entity);
            Assert.Equal("minCyclo", representation.File.Concerns[1].Tag);
            Assert.Equal("8", representation.File.Concerns[1].Strength);

            Assert.Equal("path1", representation.File.Concerns[2].Entity);
            Assert.Equal("avgCyclo", representation.File.Concerns[2].Tag);
            Assert.Equal("10", representation.File.Concerns[2].Strength);

            Assert.Equal("path1", representation.File.Concerns[3].Entity);
            Assert.Equal("sumCyclo", representation.File.Concerns[3].Tag);
            Assert.Equal("20", representation.File.Concerns[3].Strength);


            Assert.Equal("path2", representation.File.Concerns[4].Entity);
            Assert.Equal("maxCyclo", representation.File.Concerns[4].Tag);
            Assert.Equal("6", representation.File.Concerns[4].Strength);

            Assert.Equal("path2", representation.File.Concerns[5].Entity);
            Assert.Equal("minCyclo", representation.File.Concerns[5].Tag);
            Assert.Equal("1", representation.File.Concerns[5].Strength);

            Assert.Equal("path2", representation.File.Concerns[6].Entity);
            Assert.Equal("avgCyclo", representation.File.Concerns[6].Tag);
            Assert.Equal("2", representation.File.Concerns[6].Strength);

            Assert.Equal("path2", representation.File.Concerns[7].Entity);
            Assert.Equal("sumCyclo", representation.File.Concerns[7].Tag);
            Assert.Equal("8", representation.File.Concerns[7].Strength);
        }
    }
}

using HoneydewCore.IO;
using HoneydewCore.Processors;
using HoneydewModels.CSharp;
using Moq;
using Xunit;

namespace HoneydewCoreTest.Processors
{
    public class FilePathShortenerProcessorTests
    {
        [Fact]
        public void Process_ShouldHaveSolutionsWithShortenedPath_WhenProvidedWithInputPathToAFolder()
        {
            var repositoryModel = new RepositoryModel();
            repositoryModel.Solutions.Add(new SolutionModel
            {
                FilePath = "C:/SomePath/InputFolder/Solution1.sln"
            });
            repositoryModel.Solutions.Add(new SolutionModel
            {
                FilePath = "C:\\SomePath\\InputFolder\\SomeFolder\\solution2.sln"
            });
            repositoryModel.Solutions.Add(new SolutionModel
            {
                FilePath = "C:\\SomePath\\InputFolder\\solution3.sln"
            });

            repositoryModel.Solutions.Add(new SolutionModel
            {
                FilePath = "C:/SomePath/InputFolder/Folder/Other/Folder2/solution4.sln"
            });

            var folderPathValidatorMock = new Mock<IFolderPathValidator>();
            folderPathValidatorMock.Setup(validator => validator.IsFolder("C:\\SomePath\\InputFolder")).Returns(true);
            var sut = new FilePathShortenerProcessor(folderPathValidatorMock.Object, "C:\\SomePath\\InputFolder");

            var processesRepositoryModel = sut.Process(repositoryModel);

            Assert.Equal("Solution1.sln", processesRepositoryModel.Solutions[0].FilePath);
            Assert.Equal("SomeFolder/solution2.sln", processesRepositoryModel.Solutions[1].FilePath);
            Assert.Equal("solution3.sln", processesRepositoryModel.Solutions[2].FilePath);
            Assert.Equal("Folder/Other/Folder2/solution4.sln",
                processesRepositoryModel.Solutions[3].FilePath);
        }

        [Fact]
        public void Process_ShouldHaveProjectsWithShortenedPath_WhenProvidedWithInputPathToAFolder()
        {
            var repositoryModel = new RepositoryModel();
            repositoryModel.Solutions.Add(new SolutionModel
            {
                Projects =
                {
                    new ProjectModel
                    {
                        FilePath = "C:/SomePath/InputFolder/Project1/Project1.csproj"
                    },
                    new ProjectModel
                    {
                        FilePath = "C:\\SomePath\\InputFolder\\Project2\\Project2.csproj"
                    }
                }
            });
            repositoryModel.Solutions.Add(new SolutionModel
            {
                Projects =
                {
                    new ProjectModel
                    {
                        FilePath = "C:/SomePath/InputFolder/SomeFolder/Proj.csproj"
                    }
                }
            });
            repositoryModel.Solutions.Add(new SolutionModel
            {
                Projects =
                {
                    new ProjectModel
                    {
                        FilePath = "C:\\SomePath\\InputFolder\\Folder\\Folder2\\Folder3\\Project1.csproj"
                    },
                    new ProjectModel
                    {
                        FilePath = "C:\\SomePath\\InputFolder\\Folder\\Folder1\\Folder3\\Project2.csproj"
                    }
                }
            });

            var folderPathValidatorMock = new Mock<IFolderPathValidator>();
            folderPathValidatorMock.Setup(validator => validator.IsFolder("C:\\SomePath\\InputFolder")).Returns(true);
            var sut = new FilePathShortenerProcessor(folderPathValidatorMock.Object, "C:\\SomePath\\InputFolder");

            var processesRepositoryModel = sut.Process(repositoryModel);

            Assert.Equal("Project1/Project1.csproj",
                processesRepositoryModel.Solutions[0].Projects[0].FilePath);
            Assert.Equal("Project2/Project2.csproj",
                processesRepositoryModel.Solutions[0].Projects[1].FilePath);
            Assert.Equal("SomeFolder/Proj.csproj",
                processesRepositoryModel.Solutions[1].Projects[0].FilePath);
            Assert.Equal("Folder/Folder2/Folder3/Project1.csproj",
                processesRepositoryModel.Solutions[2].Projects[0].FilePath);
            Assert.Equal("Folder/Folder1/Folder3/Project2.csproj",
                processesRepositoryModel.Solutions[2].Projects[1].FilePath);
        }

        [Fact]
        public void Process_ShouldHaveClassesWithShortenedPath_WhenProvidedWithInputPathToAFolder()
        {
            var repositoryModel = new RepositoryModel();
            repositoryModel.Solutions.Add(new SolutionModel
            {
                Projects =
                {
                    new ProjectModel
                    {
                        Namespaces =
                        {
                            new NamespaceModel
                            {
                                ClassModels =
                                {
                                    new ClassModel
                                    {
                                        FilePath = "C:/SomePath/InputFolder/Project1/Namespace1/Class1.cs"
                                    },
                                    new ClassModel
                                    {
                                        FilePath = "C:/SomePath/InputFolder/Project1/Namespace1/Class2.cs"
                                    }
                                }
                            }
                        }
                    },
                    new ProjectModel
                    {
                        Namespaces =
                        {
                            new NamespaceModel
                            {
                                ClassModels =
                                {
                                    new ClassModel
                                    {
                                        FilePath = "C:\\SomePath\\InputFolder\\Project2\\Models\\Model1.cs"
                                    },
                                    new ClassModel
                                    {
                                        FilePath = "C:\\SomePath\\InputFolder\\Project2\\Models\\Model2.cs"
                                    }
                                }
                            },
                            new NamespaceModel
                            {
                                ClassModels =
                                {
                                    new ClassModel
                                    {
                                        FilePath = "C:\\SomePath\\InputFolder\\Project2\\Controllers\\Controller.cs"
                                    }
                                }
                            }
                        }
                    }
                }
            });
            repositoryModel.Solutions.Add(new SolutionModel
            {
                Projects =
                {
                    new ProjectModel
                    {
                        Namespaces =
                        {
                            new NamespaceModel
                            {
                                ClassModels =
                                {
                                    new ClassModel
                                    {
                                        FilePath =
                                            "C:/SomePath/InputFolder/SomeFolder/Folder1/Folder2/Folder3/_my_class.cs"
                                    }
                                }
                            }
                        }
                    }
                }
            });

            var folderPathValidatorMock = new Mock<IFolderPathValidator>();
            folderPathValidatorMock.Setup(validator => validator.IsFolder("C:\\SomePath\\InputFolder")).Returns(true);
            var sut = new FilePathShortenerProcessor(folderPathValidatorMock.Object, "C:\\SomePath\\InputFolder");

            var processesRepositoryModel = sut.Process(repositoryModel);

            Assert.Equal("Project1/Namespace1/Class1.cs",
                processesRepositoryModel.Solutions[0].Projects[0].Namespaces[0].ClassModels[0].FilePath);
            Assert.Equal("Project1/Namespace1/Class2.cs",
                processesRepositoryModel.Solutions[0].Projects[0].Namespaces[0].ClassModels[1].FilePath);
            Assert.Equal("Project2/Models/Model1.cs",
                processesRepositoryModel.Solutions[0].Projects[1].Namespaces[0].ClassModels[0].FilePath);
            Assert.Equal("Project2/Models/Model2.cs",
                processesRepositoryModel.Solutions[0].Projects[1].Namespaces[0].ClassModels[1].FilePath);
            Assert.Equal("Project2/Controllers/Controller.cs",
                processesRepositoryModel.Solutions[0].Projects[1].Namespaces[1].ClassModels[0].FilePath);
            Assert.Equal("SomeFolder/Folder1/Folder2/Folder3/_my_class.cs",
                processesRepositoryModel.Solutions[1].Projects[0].Namespaces[0].ClassModels[0].FilePath);
        }

        [Fact]
        public void Process_ShouldHaveTheSamePath_WhenFilePathsDontContainTheInputPathToAFolder()
        {
            var repositoryModel = new RepositoryModel();
            repositoryModel.Solutions.Add(new SolutionModel
            {
                FilePath = "RandomPathToSolution1",
                Projects =
                {
                    new ProjectModel
                    {
                        FilePath = "Path1/Path2/Project.csproj",
                        Namespaces =
                        {
                            new NamespaceModel
                            {
                                ClassModels =
                                {
                                    new ClassModel
                                    {
                                        FilePath = "Class1.cs"
                                    },
                                    new ClassModel
                                    {
                                        FilePath = "Path1/Path2/Namespace1/Class2.cs"
                                    }
                                }
                            }
                        }
                    }
                }
            });
            repositoryModel.Solutions.Add(new SolutionModel
            {
                FilePath = "Folder1\\Folder\\Solution1.sln",
                Projects =
                {
                    new ProjectModel
                    {
                        FilePath = "Folder1\\Folder\\Project2\\Project2.csproj",
                        Namespaces =
                        {
                            new NamespaceModel
                            {
                                ClassModels =
                                {
                                    new ClassModel
                                    {
                                        FilePath = "Models\\Model1.cs"
                                    },
                                    new ClassModel
                                    {
                                        FilePath = "Folder1\\Folder\\Models\\Model2.cs"
                                    }
                                }
                            },
                            new NamespaceModel
                            {
                                ClassModels =
                                {
                                    new ClassModel
                                    {
                                        FilePath = "Controllers\\Controller.cs"
                                    }
                                }
                            }
                        }
                    }
                }
            });

            var folderPathValidatorMock = new Mock<IFolderPathValidator>();
            folderPathValidatorMock.Setup(validator => validator.IsFolder("C:\\SomePath\\InputFolder")).Returns(true);
            var sut = new FilePathShortenerProcessor(folderPathValidatorMock.Object, "C:\\SomePath\\InputFolder");

            var processesRepositoryModel = sut.Process(repositoryModel);

            Assert.Equal("RandomPathToSolution1", processesRepositoryModel.Solutions[0].FilePath);
            Assert.Equal("Path1/Path2/Project.csproj",
                processesRepositoryModel.Solutions[0].Projects[0].FilePath);
            Assert.Equal("Class1.cs",
                processesRepositoryModel.Solutions[0].Projects[0].Namespaces[0].ClassModels[0].FilePath);
            Assert.Equal("Path1/Path2/Namespace1/Class2.cs",
                processesRepositoryModel.Solutions[0].Projects[0].Namespaces[0].ClassModels[1].FilePath);

            Assert.Equal("Folder1/Folder/Solution1.sln",
                processesRepositoryModel.Solutions[1].FilePath);
            Assert.Equal("Folder1/Folder/Project2/Project2.csproj",
                processesRepositoryModel.Solutions[1].Projects[0].FilePath);
            Assert.Equal("Models/Model1.cs",
                processesRepositoryModel.Solutions[1].Projects[0].Namespaces[0].ClassModels[0].FilePath);
            Assert.Equal("Folder1/Folder/Models/Model2.cs",
                processesRepositoryModel.Solutions[1].Projects[0].Namespaces[0].ClassModels[1].FilePath);
            Assert.Equal("Controllers/Controller.cs",
                processesRepositoryModel.Solutions[1].Projects[0].Namespaces[1].ClassModels[0].FilePath);
        }

        [Fact]
        public void Process_ShouldHaveFilePathsShortened_WhenProvidedWithInputPathToASolutionFile()
        {
            var repositoryModel = new RepositoryModel();
            repositoryModel.Solutions.Add(new SolutionModel
            {
                FilePath = "D:/SomePath/Solution.sln",
                Projects =
                {
                    new ProjectModel
                    {
                        FilePath = "D:/SomePath/Project1/Project1.csproj",
                        Namespaces =
                        {
                            new NamespaceModel
                            {
                                ClassModels =
                                {
                                    new ClassModel
                                    {
                                        FilePath = "D:/SomePath/Project1/Models/Model1.cs"
                                    },
                                    new ClassModel
                                    {
                                        FilePath = "D:/SomePath/Project1/Models/Model2.cs"
                                    }
                                }
                            }
                        }
                    },
                    new ProjectModel
                    {
                        FilePath = "D:\\SomePath\\Project2\\Project2.csproj",
                        Namespaces =
                        {
                            new NamespaceModel
                            {
                                ClassModels =
                                {
                                    new ClassModel
                                    {
                                        FilePath = "D:\\SomePath\\Project2\\Controller\\Impl\\Controller.cs"
                                    }
                                }
                            },
                            new NamespaceModel
                            {
                                ClassModels =
                                {
                                    new ClassModel
                                    {
                                        FilePath = "D:\\SomePath\\Project2\\Repository\\IRepository.cs"
                                    }
                                }
                            }
                        }
                    }
                }
            });

            var folderPathValidatorMock = new Mock<IFolderPathValidator>();
            folderPathValidatorMock.Setup(validator => validator.IsFolder("D:/SomePath/Solution.sln")).Returns(false);

            var processesRepositoryModel =
                new FilePathShortenerProcessor(folderPathValidatorMock.Object, "D:/SomePath/Solution.sln").Process(
                    repositoryModel);

            Assert.Equal("Solution.sln", processesRepositoryModel.Solutions[0].FilePath);
            Assert.Equal("Project1/Project1.csproj",
                processesRepositoryModel.Solutions[0].Projects[0].FilePath);
            Assert.Equal("Project1/Models/Model1.cs",
                processesRepositoryModel.Solutions[0].Projects[0].Namespaces[0].ClassModels[0].FilePath);
            Assert.Equal("Project1/Models/Model2.cs",
                processesRepositoryModel.Solutions[0].Projects[0].Namespaces[0].ClassModels[1].FilePath);
            Assert.Equal("Project2/Project2.csproj",
                processesRepositoryModel.Solutions[0].Projects[1].FilePath);
            Assert.Equal("Project2/Controller/Impl/Controller.cs",
                processesRepositoryModel.Solutions[0].Projects[1].Namespaces[0].ClassModels[0].FilePath);
            Assert.Equal("Project2/Repository/IRepository.cs",
                processesRepositoryModel.Solutions[0].Projects[1].Namespaces[1].ClassModels[0].FilePath);
        }

        [Fact]
        public void Process_ShouldHaveFilePathsShortened_WhenProvidedWithInputPathToACSharpProjectFile()
        {
            var repositoryModel = new RepositoryModel();
            repositoryModel.Solutions.Add(new SolutionModel
            {
                Projects =
                {
                    new ProjectModel
                    {
                        FilePath = "D:/SomePath/Project1/Project1.csproj",
                        Namespaces =
                        {
                            new NamespaceModel
                            {
                                ClassModels =
                                {
                                    new ClassModel
                                    {
                                        FilePath = "D:/SomePath/Project1/Models/Model1.cs"
                                    },
                                    new ClassModel
                                    {
                                        FilePath = "D:/SomePath/Project1/Models/Model2.cs"
                                    }
                                }
                            },
                            new NamespaceModel
                            {
                                ClassModels =
                                {
                                    new ClassModel
                                    {
                                        FilePath = "D:\\SomePath\\Project1\\Controller\\Impl\\Controller.cs"
                                    }
                                }
                            },
                            new NamespaceModel
                            {
                                ClassModels =
                                {
                                    new ClassModel
                                    {
                                        FilePath = "D:\\SomePath\\Project1\\Repository\\IRepository.cs"
                                    }
                                }
                            }
                        }
                    }
                }
            });

            var folderPathValidatorMock = new Mock<IFolderPathValidator>();
            folderPathValidatorMock.Setup(validator => validator.IsFolder("D:/SomePath/Project.csproj"))
                .Returns(false);

            var processesRepositoryModel =
                new FilePathShortenerProcessor(folderPathValidatorMock.Object, "D:/SomePath/Project.csproj").Process(
                    repositoryModel);

            Assert.Equal("Project1/Project1.csproj",
                processesRepositoryModel.Solutions[0].Projects[0].FilePath);
            Assert.Equal("Project1/Models/Model1.cs",
                processesRepositoryModel.Solutions[0].Projects[0].Namespaces[0].ClassModels[0].FilePath);
            Assert.Equal("Project1/Models/Model2.cs",
                processesRepositoryModel.Solutions[0].Projects[0].Namespaces[0].ClassModels[1].FilePath);
            Assert.Equal("Project1/Controller/Impl/Controller.cs",
                processesRepositoryModel.Solutions[0].Projects[0].Namespaces[1].ClassModels[0].FilePath);
            Assert.Equal("Project1/Repository/IRepository.cs",
                processesRepositoryModel.Solutions[0].Projects[0].Namespaces[2].ClassModels[0].FilePath);
        }
    }
}

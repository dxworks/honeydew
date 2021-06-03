using System.Collections.Generic;
using System.IO;
using HoneydewCore.IO.Readers;
using HoneydewCore.IO.Readers.Filters;
using Xunit;

namespace HoneydewCoreTest.Analyzers.IO.Readers
{
    public class FileReaderTests
    {
        private readonly IFileReader _sut;

        public FileReaderTests()
        {
            _sut = new FileReader(new List<PathFilter>());
        }

        [Fact]
        public void ReadFile_ShouldThrowFileNotFoundException_WhenGivenAnInvalidPath()
        {
            Assert.Throws<FileNotFoundException>(() => _sut.ReadFile("invalidPath.txt"));
        }

        [Fact]
        public void ReadFilePaths_ShouldReturnEmptyList_WhenGivenAnInvalidPathToADirectory()
        {
            Assert.Empty(_sut.ReadFilePaths("pathToAnInvalidDir"));
        }
    }
}
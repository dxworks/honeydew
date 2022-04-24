using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit.Sdk;

namespace Honeydew.Tests;

public class FileDataAttribute : DataAttribute
{
    private readonly string _filePath;

    public FileDataAttribute(string filePath, bool addRootDirectoryPath = true)
    {
        _filePath = addRootDirectoryPath ? Path.Combine(Directory.GetCurrentDirectory(), filePath) : filePath;
    }

    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        var readAllTextAsync = File.ReadAllTextAsync(_filePath);
        yield return new object[] { readAllTextAsync.Result };
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace HoneydewCoreIntegrationTests;

public class MultiFileDataAttribute : DataAttribute
{
    private readonly string[] _filePaths;

    public MultiFileDataAttribute(params string[] filePaths)
    {
        _filePaths = filePaths.Select(path => Path.Combine(Directory.GetCurrentDirectory(), path)).ToArray();
    }

    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        var allTexts = _filePaths.Select(path => File.ReadAllTextAsync(path));

        var fileContentsTask = Task.WhenAll(allTexts);

        var fileContentsResult = fileContentsTask.Result;

        yield return Array.ConvertAll<string, object>(fileContentsResult, input => input);
    }
}

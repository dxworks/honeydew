namespace Honeydew.Extractors.Load;

public interface IProjectProvider
{
}

public interface IProjectProvider<TProject> : IProjectProvider
{
    Task<TProject> GetProject(string path, CancellationToken cancellationToken);
}

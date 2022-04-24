namespace Honeydew.Utils;

public static class TaskUtils
{
    // https://devblogs.microsoft.com/pfxteam/processing-tasks-as-they-complete/
    public static Task<Task<T>>[] Interleaved<T>(IEnumerable<Task<T>> tasks)
    {
        var inputTasks = tasks.ToList();

        var buckets = new TaskCompletionSource<Task<T>>[inputTasks.Count];
        var results = new Task<Task<T>>[buckets.Length];
        for (int i = 0; i < buckets.Length; i++)
        {
            buckets[i] = new TaskCompletionSource<Task<T>>();
            results[i] = buckets[i].Task;
        }

        var nextTaskIndex = -1;

        void Continuation(Task<T> completed)
        {
            var bucket = buckets[Interlocked.Increment(ref nextTaskIndex)];
            bucket.TrySetResult(completed);
        }

        foreach (var inputTask in inputTasks)
            inputTask.ContinueWith(Continuation, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);

        return results;
    }
}

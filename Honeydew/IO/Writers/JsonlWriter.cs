using System;
using System.IO;
using System.Text.Json;

namespace Honeydew.IO.Writers
{
    /// <summary>
    /// Lightweight JSON Lines (JSONL) writer. Writes one JSON object per line.
    /// Intended for large exports to avoid holding all records in memory.
    /// </summary>
    public sealed class JsonlWriter : IDisposable
    {
        private readonly StreamWriter _writer;
        private readonly JsonSerializerOptions _options;
        private bool _disposed;

        public JsonlWriter(string filePath, JsonSerializerOptions? options = null)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            _writer = new StreamWriter(filePath, append: false);
            _options = options ?? new JsonSerializerOptions
            {
                WriteIndented = false
            };
        }

        public void Write<T>(T record)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(JsonlWriter));
            var json = JsonSerializer.Serialize(record, _options);
            _writer.WriteLine(json);
        }

        public void Flush()
        {
            if (_disposed) return;
            _writer.Flush();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _writer.Flush();
            _writer.Dispose();
        }
    }
}

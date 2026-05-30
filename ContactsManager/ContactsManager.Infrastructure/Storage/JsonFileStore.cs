using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Storage
{
    public class JsonFileStore
    {
        private static readonly SemaphoreSlim FileLock = new(1, 1);

        private readonly FileStorageOptions _options;

        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public JsonFileStore(IOptions<FileStorageOptions> options)
        {
            _options = options.Value;

            if (string.IsNullOrWhiteSpace(_options.RootPath))
            {
                throw new InvalidOperationException("File storage root path is not configured.");
            }

            Directory.CreateDirectory(_options.RootPath);
        }

        public string GetFullPath(string fileName)
        {
            return Path.Combine(_options.RootPath, fileName);
        }

        public async Task<List<T>> ReadListAsync<T>(string fileName, Func<List<T>>? seedFactory = null)
        {
            string path = GetFullPath(fileName);

            await FileLock.WaitAsync();
            try
            {
                if (!File.Exists(path))
                {
                    List<T> seeded = seedFactory?.Invoke() ?? new List<T>();
                    await WriteListInternalAsync(path, seeded);
                    return seeded;
                }

                await using FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (stream.Length == 0)
                {
                    return seedFactory?.Invoke() ?? new List<T>();
                }

                List<T>? items = await JsonSerializer.DeserializeAsync<List<T>>(stream, _serializerOptions);
                return items ?? new List<T>();
            }
            finally
            {
                FileLock.Release();
            }
        }

        public async Task WriteListAsync<T>(string fileName, IEnumerable<T> items)
        {
            string path = GetFullPath(fileName);

            await FileLock.WaitAsync();
            try
            {
                await WriteListInternalAsync(path, items);
            }
            finally
            {
                FileLock.Release();
            }
        }

        private async Task WriteListInternalAsync<T>(string path, IEnumerable<T> items)
        {
            string? directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using FileStream stream = File.Create(path);
            await JsonSerializer.SerializeAsync(stream, items, _serializerOptions);
        }
    }
}
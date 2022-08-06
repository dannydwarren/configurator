using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Configurator.Utilities
{
    public interface IFileSystem
    {
        Task<List<string>> ReadAllLinesAsync(string path);
        Task<string> ReadAllTextAsync(string path);
        Task WriteStreamAsync(string path, Stream stream);
        void Delete(string path);
        List<string> EnumerateFileSystemEntries(string path);
        Task WriteAllTextAsync(string path, string contents);
        bool Exists(string path);
        void CreateDirectory(string path);
    }

    public class FileSystem : IFileSystem
    {
        public async Task<List<string>> ReadAllLinesAsync(string path)
        {
            return (await File.ReadAllLinesAsync(path)).ToList();
        }

        public Task<string> ReadAllTextAsync(string path)
        {
            return File.ReadAllTextAsync(path);
        }

        public async Task WriteStreamAsync(string path, Stream stream)
        {
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            await File.WriteAllBytesAsync(path, memoryStream.ToArray());
        }

        public void Delete(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            if (Directory.Exists(path))
            {
                Directory.Delete(path);
            }
        }

        public List<string> EnumerateFileSystemEntries(string path)
        {
            return Directory.EnumerateFileSystemEntries(path)
                .Select(x => Path.Combine(path, x))
                .ToList();
        }

        public async Task WriteAllTextAsync(string path, string contents)
        {
            await File.WriteAllTextAsync(path, contents);
        }

        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }
    }
}

﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Configurator.Utilities
{
    public interface IFileSystem
    {
        Task<List<string>> ReadAllLinesAsync(string path);
        Task WriteStreamAsync(string path, Stream stream);
    }

    public class FileSystem : IFileSystem
    {
        public async Task<List<string>> ReadAllLinesAsync(string path)
        {
            return (await File.ReadAllLinesAsync(path)).ToList();
        }

        public async Task WriteStreamAsync(string path, Stream stream)
        {
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            await File.WriteAllBytesAsync(path, memoryStream.ToArray());
        }
    }
}

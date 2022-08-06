using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Configurator.Utilities
{
    public interface ISpecialFolders
    {
        List<string> GetDesktopPaths();
        string GetLocalAppDataPath();
    }

    public class SpecialFolders : ISpecialFolders
    {
        public List<string> GetDesktopPaths()
        {
            return new List<string>
                {
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory)
                }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
        }

        public string GetLocalAppDataPath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                nameof(Configurator));
        }
    }
}

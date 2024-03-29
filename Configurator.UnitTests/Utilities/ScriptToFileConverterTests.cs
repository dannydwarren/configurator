﻿using System;
using System.IO;
using System.Threading.Tasks;
using Configurator.Utilities;
using Shouldly;
using Xunit;

namespace Configurator.UnitTests.Utilities
{
    public class ScriptToFileConverterTests : UnitTestBase<ScriptToFileConverter>
    {
        [Fact]
        public async Task When_converting_to_powershell()
        {
            var script = RandomString();
            var localAppDataPath = RandomString();
            var localTempDir = Path.Combine(localAppDataPath, "temp");

            GetMock<ISpecialFolders>().Setup(x => x.GetLocalAppDataPath()).Returns(localAppDataPath);

            string? capturedFilePath = null;
            string? capturedContents = null;
            GetMock<IFileSystem>().Setup(x => x.WriteAllTextAsync(IsAny<string>(), IsAny<string>()))
                .Callback<string, string>((filePath, contents) =>
                {
                    capturedFilePath = filePath;
                    capturedContents = contents;
                });

            var scriptFilePath = await BecauseAsync(() => ClassUnderTest.ToPowerShellAsync(script));

            It("writes the script file", () =>
            {
                GetMock<IFileSystem>().Verify(x => x.CreateDirectory(localTempDir));
                capturedContents.ShouldBe(script);
            });

            It("returns the file path", () =>
            {
                capturedFilePath.ShouldSatisfyAllConditions(x =>
                {
                    x.ShouldNotBeNull();
                    x.ShouldStartWith(localTempDir);
                    x.ShouldContain(DateTimeOffset.Now.ToString("yyyy-MM-dd_HH-mm"));
                    x.ShouldEndWith(".ps1");
                    x.ShouldBe(scriptFilePath);
                });
            });
        }
    }
}

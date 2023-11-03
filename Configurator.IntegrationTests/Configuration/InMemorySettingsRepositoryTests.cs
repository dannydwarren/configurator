﻿using Shouldly;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Configurator.IntegrationTests.Configuration
{
    public class InMemorySettingsRepositoryTests : IntegrationTestBase<InMemorySettingsRepository>
    {
        [Fact]
        public async Task When_loading_before_saving()
        {
            string executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            string testManifestsDirectory = Path.Combine(executingDirectory, "..\\..\\..\\TestManifests");
            var testManifestFileName = "test.manifest.json";

            if (!Directory.Exists(testManifestsDirectory))
            {
                throw new Exception("TestManifest directory has moved...");
            }

            var settings = await BecauseAsync(() => ClassUnderTest.LoadSettingsAsync());

            It("loads sensible defaults for testing", () =>
            {
                settings.ShouldNotBeNull().ShouldSatisfyAllConditions(x =>
                {
                    x.Manifest.Directory.ShouldBe(testManifestsDirectory);
                    x.Manifest.FileName.ShouldBe(testManifestFileName);
                });
            });
        }
    }
}

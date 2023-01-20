using System.Collections.Generic;
using System.Threading.Tasks;
using Configurator.Installers;
using Shouldly;
using Xunit;

namespace Configurator.UnitTests.Installers;

public class ConfigureAppCommandTests : UnitTestBase<ConfigureAppCommand>
{
    [Theory]
    [InlineData(AppType.Winget)]
    [InlineData(AppType.Scoop)]
    public async Task When_executing(AppType appType)
    {
        var appId = RandomString();
        var environments = new List<string> { "env1" };
        var expectedEnvironments = string.Join("|", environments);

        Installable? capturedInstallable = null;
        GetMock<IManifestRepository_V2>().Setup(x => x.SaveInstallableAsync(IsAny<Installable>()))
            .Callback<Installable>(installable => { capturedInstallable = installable; });

        await BecauseAsync(() => ClassUnderTest.ExecuteAsync(appId, appType, environments));

        It("saves installable", () => capturedInstallable.ShouldNotBeNull()
            .ShouldSatisfyAllConditions(x =>
            {
                x.AppId.ShouldBe(appId);
                x.AppType.ShouldBe(appType);
                x.Environments.ShouldBe(expectedEnvironments);
            }));
    }
}

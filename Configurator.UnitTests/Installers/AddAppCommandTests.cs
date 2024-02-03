using System.Collections.Generic;
using System.Threading.Tasks;
using Configurator.Installers;
using Shouldly;
using Xunit;

namespace Configurator.UnitTests.Installers;

public class AddAppCommandTests : UnitTestBase<AddAppCommand>
{
    [Theory]
    [InlineData(AppType.Winget)]
    [InlineData(AppType.Scoop)]
    public async Task When_adding_an_app(AppType appType)
    {
        var appId = RandomString();
        var environments = new List<string> { "env1" };
        var expectedEnvironments = string.Join("|", environments);

        Installable? capturedInstallable = null;
        GetMock<IManifestRepository>().Setup(x => x.SaveInstallableAsync(IsAny<Installable>()))
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

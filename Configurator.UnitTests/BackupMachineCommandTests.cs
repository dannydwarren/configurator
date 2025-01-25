using System.Collections.Generic;
using System.Threading.Tasks;
using Configurator.Apps;
using Configurator.Installers;
using Moq;
using Xunit;

namespace Configurator.UnitTests;

public class BackupMachineCommandTests : UnitTestBase<BackupMachineCommand>
{
    [Fact]
    public async Task When_backing_up_all_apps()
    {
        var manifest = new Manifest
        {
            Apps =
            {
                new ScriptApp { AppId = RandomString() },
                new ScriptApp { AppId = RandomString() },
                new PowerShellAppPackage { AppId = RandomString() }
            }
        };
        
        var manifestRepositoryMock = GetMock<IManifestRepository>();
        manifestRepositoryMock.Setup(x => x.LoadAsync(new List<string>())).ReturnsAsync(manifest);

        await BecauseAsync(() => ClassUnderTest.ExecuteAsync());
        
        It("backs up all apps in manifest", () => GetMock<IAppConfigurator>().Verify(x => x.Backup(IsAny<IApp>()), Times.Exactly(3)));
    } 
}

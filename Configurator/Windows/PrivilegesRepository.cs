using System.Security.Principal;

namespace Configurator.Windows;

public interface IPrivilegesRepository
{
    bool UserHasElevatedPrivileges();
}

public class PrivilegesRepository : IPrivilegesRepository
{
    public bool UserHasElevatedPrivileges()
    {
        return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
    }
}

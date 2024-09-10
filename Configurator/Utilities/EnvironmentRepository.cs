using System;

namespace Configurator.Utilities
{
    public interface IEnvironmentRepository
    {
        void AddToMachinePath(string additionalPath);
    }

    public class EnvironmentRepository : IEnvironmentRepository
    {
        public void AddToMachinePath(string additionalPath)
        {
            var name = "PATH";
            var scope = EnvironmentVariableTarget.Machine;
            var oldValue = Environment.GetEnvironmentVariable(name, scope);
            var newValue = oldValue + $";{additionalPath}";
            Environment.SetEnvironmentVariable(name, newValue, scope);
        }
    }
}

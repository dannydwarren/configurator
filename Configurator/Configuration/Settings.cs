namespace Configurator.Configuration
{
    public interface ISettings
    {
        void Update();
    }

    public class Settings : ISettings
    {
        public void Update()
        {
            throw new System.NotImplementedException();
        }
    }
}

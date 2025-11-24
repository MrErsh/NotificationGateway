namespace NotificationGateway.Infrastructure.Exceptions
{
    public class ConfigurationException : Exception
    {
        public ConfigurationException(string sectionName) : base($"Configuration error: {sectionName} is null") { }
    }
}

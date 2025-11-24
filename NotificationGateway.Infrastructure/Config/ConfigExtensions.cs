using Microsoft.Extensions.Configuration;

namespace NotificationGateway.Infrastructure.Config
{
    public static class ConfigurationExtensions
    {
        public static T GetSection<T>(this IConfiguration config) where T : ConfigSection, new()
        {
            var section = new T();
            config.GetSection(section.Path).Bind(section);
            return section;
        }
    }
}

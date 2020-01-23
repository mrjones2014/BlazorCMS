using Microsoft.Extensions.DependencyInjection;

namespace BlazorCMS.Server.Data
{
    public static class IServiceCollectionExtensions
    {
        public static void UseEnvFile(this IServiceCollection services)
        {
            DotNetEnv.Env.Load();
        }
    }
}

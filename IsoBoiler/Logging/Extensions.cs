using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IsoBoiler.Logging
{
    public static class LoggingExtensions
    {
        public static IServiceCollection AddILog(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException("services");
            }

            services.AddOptions();
            services.TryAdd(ServiceDescriptor.Singleton<ILog, IsoBoilerLogger>());
            return services;
        }

        public static async Task LogExceptionsWith(this Task task, ILog logger)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                logger.Log(ex);
                throw;
            }
        }

        public static async Task<T> LogExceptionsWith<T>(this Task<T> task, ILog logger)
        {
            try
            {
                return await task;
            }
            catch (Exception ex)
            {
                logger.Log(ex);
                throw;
            }
        }
    }
}

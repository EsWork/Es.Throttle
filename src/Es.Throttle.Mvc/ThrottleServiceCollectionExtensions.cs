using System;
using Es.Throttle;
using Es.Throttle.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ThrottleServiceCollectionExtensions
    {
        /// <summary>
        /// 添加默认的节流控制
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="options">节流选项设置</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">
        /// services
        /// or
        /// setupAction
        /// </exception>
        public static IServiceCollection AddThrottle(this IServiceCollection services, ThrottleOptions options)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (options == null) options = new ThrottleOptions();

            services.TryAddSingleton(options);

            services.Configure<ThrottleOptions>(setup =>
            {
                setup.EnableGlobalFilter = options.EnableGlobalFilter;
                setup.Policy = options.Policy;
                setup.UseDistributed = options.UseDistributed;
            });

            if (options.UseDistributed)
            {
                services.TryAddSingleton<IRateLimitStore, RateLimitStoreDistributed>();
            }
            else
            {
                services.TryAddSingleton<IRateLimitStore, RateLimitStoreMemory>();
            }

            services.AddSingleton<IRateLimiter, RateLimiter>();
            services.AddScoped<IThrottleService, ThrottleService>();
            services.AddScoped<ThrottlingFilter>();
            services.Configure<MvcOptions>(opt =>
            {
                opt.Filters.AddService(typeof(ThrottlingFilter));
            });

            return services;
        }

        /// <summary>
        /// 添加节流控制
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">services</exception>
        /// <exception cref="System.ArgumentNullException">services
        /// or
        /// setupAction</exception>
        public static IServiceCollection AddThrottle(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var options = new ThrottleOptions();

            configuration.Bind(options);

            services.TryAddSingleton(options);

            services.Configure<ThrottleOptions>(configuration);


            if (options.UseDistributed)
            {
                services.TryAddSingleton<IRateLimitStore, RateLimitStoreDistributed>();
            }
            else
            {
                services.TryAddSingleton<IRateLimitStore, RateLimitStoreMemory>();
            }

            services.AddSingleton<IRateLimiter, RateLimiter>();
            services.AddScoped<IThrottleService, ThrottleService>();
            services.AddScoped<ThrottlingFilter>();
            services.Configure<MvcOptions>(opt =>
            {
                opt.Filters.AddService(typeof(ThrottlingFilter));
            });

            return services;
        }

        /// <summary>
        /// 添加自定义节流业务服务
        /// </summary>
        /// <typeparam name="T">自定义的节流服务实现</typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddThrottleCustomService<T>(this IServiceCollection services)
            where T : class, IThrottleService
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            services.Replace(ServiceDescriptor.Scoped<IThrottleService, T>());
            return services;
        }
    }
}
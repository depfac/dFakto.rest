using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace dFakto.Rest.AspNetCore.Mvc
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddRest(this IServiceCollection services)
        {
            services.AddTransient(x =>
            {
                var o = x.GetService<IOptions<MvcJsonOptions>>();
                return o != null ? o.Value.SerializerSettings : new JsonSerializerSettings();
            });
            services.AddTransient(x => new ResourceBuilder(x.GetService<JsonSerializerSettings>()));

            return services;
        }
    }
}
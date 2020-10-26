using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace dFakto.Rest.AspNetCore.Mvc
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddRest(this IServiceCollection services, JsonSerializerSettings settings = null)
        {
            services.AddSingleton(x =>
            {
                var e = x.GetService<IOptions<MvcNewtonsoftJsonOptions>>();
                e.Value.SerializerSettings.Converters.Add(new ResourceConverter());
                return e.Value.SerializerSettings;
            });
            services.AddSingleton(x => new ResourceBuilder(x.GetService<JsonSerializerSettings>()));

            return services;
        }
    }
}
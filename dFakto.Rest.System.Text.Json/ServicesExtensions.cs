
using System.Text.Json;
using dFakto.Rest.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace dFakto.Rest.System.Text.Json
{
    public static class ServicesExtensions
    {
        public static IServiceCollection AddSystemTextJsonResources(this IServiceCollection services, JsonSerializerOptions? options = null)
        {
            services.AddSingleton(options ?? new JsonSerializerOptions());
            services.AddSingleton<IResourceFactory, ResourceFactory>();
            services.AddSingleton<IResourceSerializer, ResourceSerializer>();
            return services;
        }
    }
}
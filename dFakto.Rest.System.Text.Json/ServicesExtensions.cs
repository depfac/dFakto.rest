
using System;
using System.Text.Json;
using dFakto.Rest.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace dFakto.Rest.System.Text.Json
{
    public class ResourceSerializerOptions
    {
        public ResourceSerializerOptions()
        {
            JsonSerializerOptions = new JsonSerializerOptions();
        }
        
        public bool ForceUseOfArraysForSingleElements { get; set; }
        public JsonSerializerOptions JsonSerializerOptions { get; set; }
    }
    
    public static class ServicesExtensions
    {
        public static IServiceCollection AddSystemTextJsonResources(this IServiceCollection services, Action<ResourceSerializerOptions>? optionAction = null)
        {
            var options = new ResourceSerializerOptions();
            optionAction?.Invoke(options);

            services.AddSingleton(options);
            services.AddSingleton<IResourceFactory, ResourceFactory>();
            return services;
        }
    }
}
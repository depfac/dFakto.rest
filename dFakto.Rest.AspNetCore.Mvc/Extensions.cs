using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using dFakto.Rest.Abstractions;
using dFakto.Rest.System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace dFakto.Rest.AspNetCore.Mvc;

public static class Extensions
{
    /// <summary>
    /// Add Input and Output Formatter to support application/hal+json Content Type
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static MvcOptions AddHypermediaApplicationLanguageFormatters(this MvcOptions x)
    {
        x.InputFormatters.Add(new ResourceInputFormatter());
        x.OutputFormatters.Add(new ResourceOutputFormatter());
        return x;
    }

    /// <summary>
    /// Add support for Hypermedia Application Language Resource factory <see cref="IResourceFactory"/>
    /// </summary>
    /// <param name="services"></param>
    /// <param name="optionAction"></param>
    /// <returns></returns>
    public static IServiceCollection AddHypermediaApplicationLanguage(
        this IServiceCollection services,
        Action<HypermediaApplicationLanguageExpandMiddlewareOptions>? optionAction = null)
    {
        if (optionAction != null)
        {
            services.Configure(optionAction);
        }

        services.AddHttpContextAccessor();
        
        services.AddHeaderPropagation(x =>
        {
            x.Headers.Add(HttpRequestHeader.Authorization.ToString());
            x.Headers.Add(HttpRequestHeader.Cookie.ToString());
        });
        
        services.AddHttpClient(ResourceAccessor.ResourceAccessorHttpClientName, x =>
                {
                    x.DefaultRequestHeaders.Accept.Clear();
                    x.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HypertextApplicationLanguageMediaType));
                }).AddHeaderPropagation();
        
        services.AddTransient<IResourceAccessor, ResourceAccessor>();
        services.AddTransient<IResourceUriFactory, ResourceUriFactory>();
        services.AddTransient<ResourceUriFactory>();
        services.AddSingleton<IResourceFactory>(x =>
            {
                var cfg = x.GetService<IOptions<JsonOptions>>() ??
                          throw new ApplicationException("Unable to resolve IOptions<JsonOptions>");
                var config = new ResourceSerializerOptions
                {
                    JsonSerializerOptions = cfg.Value.JsonSerializerOptions ?? new JsonSerializerOptions()
                };
                return new ResourceFactory(config);
            }
        );

        return services;
    }

    public static IApplicationBuilder UseHypermediaApplicationLanguageExpandMiddleware(this IApplicationBuilder applicationBuilder)
    {
        applicationBuilder.UseHeaderPropagation();
        return applicationBuilder.UseMiddleware<HypermediaApplicationLanguageExpandMiddleware>();
    }
}
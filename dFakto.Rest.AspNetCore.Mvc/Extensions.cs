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
        Action<HypermediaApplicationLanguageExpandMiddlewareOptions> optionAction = null)
    {
        if (optionAction != null)
        {
            services.Configure(optionAction);
        }

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
        return applicationBuilder.UseMiddleware<HypermediaApplicationLanguageExpandMiddleware>();
    }
        
    public static Uri LinkUri(this IUrlHelper uri, string name, object p = null)
    {
        return new Uri(uri.Link(name, p));
    }

    /// <summary>
    /// Retrieve a Resource at the given Uri.
    /// The current Cookies and Authentication Header will be transferred to the request 
    /// </summary>
    /// <param name="context">HttpContext</param>
    /// <param name="uri">Uri of the resource to retrieve</param>
    /// <param name="requestTimeout"></param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>The Stream with the resource</returns>
    public static async Task<IResource> GetResource(
        this HttpContext context, 
        Uri uri, 
        CancellationToken cancellationToken = new CancellationToken())
    {
        var factory = context.RequestServices.GetService<IResourceFactory>()
                      ?? throw new ApplicationException("Unable to resolve IResourceFactory");
        var options = context.RequestServices.GetService<IOptions<HypermediaApplicationLanguageExpandMiddlewareOptions>>()
                      ?? throw new ApplicationException("Unable to resolve HypermediaApplicationLanguageExpandMiddlewareOptions");
            
        HttpClientHandler handler = new HttpClientHandler();
        handler.CookieContainer = new CookieContainer();
        foreach (var cookie in context.Request.Cookies)
        {
            handler.CookieContainer.Add(new Cookie(cookie.Key, cookie.Value) {Domain = uri.Host});
        }

        using (HttpClient client = new HttpClient(handler))
        {
            client.Timeout = TimeSpan.FromSeconds(options.Value.RequestTimeout);
                
            HttpRequestMessage request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;
            request.RequestUri = uri;
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.HypertextApplicationLanguageMediaType));
            if (context.Request.Headers.TryGetValue("Authorization", out var auth))
            {
                request.Headers.TryAddWithoutValidation("Authorization", auth.First());
            }

            HttpResponseMessage response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            await using (var stream = await response.Content.ReadAsStreamAsync())
            {
                return await factory.CreateSerializer().Deserialize(stream);
            }
        }
    }
}
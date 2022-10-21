using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using dFakto.Rest.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using MediaTypeHeaderValue = Microsoft.Net.Http.Headers.MediaTypeHeaderValue;

namespace dFakto.Rest.AspNetCore.Mvc;

public class HypermediaApplicationLanguageExpandMiddlewareOptions
{
    public HypermediaApplicationLanguageExpandMiddlewareOptions()
    {
        SupportedMediaTypes = new List<string> {Constants.HypertextApplicationLanguageMediaType};

        RequestTimeout = 5;
    }
        
    /// <summary>
    /// The MediaType of the body that will be taken in to
    /// </summary>
    public List<string> SupportedMediaTypes { get; set; }
    public int RequestTimeout { get; set; }
}
    
internal class HypermediaApplicationLanguageExpandMiddleware
{
    private const string AnyMediaType = "*/*";
        
    private readonly RequestDelegate _next;
    private readonly IResourceAccessor _resourceAccessor;
    private readonly HypermediaApplicationLanguageExpandMiddlewareOptions _middlewareOptions;
    private readonly IResourceSerializer _resourceSerializer;
    private readonly ILogger<HypermediaApplicationLanguageExpandMiddleware> _logger;

    public HypermediaApplicationLanguageExpandMiddleware(
        RequestDelegate next,
        IOptions<HypermediaApplicationLanguageExpandMiddlewareOptions> middlewareOptions, 
        IResourceFactory resourceFactory,
        IResourceAccessor resourceAccessor,
        ILogger<HypermediaApplicationLanguageExpandMiddleware> logger)
    {
        _next = next;
        _resourceAccessor = resourceAccessor;
        _middlewareOptions = middlewareOptions.Value;
        _resourceSerializer = resourceFactory.CreateSerializer();
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var requestAccept = context.Request.Headers[HeaderNames.Accept];
        var mediaTypes = GetMediaTypes(requestAccept.ToString());

        bool mustProcess = context.Request.Query.TryGetValue("expand", out var expands) && (
            mediaTypes.Any(x => _middlewareOptions.SupportedMediaTypes.Contains(x)) ||
            mediaTypes.Contains(AnyMediaType));
            
        if (!mustProcess)
        {
            await _next(context);
        }
        else
        {
            _logger.LogDebug("Expand parameter detected");
            var origin = context.Response.Body;

            await using (var tmpBody = new MemoryStream())
            {
                //Replace Body and call next middleware
                context.Response.Body = tmpBody;
                await _next(context);
                context.Response.Body = origin;
                    
                //Reset Temp Stream
                tmpBody.Seek(0, SeekOrigin.Begin);
                    
                if (_middlewareOptions.SupportedMediaTypes.Contains(MediaTypeHeaderValue.Parse(context.Response.ContentType).MediaType.Value))
                {
                    await AutoExpandResource(tmpBody, context, expands);
                }
                else
                {
                    await tmpBody.CopyToAsync(context.Response.Body);
                }
            }
        }
    }

    private static string[] GetMediaTypes(string headerValue) =>
        headerValue?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => MediaTypeWithQualityHeaderValue.Parse(x).MediaType).ToArray();

    private async Task AutoExpandResource(Stream inputStream, HttpContext context, string[] expands)
    {
        _logger.LogDebug("Loading Resource Response");
        var resource = await _resourceSerializer.Deserialize(inputStream) ?? throw new InvalidOperationException("Unable to deserialize Resource");

        bool changed = false;

        foreach (var expand in expands)
        {
            foreach (var expandName in expand.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var tokens = expandName.Split('.', StringSplitOptions.RemoveEmptyEntries);
                var linkName = tokens.Length > 1 ? tokens[1] : tokens[0];
                var embeddedName = tokens.Length > 1 ? tokens[0] : null;

                if (linkName == Constants.Self)
                    continue;

                if (!string.IsNullOrEmpty(embeddedName) && resource.ContainsEmbedded(embeddedName))
                {
                    foreach (var r in resource.GetEmbeddedResource(embeddedName).Values)
                    {
                        if (r.ContainsLink(linkName) && !resource.ContainsEmbedded(linkName))
                        {
                            var l = r.GetLink(linkName);
                            if (l.SingleValued)
                            {
                                r.AddEmbeddedResource(linkName, await GetResourceAsync(l.Value));
                            }
                            else
                            {
                                var resources = new List<IResource>();
                                foreach (var link in l.Values)
                                {
                                    resources.Add(await GetResourceAsync(link));
                                }
                                r.AddEmbeddedResource(linkName, resources);
                            }
                                
                            changed = true;
                        }
                    }
                }
                else if (resource.ContainsLink(linkName) && !resource.ContainsEmbedded(linkName))
                {
                    resource.AddEmbeddedResource(linkName, await GetResourceAsync(resource.GetLink(linkName).Value));
                    changed = true;
                }
            }
        }
            
        if (changed)
        {
            await _resourceSerializer.Serialize(context.Response.Body, resource);
        }
        else
        {
            inputStream.Seek(0, SeekOrigin.Begin);
            await inputStream.CopyToAsync(context.Response.Body);
        }
    }

    private async Task<IResource> GetResourceAsync(Link link)
    {
        if (!string.IsNullOrEmpty(link.Type) && !_middlewareOptions.SupportedMediaTypes.Contains(link.Type))
        {
            return null;
        }

        if (link.Methods.Length > 0 && !link.Methods.Contains(HttpMethod.Get))
        {
            return null;
        }

        return await _resourceAccessor.GetResource(link.Href);
    }
}
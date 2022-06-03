using System;
using System.Text;
using System.Threading.Tasks;
using dFakto.Rest.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace dFakto.Rest.AspNetCore.Mvc;

public class ResourceOutputFormatter : TextOutputFormatter
{
    public ResourceOutputFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(Constants.HypertextApplicationLanguageMediaType));
        SupportedEncodings.Add(Encoding.UTF8);
    }

    protected override bool CanWriteType(Type type)
    {
        return typeof(IResource).IsAssignableFrom(type);
    }

    public override async Task WriteResponseBodyAsync(
        OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        var resourceFactory = context.HttpContext.RequestServices.GetService<IResourceFactory>() ??
                              throw new InvalidOperationException("Unable to resolve IResourceFactory");
            
        var jsonConverter = resourceFactory.CreateSerializer();

        await context.HttpContext.Response.WriteAsync(
            await jsonConverter.Serialize((IResource) context.Object),
            selectedEncoding);
    }
}
public class ResourceInputFormatter : TextInputFormatter
{
    public ResourceInputFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(Constants.HypertextApplicationLanguageMediaType));
        SupportedEncodings.Add(Encoding.UTF8);
    }
        
    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding effectiveEncoding)
    {
        var resourceFactory = context.HttpContext.RequestServices.GetService<IResourceFactory>();
        if (resourceFactory == null)
            return await InputFormatterResult.FailureAsync();
            
        var jsonConverter = resourceFactory.CreateSerializer();
        return await InputFormatterResult.SuccessAsync(await jsonConverter.Deserialize(context.HttpContext.Request.Body));
    }
}
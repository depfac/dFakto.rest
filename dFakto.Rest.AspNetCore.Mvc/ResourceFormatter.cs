using System;
using System.Text;
using System.Threading.Tasks;
using dFakto.Rest.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace dFakto.Rest.AspNetCore.Mvc
{
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
            var httpContext = context.HttpContext;
            var jsonConverter = httpContext.RequestServices.GetService<IResourceSerializer>();

            await httpContext.Response.WriteAsync(
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
            var httpContext = context.HttpContext;
            var jsonConverter = httpContext.RequestServices.GetService<IResourceSerializer>();
            return await InputFormatterResult.SuccessAsync(await jsonConverter.Deserialize(httpContext.Request.Body));
        }
    }
}
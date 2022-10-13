using dFakto.Rest.Abstractions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace dFakto.Rest.SampleApi;

public class AddSchemaExamples : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(IResource))
        {
            schema.Description = "HAL JSon Resource";
            schema.Properties.Clear();
            schema.Properties.Add(new KeyValuePair<string, OpenApiSchema>("_links",new OpenApiSchema()));
            schema.Properties.Add(new KeyValuePair<string, OpenApiSchema>("_embedded",new OpenApiSchema()));
        }
    }
}
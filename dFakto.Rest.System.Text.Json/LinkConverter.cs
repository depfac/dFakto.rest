using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using dFakto.Rest.Abstractions;

namespace dFakto.Rest.System.Text.Json;

internal class LinkConverter : JsonConverter<Link>
{
    private const string HrefPropertyName = "href";
    private const string DeprecationPropertyName = "deprecation";
    private const string LangPropertyName = "lang";
    private const string NamePropertyName = "name";
    private const string ProfilePropertyName = "profile";
    private const string TemplatedPropertyName = "templated";
    private const string TitlePropertyName = "title";
    private const string TypePropertyName = "type";
    private const string MethodsPropertyName = "method";

    public override Link Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        Uri? href = null;
        string? title = null;
        bool templated = false;
        string? deprecation = null;
        string? name = null;
        string? profile = null;
        string? type = null;
        string? hreflang = null;
        List<HttpMethod> methods = new List<HttpMethod>();
            
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                if (href == null)
                    throw new JsonException("Link must have an href");
                 
                return new Link(href,title,type,name,templated,deprecation,hreflang,profile,methods.ToArray());   
            }
            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException();

            string property = reader.GetString() ?? throw new JsonException("Unable to retrieve Property Name");
            reader.Read();
                
            switch (property)
            {
                case HrefPropertyName:
                    if (reader.TokenType != JsonTokenType.Null)
                    {
                        href = new Uri(reader.GetString()!);
                    }
                    break;
                case DeprecationPropertyName:
                    deprecation = reader.GetString();
                    break;
                case LangPropertyName:
                    hreflang = reader.GetString();
                    break;
                case NamePropertyName:
                    name = reader.GetString();
                    break;
                case ProfilePropertyName:
                    profile = reader.GetString();
                    break;
                case TemplatedPropertyName:
                    templated = reader.GetBoolean();
                    break;
                case TitlePropertyName:
                    title = reader.GetString();
                    break;
                case TypePropertyName:
                    type = reader.GetString();
                    break;
                case MethodsPropertyName:
                    if (reader.TokenType != JsonTokenType.StartArray)
                    {
                        throw new JsonException("Method property must be an array");
                    }
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                    {
                        methods.Add(new HttpMethod(reader.GetString()));
                    }
                    break;
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Link link, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WritePropertyName(GetJsonPropertyName(HrefPropertyName, options));
        writer.WriteStringValue(link.Href.ToString());
        if (!string.IsNullOrWhiteSpace(link.Deprecation))
        {
            writer.WriteString(DeprecationPropertyName,link.Deprecation);
        }

        if (!string.IsNullOrWhiteSpace(link.Hreflang))
        {
            writer.WriteString(LangPropertyName,link.Hreflang);
        }

        if (!string.IsNullOrWhiteSpace(link.Name))
        {
            writer.WriteString(NamePropertyName,link.Name);
        }

        if (!string.IsNullOrWhiteSpace(link.Profile))
        {
            writer.WriteString(ProfilePropertyName,link.Profile);
        }

        if (link.Templated.HasValue && link.Templated.Value)
        {
            writer.WriteBoolean(TemplatedPropertyName,true);
        }

        if (!string.IsNullOrWhiteSpace(link.Title))
        {
            writer.WriteString(TitlePropertyName,link.Title);
        }

        if (!string.IsNullOrWhiteSpace(link.Type))
        {
            writer.WriteString(TypePropertyName,link.Type);
        }

        if (link.Methods.Any())
        {
            writer.WritePropertyName(MethodsPropertyName);
            writer.WriteStartArray();
            foreach (var r in link.Methods)
            {
                writer.WriteStringValue(r.Method);
            }
            writer.WriteEndArray();
        }

        writer.WriteEndObject();
    }

    private string GetJsonPropertyName(string propertyName, JsonSerializerOptions options)
    {
        return options.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName;
    }
}
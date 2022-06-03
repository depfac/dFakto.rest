using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using dFakto.Rest.Abstractions;

namespace dFakto.Rest.System.Text.Json;

internal class ResourceConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert == typeof(IResource) || typeToConvert == typeof(Resource);
    }

    public override JsonConverter CreateConverter(
        Type type,
        JsonSerializerOptions options)
    {
        return new ResourceConverter();
    }
}

internal class ResourceConverter : JsonConverter<IResource>
{
    private delegate T DeserializeFunc<out T>(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options);
    private readonly LinkConverter _linkConverter;

    public ResourceConverter()
    {
        _linkConverter = new LinkConverter();
    }
        
    public override IResource Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        Resource resource = new Resource(options);
            
        // Used to compute the resource properties
        var outputBuffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(outputBuffer))
        {
            writer.WriteStartObject();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                string propertyName = ReadPropertyName(ref reader);

                switch (propertyName)
                {
                    case Constants.Links:
                        var links = ReadObjectorArray(ref reader, options, _linkConverter.Read);
                        foreach (var l in links)
                        {
                            resource.AddLink(l.Key, l.Value);
                        }

                        break;
                    case Constants.Embedded:
                        var embedded = ReadObjectorArray(ref reader, options, Read);
                        foreach (var l in embedded)
                        {
                            resource.AddEmbedded(l.Key, l.Value);
                        }

                        break;
                    default:
                        writer.WritePropertyName(propertyName);
                        if (reader.TokenType == JsonTokenType.Null)
                        {
                            writer.WriteNullValue();
                        }
                        else
                        {
                            var v = (JsonElement) (JsonSerializer.Deserialize<object>(ref reader, options)!);
                            v.WriteTo(writer);
                        }

                        break;
                }
            }

            writer.WriteEndObject();
        }

        resource.Add(JsonSerializer.Deserialize<object>(outputBuffer.WrittenSpan, options)!);

        return resource;
    }

    public override void Write(Utf8JsonWriter writer, IResource value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WritePropertyName(Constants.Links);
        writer.WriteStartObject();
        foreach ((string key, SingleOrList<Link> links) in value.Links)
        {
            writer.WritePropertyName(GetJsonPropertyName(key, options));
            if (links.SingleValued)
            {
                _linkConverter.Write(writer,links.Value,options);
            }
            else
            {
                writer.WriteStartArray();
                foreach (var link in links.Values)
                {
                    _linkConverter.Write(writer,link,options);
                }
                writer.WriteEndArray();
            }
        }
        writer.WriteEndObject();

        if (value.Embedded.Any())
        {
            writer.WritePropertyName(Constants.Embedded);
            writer.WriteStartObject();
            foreach ((string key, SingleOrList<IResource> resources) in value.Embedded)
            {
                writer.WritePropertyName(GetJsonPropertyName(key, options));
                if (resources.SingleValued)
                {
                    Write(writer, resources.Value, options);
                }
                else
                {
                    writer.WriteStartArray();
                    foreach (var r in resources.Values)
                    {
                        Write(writer, r, options);
                    }

                    writer.WriteEndArray();
                }
            }

            writer.WriteEndObject();
        }

        var js = (Resource) value;
        using (var doc = JsonDocument.Parse(js.JsonObjectValues))
        {
            foreach (var property in doc.RootElement.EnumerateObject())
            {
                property.WriteTo(writer);
            }
        }

        writer.WriteEndObject();
    }
        
    private static string ReadPropertyName(ref Utf8JsonReader reader)
    {
        if (reader.TokenType != JsonTokenType.PropertyName)
        {
            throw new JsonException();
        }

        var propertyName = reader.GetString() ?? throw new InvalidOperationException("Unable to read property Value");
        reader.Read();
        return propertyName;
    }

    private static Dictionary<string, SingleOrList<T>> ReadObjectorArray<T>(
        ref Utf8JsonReader reader,
        JsonSerializerOptions options,
        DeserializeFunc<T> objReader)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        var result = new Dictionary<string, SingleOrList<T>>();
           
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }
                
            string propertyName = ReadPropertyName(ref reader);

            if (reader.TokenType == JsonTokenType.StartArray)
            {
                var items = new List<T>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        result.Add(propertyName,new SingleOrList<T>(items));
                        break;
                    }
                    items.Add(objReader(ref reader,typeof(T), options));
                }
            }
            else if(reader.TokenType == JsonTokenType.StartObject)
            {
                result.Add(propertyName,new SingleOrList<T>(objReader(ref reader,typeof(T), options)));
            }
            else
            {
                throw new JsonException();
            }
        }

        return result;
    }

    private static string GetJsonPropertyName(string propertyName,JsonSerializerOptions options)
    {
        return options.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName;
    }
}
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using dFakto.Rest.Abstractions;

namespace dFakto.Rest.System.Text.Json;

[JsonConverter(typeof(ResourceConverterFactory))]
internal class Resource : IResource
{

    private static readonly byte[] EmptyJsonObject = Encoding.UTF8.GetBytes("{}");
    private readonly Dictionary<string, ISingleOrList<Link>> _links = new Dictionary<string, ISingleOrList<Link>>();
    private readonly Dictionary<string, ISingleOrList<IResource>> _embedded = new Dictionary<string, ISingleOrList<IResource>>();
    private readonly JsonSerializerOptions _serializerSettings;

    public Resource(JsonSerializerOptions serializerSettings)
    {
        _serializerSettings = serializerSettings;
    }

    public Uri Self => _links[Constants.Self].Value.Href;
    public bool ContainsLink(string name)
    {
        return _links.ContainsKey(name);
    }

    public bool ContainsEmbedded(string name)
    {
        return _embedded.ContainsKey(name);
    }

    public IReadOnlyDictionary<string, ISingleOrList<Link>> GetLinks()
    {
        return _links;
    }

    public IReadOnlyDictionary<string, ISingleOrList<IResource>> GetEmbeddedResources()
    {
        return _embedded;
    }

    public ISingleOrList<Link> GetLink(string name)
    {
        return _links.TryGetValue(name, out var link) ? link : throw new Exception($"No Link named {name} found");
    }

    public ISingleOrList<IResource> GetEmbeddedResource(string name)
    {
        return _embedded.TryGetValue(name, out var value) ? value : throw new Exception($"No Link named {name} found");
    }

    public IResource AddLink(string name, Uri? href)
    {
        return href != null
            ? AddLink(name, new Link(href))
            : this;
    }
    
    public IResource AddLink(string name, Link? link)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(nameof(name));
        
        if (link != null)
        {
            AddLink(name, new SingleOrList<Link>(link));
        }
        return this;
    }
    
    public IResource AddLink(string name, IEnumerable<Link> links)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(nameof(name));
        if (links == null)
            throw new ArgumentNullException(nameof(links));
            
        if (_links.ContainsKey(name) && _links[name].SingleValued)
        {
            _links.Remove(name);
        }

        if (_links.TryGetValue(name, out var link))
        {
            ((SingleOrList<Link>) link).AddRange(links);
        }
        else
        {
            AddLink(name, new SingleOrList<Link>(links.Where(x => x != null)));
        }

        return this;
    }

    public IResource AddEmbeddedResource(string name, IResource? embedded)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(nameof(name));

        return embedded != null
            ? AddEmbedded(name, new SingleOrList<IResource>(embedded))
            : this;

    }
        
    public IResource AddEmbeddedResource(string name, IEnumerable<IResource> embedded)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(nameof(name));
        if (embedded == null)
            throw new ArgumentNullException(nameof(embedded));

        if (_embedded.ContainsKey(name) && _embedded[name].SingleValued)
        {
            _embedded.Remove(name);
        }

        if (_embedded.TryGetValue(name, out var value))
        {
            ((SingleOrList<IResource>) value).AddRange(embedded);
        }
        else
        {
            AddEmbedded(name, new SingleOrList<IResource>(embedded));
        }
            
        return this;
    }

    public IResource Add<T>(T? values, Func<T,object>? onlyFields = null) where T : class
    {
        if (values == null)
            return this;
            
        using (var jsonDocument = JsonDocument.Parse(
                   JsonSerializer.SerializeToUtf8Bytes(onlyFields != null ? onlyFields(values) : values,
                       _serializerSettings)))
        {
            if (jsonDocument.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new ArgumentException("Must be serializable to object");
            }
                
            JsonObjectValues = MergeJsonDocument(jsonDocument);
        }

        return this;
    }

    public T As<T>()
    {
        return JsonSerializer.Deserialize<T>(JsonObjectValues,_serializerSettings) ??
            throw new InvalidOperationException("Resource fields cannot be null");
    }

    public T Bind<T>(T type)
    {
        return As<T>();
    }

    internal IResource AddLink(string name, SingleOrList<Link> link)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(nameof(name));

        _links[name] = link ?? throw new ArgumentNullException(nameof(link));
        return this;
    }
    internal IResource AddEmbedded(string name, SingleOrList<IResource> embedded)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(nameof(name));

        _embedded[name] = embedded ?? throw new ArgumentNullException(nameof(embedded));
        return this;
    }

    internal byte[] JsonObjectValues { get; private set; } = EmptyJsonObject;

    private byte[] MergeJsonDocument(JsonDocument newDocument)
    {
        var outputBuffer = new ArrayBufferWriter<byte>();

        using (var currentDocument = JsonDocument.Parse(JsonObjectValues))
        using (var jsonWriter = new Utf8JsonWriter(outputBuffer))
        {
            var root1 = currentDocument.RootElement;
            var root2 = newDocument.RootElement;

            jsonWriter.WriteStartObject();

            // Write all the properties of the first document that don't conflict with the second
            foreach (var property in root1.EnumerateObject())
            {
                EnsurePropertyNameIsNotReserved(property);

                if (!root2.TryGetProperty(property.Name, out _))
                {
                    property.WriteTo(jsonWriter);
                }
            }

            // Write all the properties of the second document (including those that are duplicates which were skipped earlier)
            // The property values of the second document completely override the values of the first
            foreach (var property in root2.EnumerateObject())
            {
                EnsurePropertyNameIsNotReserved(property);
                    
                property.WriteTo(jsonWriter);
            }

            jsonWriter.WriteEndObject();
        }

        return outputBuffer.WrittenSpan.ToArray();
    }

    private static void EnsurePropertyNameIsNotReserved(JsonProperty property)
    {
        switch (property.Name)
        {
            case Constants.Links:
                throw new ArgumentException("_links is a reserved property name");
            case Constants.Embedded:
                throw new ArgumentException("_embedded is a reserved property name");
        }
    }
}
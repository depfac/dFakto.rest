using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using dFakto.Rest.Abstractions;

namespace dFakto.Rest.System.Text.Json
{
    [JsonConverter(typeof(ResourceConverterFactory))]
    internal class Resource : IResource
    {
        private readonly Dictionary<string, List<Link>> _links = new Dictionary<string, List<Link>>();
        private readonly Dictionary<string, List<IResource>> _embedded = new Dictionary<string, List<IResource>>();

        private readonly JsonSerializerOptions _serializerSettings;

        public Resource(JsonSerializerOptions serializerSettings)
        {
            _serializerSettings = serializerSettings;
        }

        public IReadOnlyDictionary<string, IReadOnlyList<Link>> Links => _links
            .ToDictionary(x => x.Key, x =>(IReadOnlyList<Link>) x.Value);

        public IReadOnlyDictionary<string, IReadOnlyList<IResource>> Embedded => _embedded
            .ToDictionary(x => x.Key, x =>(IReadOnlyList<IResource>) x.Value);

        public IResource AddLink(string name, Uri href)
        {
            return AddLink(name, new Link(href));
        }
        
        public IResource AddLink(string name, params Link[] links)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));
            if (links == null)
                throw new ArgumentNullException(nameof(links));
            
            return AddLink(name,(IEnumerable<Link>) links);
        }
        
        public IResource AddLink(string name, IEnumerable<Link> links)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));
            if (links == null)
                throw new ArgumentNullException(nameof(links));
            
            if (_links.ContainsKey(name))
            {
                _links.Remove(name);
            }
            _links.Add(name, new List<Link>(links.Where(x => x != null)));

            return this;
        }

        public IResource AddEmbedded(string name, params IResource[] embedded)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));
            if (embedded == null)
                throw new ArgumentNullException(nameof(embedded));
            
            return AddEmbedded(name,(IEnumerable<IResource>) embedded);
        }
        
        public IResource AddEmbedded(string name, IEnumerable<IResource> embedded)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));
            if (embedded == null)
                throw new ArgumentNullException(nameof(embedded));
            
            if (_embedded.ContainsKey(name))
            {
                _embedded.Remove(name);
            }
            _embedded.Add(name,new List<IResource>(embedded.Where(x => x != null)));
            return this;
        }

        public IResource Add<T>(T values, Func<T,object>? onlyFields = null) where T : class
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

        public T? As<T>()
        {
            return JsonSerializer.Deserialize<T>(JsonObjectValues);
        }

        public T? Bind<T>(T type)
        {
            return As<T>();
        }

        internal byte[] JsonObjectValues { get; private set; } = Encoding.UTF8.GetBytes("{}");

        private byte[] MergeJsonDocument(JsonDocument newDocument)
        {
            var outputBuffer = new ArrayBufferWriter<byte>();

            using (JsonDocument currentDocument = JsonDocument.Parse(JsonObjectValues))
            using (var jsonWriter = new Utf8JsonWriter(outputBuffer))
            {
                JsonElement root1 = currentDocument.RootElement;
                JsonElement root2 = newDocument.RootElement;

                jsonWriter.WriteStartObject();

                // Write all the properties of the first document that don't conflict with the second
                foreach (JsonProperty property in root1.EnumerateObject())
                {
                    EnsurePropertyNameIsNotReserved(property);

                    if (!root2.TryGetProperty(property.Name, out _))
                    {
                        property.WriteTo(jsonWriter);
                    }
                }

                // Write all the properties of the second document (including those that are duplicates which were skipped earlier)
                // The property values of the second document completely override the values of the first
                foreach (JsonProperty property in root2.EnumerateObject())
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
}
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace dFakto.Rest
{
    public class Resource
    {
        private const string LinksPropertyName = "_links";
        private const string EmbeddedPropertyName = "_embedded";
        private const string SelfPropertyName = "self";
        private const string HrefPropertyName = "href";

        public static readonly JsonSerializer JsonSerializer = new JsonSerializer
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private static readonly JsonMergeSettings MergeSettings = new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Replace,
            PropertyNameComparison = StringComparison.InvariantCultureIgnoreCase,
            MergeNullValueHandling = MergeNullValueHandling.Merge
        };

        internal JObject Value { get; } = new JObject();

        public Resource(string self)
        {
            if (string.IsNullOrWhiteSpace(self))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(self));
            Self = self;
        }

        public string Self
        {
            get => Value[LinksPropertyName]?[SelfPropertyName]?[HrefPropertyName]?.Value<string>();
            set
            {
                if (value != null) AddLink(SelfPropertyName, value);
            }
        }

        public Resource Remove(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            
            if (Value.ContainsKey(name))
            {
                Value.Remove(name);
            }

            return this;
        }
        
        public Resource Add(object value, IEnumerable<string> only = null)
        {
            if (value == null)
                return this;

            var j = JObject.FromObject(value, JsonSerializer);
            if (only != null)
            {
                var o = only.Select(x => x.ToLower()).ToArray();
                if(o.Length == 0)
                    return this;

                var removed = j.Properties().Where(p => !o.Contains(p.Name.ToLower())).ToArray();

                foreach (var p in removed) p.Remove();
            }

            Value.Merge(j, MergeSettings);

            return this;
        }

        public Resource Add(string propertyName, JToken propertyValue)
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            if (propertyValue == null)
                return this;

            Value.Add(propertyName, propertyValue);

            return this;
        }

        public Resource AddLink(string name, Link link)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (link == null) throw new ArgumentNullException(nameof(link));
            
            if (!Value.ContainsKey(LinksPropertyName)) Add(LinksPropertyName, new JObject());

            var l = JObject.FromObject(link, JsonSerializer);
            var links = (JObject) Value[LinksPropertyName];

            if (!links.ContainsKey(name))
            {
                links.Add(name, l);
            }
            else
            {
                if (name == SelfPropertyName)
                    throw new RestException("Resource cannot have two Self link");

                if (links[name] is JObject)
                {
                    var array = new JArray();
                    array.Add(links[name]);

                    links[name] = array;
                }

                ((JArray) links[name]).Add(l);
            }

            return this;
        }

        public Resource AddLink(string name, string href)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (href == null) throw new ArgumentNullException(nameof(href));

            return AddLink(name, new Link(href));
        }

        public Resource AddEmbedded(string name, Resource resource)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return resource == null ? this : AddEmbedded(name, new[] {resource});
        }

        public Resource AddEmbedded(string name, params Resource[] resources)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return resources == null ? this : AddEmbedded(name, (IEnumerable<Resource>) resources);
        }

        public Resource AddEmbedded(string name, IEnumerable<Resource> resource)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            if (!resource.Any())
                return this;

            if (!Value.ContainsKey(EmbeddedPropertyName))
            {
                Value[LinksPropertyName].Parent.AddAfterSelf(new JProperty(EmbeddedPropertyName,new JObject()));
            }

            var embedded = (JObject) Value[EmbeddedPropertyName];

            foreach (var r in resource)
            {
                if (!embedded.ContainsKey(name))
                {
                    embedded.Add(name, r.Value);
                }
                else
                {
                    if (embedded[name] is JObject)
                    {
                        var array = new JArray();
                        array.Add(embedded[name]);

                        embedded[name] = array;
                    }

                    ((JArray) embedded[name]).Add(r.Value);
                }
            }

            return this;
        }

        public override string ToString()
        {
            return Value.ToString(Formatting.Indented);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace dFakto.Rest
{
    [JsonConverter(typeof(ResourceConverter))]
    public class Resource
    {
        private const string SelfPropertyName = "self";
        
        private readonly Dictionary<string,JToken> _fields = new Dictionary<string, JToken>();
        private readonly Dictionary<string,List<Resource>> _embedded = new Dictionary<string,List<Resource>>();
        private readonly Dictionary<string,List<Link>> _links = new Dictionary<string,List<Link>>();
        
        internal Resource()
        {}
        
        public Resource(string self)
        {
            if (string.IsNullOrWhiteSpace(self))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(self));
            Self = self;
        }
        
        internal IDictionary<string,JToken> Fields
        {
            get => _fields;
        }
        internal Dictionary<string,List<Link>> Links
        {
            get => _links;
        }
        internal Dictionary<string,List<Resource>> Embedded
        {
            get => _embedded;
        }

        public string Self
        {
            get => GetFirstLink(SelfPropertyName)?.Href;
            set => AddLink(SelfPropertyName, value);
        }

        public T GetField<T>(string fieldName)
        {
            return _fields[fieldName].Value<T>();
        }

        public bool ContainsLink(string name)
        {
            return _links.ContainsKey(name);
        }

        public bool ContainsEmbedded(string name)
        {
            return _embedded.ContainsKey(name);
        }

        public Link[] GetLinks(string name)
        {
            if (!ContainsLink(name))
                return new Link[0];
            
            return _links[name].ToArray();
        }

        public Resource[] GetEmbedded(string name)
        {
            if (!ContainsEmbedded(name))
                return new Resource[0];

            return _embedded[name].ToArray();
        }

        public Resource Remove(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            
            if (_fields.ContainsKey(name))
            {
                _fields.Remove(name);
            }

            return this;
        }

        public Resource Add(object value, IEnumerable<string> only = null)
        {
            if (value == null)
                return this;

            var properties = (IEnumerable<PropertyInfo>) value.GetType().GetProperties(BindingFlags.Public);
            if (only != null)
            {
                var o = only.Select(x => x.ToLower()).ToArray();
                properties = properties.Where(x => o.Contains(x.Name.ToLower()));
            }

            foreach (var pro in properties)
            {
                AddOrReplaceField(pro.Name, JToken.FromObject(pro.GetValue(value)));
            }

            return this;
        }

        public Resource Add(string propertyName, JToken propertyValue)
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));

            AddOrReplaceField(propertyName, propertyValue);

            return this;
        }

        public Resource AddLink(string name, Link link)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (link == null) throw new ArgumentNullException(nameof(link));

            if (!_links.ContainsKey(name))
            {
                _links.Add(name,new List<Link>());
            }
            _links[name].Add(link);
            return this;
        }

        public Link GetFirstLink(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (_links.ContainsKey(name))
                return _links[name].First();
            return null;
        }

        public Resource AddLink(string name, string href)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (href == null) throw new ArgumentNullException(nameof(href));

            return AddLink(name, new Link(href));
        }

        public Resource AddEmbedded(string name, Resource resource)
        {
            return AddEmbedded(name, new[] {resource});
        }

        public Resource AddEmbedded(string name, params Resource[] resources)
        {
            return AddEmbedded(name, (IEnumerable<Resource>)resources);
        }

        public Resource AddEmbedded(string name, IEnumerable<Resource> resources)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            
            if (!_embedded.ContainsKey(name))
            {
                _embedded.Add(name,new List<Resource>());
            }

            _embedded[name].AddRange(resources);
            return this;
        }
        
        private void AddOrReplaceField(string fieldName, JToken value)
        {
            if (_fields.ContainsKey(fieldName))
            {
                _fields[fieldName] = value;
            }
            else
            {
                _fields.Add(fieldName, value);
            }
        }
    }
}
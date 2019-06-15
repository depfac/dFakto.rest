using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace dFakto.Rest
{
    [JsonConverter(typeof(ResourceConverter))]
    public class Resource
    {
        private const string SelfPropertyName = "self";

        private readonly Dictionary<string, JToken> _fields = new Dictionary<string, JToken>();

        internal Resource()
        {
        }

        public Resource(string self)
        {
            if (string.IsNullOrWhiteSpace(self))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(self));
            }

            Self = self;
        }

        internal IDictionary<string, JToken> Fields => _fields;

        internal Dictionary<string, List<Link>> Links { get; } = new Dictionary<string, List<Link>>();

        internal Dictionary<string, List<Resource>> Embedded { get; } = new Dictionary<string, List<Resource>>();

        public string Self
        {
            get => Links[SelfPropertyName].FirstOrDefault()?.Href;
            set => AddLink(SelfPropertyName, value);
        }

        public T GetField<T>(string fieldName)
        {
            return !_fields.ContainsKey(fieldName) ? default(T) : _fields[fieldName].Value<T>();
        }

        public bool ContainsField(string name)
        {
            return _fields.ContainsKey(name);
        }

        public bool ContainsLink(string name)
        {
            return Links.ContainsKey(name);
        }

        public bool ContainsEmbedded(string name)
        {
            return Embedded.ContainsKey(name);
        }

        public Link[] GetLinks(string name)
        {
            if (!ContainsLink(name))
            {
                return new Link[0];
            }

            return Links[name].ToArray();
        }

        public Resource[] GetEmbedded(string name)
        {
            return !ContainsEmbedded(name) ? new Resource[0] : Embedded[name].ToArray();
        }

        public Resource RemoveField(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (_fields.ContainsKey(name))
            {
                _fields.Remove(name);
            }

            return this;
        }

        public Resource Add(object value, IEnumerable<string> only = null)
        {
            if (value == null)
            {
                return this;
            }

            var properties = (IEnumerable<PropertyInfo>) value.GetType().GetProperties();
            if (only != null)
            {
                var o = only.Select(x => x.ToLower()).ToArray();
                properties = properties.Where(x => o.Contains(x.Name.ToLower()));
            }

            foreach (var pro in properties)
            {
                var v = pro.GetValue(value);
                if (v != null)
                {
                    AddOrReplaceField(pro.Name, JToken.FromObject(v));
                }
                else if (_fields.ContainsKey(pro.Name))
                {
                    _fields.Remove(pro.Name);
                }
            }

            return this;
        }

        public Resource Add(string propertyName, JToken propertyValue)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            AddOrReplaceField(propertyName, propertyValue);

            return this;
        }

        public Resource AddLink(string name, Link link)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (link == null)
            {
                throw new ArgumentNullException(nameof(link));
            }

            if (!Links.ContainsKey(name))
            {
                Links.Add(name, new List<Link>());
            }

            Links[name].Add(link);
            return this;
        }

        public Resource AddLink(string name, string href)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (href == null)
            {
                throw new ArgumentNullException(nameof(href));
            }

            return AddLink(name, new Link(href));
        }

        public Resource AddEmbedded(string name, Resource resource)
        {
            return AddEmbedded(name, new[] {resource});
        }

        public Resource AddEmbedded(string name, params Resource[] resources)
        {
            return AddEmbedded(name, (IEnumerable<Resource>) resources);
        }

        public Resource AddEmbedded(string name, IEnumerable<Resource> resources)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (!Embedded.ContainsKey(name))
            {
                Embedded.Add(name, new List<Resource>());
            }

            Embedded[name].AddRange(resources);
            return this;
        }

        private void AddOrReplaceField(string fieldName, JToken value)
        {
            if (_fields.ContainsKey(fieldName) && value != null)
            {
                _fields[fieldName] = value;
            }
            else if (value != null)
            {
                _fields.Add(fieldName, value);
            }
            else
            {
                _fields.Remove(fieldName);
            }
        }
    }
}
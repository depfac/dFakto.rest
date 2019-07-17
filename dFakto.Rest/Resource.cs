using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace dFakto.Rest
{
    [JsonConverter(typeof(ResourceConverter))]
    public class Resource
    {        
        private readonly JObject _json;
        private readonly JObject _links;
        private readonly JsonSerializer _serializer;
        
        internal Resource(JObject json, JsonSerializer serializer)
        {
            _json = json;

            if (!json.ContainsKey(Properties.Links))
            {
                json.Add(Properties.Links,new JObject());
            }
            
            _links = (JObject) json[Properties.Links];
            _serializer = serializer;
        }

        internal JObject Json => _json;

        #region Accessors

        public IEnumerable<string> LinkNames
        {
            get { return _links.Properties().Select(x => x.Name); }
        }

        public IEnumerable<string> EmbeddedResourceNames
        {
            get
            {
                var e = (JObject) _json[Properties.Embedded];
                if (e == null)
                    return new string[0];
                return e.Properties().Select(x => x.Name);
            }
        }

        public IEnumerable<string> FieldsNames
        {
            get
            {
                return _json.Properties().Where(x => x.Name != Properties.Links && x.Name != Properties.Embedded)
                    .Select(x => x.Name);
            }
        }

        public T GetField<T>(string fieldName)
        {
            if(string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentNullException(nameof(fieldName));

            var name = GetPropertyName(fieldName);
            
            if(!_json.ContainsKey(name))
                return default(T);

            var t = _json[name];
            
            if (t is JObject || t is JArray)
                return t.ToObject<T>();
            
            return t.Value<T>();
        }

        public bool ContainsField(string name)
        {
            if(string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            
            if (name == Properties.Links || name == Properties.Embedded)
                return false;
            
            return _json.ContainsKey(GetPropertyName(name));
        }

        public bool ContainsLink(string rel)
        {
            if(string.IsNullOrWhiteSpace(rel))
                throw new ArgumentNullException(nameof(rel));
            
            return _links.ContainsKey(GetPropertyName(rel));
        }

        public bool ContainsEmbeddedResource(string name)
        {            
            if(string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            
            var e = (JObject)_json[Properties.Embedded];
            if(e == null)
                return false;
            return e.ContainsKey(GetPropertyName(name));
        }

        public Link GetLink(string rel)
        {
            return GetLinks(rel).FirstOrDefault();
        }

        public IEnumerable<Link> GetLinks(string rel)
        {
            if(string.IsNullOrWhiteSpace(rel))
                throw new ArgumentNullException(nameof(rel));

            string linkName = GetPropertyName(rel);
            
            if (!ContainsLink(linkName))
            {
                yield break;
            }

            var l = _links[linkName];
            
            if (l is JArray array)
            {
                foreach (var link in array)
                {
                    yield return link.ToObject<Link>();
                }
            }
            else
            {
                yield return l.ToObject<Link>();
            }
        }

        public IEnumerable<Resource> GetEmbeddedResources(string name)
        {
            if(string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            
            if(!ContainsEmbeddedResource(name))
                yield break;

            var e = ((JObject)_json[Properties.Embedded])[name];
            switch (e)
            {
                case JArray array:
                {
                    foreach (var embed in array)
                    {
                        yield return new Resource((JObject) embed, _serializer);
                    }

                    break;
                }

                default:
                    yield return new Resource((JObject) e, _serializer);
                    break;
            }
        }

        public Resource GetEmbeddedResource(string name)
        {
            if(string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            return GetEmbeddedResources(name).FirstOrDefault();
        }

        public Link GetSelf()
        {
            return GetLink(Properties.Self);
        }
        
        public T As<T>()
        {
            return _json.ToObject<T>(_serializer);
        }
        
        #endregion

        #region Modifiers

        public Resource Self(string uri)
        {
            if(string.IsNullOrWhiteSpace(uri))
                throw new ArgumentNullException(nameof(uri));
            
            AddLink(Properties.Self, uri);
            return this;
        }
        public Resource Merge<T>(T value) where T : class
        {
            if(value == null)
                throw new ArgumentNullException(nameof(value));

            return AddOrReplaceField(string.Empty, value, null);
        }
        public Resource Merge<T>(T value, IEnumerable<string> only) where T : class
        {
            if(value == null)
                throw new ArgumentNullException(nameof(value));

            return AddOrReplaceField(string.Empty, value, only);
        }
        public Resource Add(string propertyName, object value, IEnumerable<string> only = null)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            return AddOrReplaceField(GetPropertyName(propertyName), value, only);
        }

        public Resource AddLinks(string name, params Link[] links)
        {
            return AddLinks(name, (IEnumerable<Link>) links);
        }

        public Resource AddLinks(string name, IEnumerable<Link> links)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (links == null)
            {
                throw new ArgumentNullException(nameof(links));
            }

            var n = GetPropertyName(name);
            if (_links.ContainsKey(n))
            {
                _links.Remove(n);
            }

            JArray array = new JArray();
            foreach (var link in links)
            {
                if (link == null)
                    continue;
                
                array.Add(JObject.FromObject(link, _serializer));
            }

            _links.Add(n, array);

            return this;

        }

        public Resource AddLink(string name, Link link)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (link == null)
            {
                throw new ArgumentNullException(nameof(link));
            }
            
            var n = GetPropertyName(name);
            if (_links.ContainsKey(n))
            {
                _links.Remove(n);
            }

            _links.Add(n,JObject.FromObject(link, _serializer));

            return this;
        }
        public Resource AddLink(string name, string href)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(href))
            {
                throw new ArgumentNullException(nameof(href));
            }

            return AddLink(name, new Link(href));
        }
        
        public Resource AddEmbeddedResource(string name, Resource resource)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            string n = GetPropertyName(name);

            
            if (!_json.ContainsKey(Properties.Embedded))
            {
                if (resource != null)
                {
                    _links.Parent.AddAfterSelf(new JProperty(Properties.Embedded,new JObject()));
                    
                    var embedded = (JObject) _json[Properties.Embedded];
                    embedded.Add(n,JObject.FromObject(resource, _serializer));
                }
            }
            else
            {
                var embedded = (JObject) _json[Properties.Embedded];
                
                if (embedded.ContainsKey(n))
                {
                    embedded.Remove(n);
                }
                
                if (resource != null)
                {
                    embedded.Add(n,JObject.FromObject(resource, _serializer));
                }
            }
            return this;
        }
        public Resource AddEmbeddedResources(string name, params Resource[] resources)
        {
            return AddEmbeddedResources(name, (IEnumerable<Resource>) resources);
        }
        public Resource AddEmbeddedResources(string name, IEnumerable<Resource> resources)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (resources == null)
            {
                throw new ArgumentNullException(nameof(resources));
            }

            string n = GetPropertyName(name);
            var array = new JArray();
            foreach (var resource in resources)
            {
                if (resource == null)
                    continue;

                array.Add(JObject.FromObject(resource, _serializer));
            }
            
            if (!_json.ContainsKey(Properties.Embedded))
            {
                if (array.Count > 0)
                {
                    _links.Parent.AddAfterSelf(new JProperty(Properties.Embedded,new JObject()));
                    ((JObject)_json[Properties.Embedded]).Add(n,array);
                }
            }
            else
            {
                var embedded = (JObject) _json[Properties.Embedded];
                
                if (embedded.ContainsKey(n))
                {
                    embedded.Remove(n);
                }
                
                if (array.Count > 0)
                {
                    embedded.Add(n,array);
                }
            }
            return this;
        }

        private Resource AddOrReplaceField(string fieldName, object value, IEnumerable<string> only = null)
        {
            if(fieldName == Properties.Links ||
               fieldName == Properties.Embedded)
               throw new ArgumentException("Invalid field name, '_links' and '_embedded' are reserved names");
            
            if (fieldName != string.Empty)
            {
                if (_json.ContainsKey(fieldName))
                {
                    _json.Remove(fieldName);
                }

                if (value != null)
                {
                    var i = JToken.FromObject(value, _serializer);

                    if (i is JObject jObject)
                    {
                        var o = new JObject();
                        foreach (var token in GetOnlyJToken(jObject, only))
                        {
                            o.Add(token.Key, token.Value);
                        }

                        _json.Add(fieldName, o);
                    }
                    else
                    {
                        _json.Add(fieldName, i);
                    }
                }
            }
            else
            {
                var i = JObject.FromObject(value, _serializer);

                if (i is JObject jObject)
                {
                    var o = new JObject();
                    foreach (var token in GetOnlyJToken(jObject, only))
                    {
                        o.Add(token.Key, token.Value);
                    }

                    _json.Merge(o);
                }
            }

            return this;
        }

        private string GetPropertyName(string name, bool specificName = true)
        {
            if (_serializer.ContractResolver is DefaultContractResolver ss)
            {
                return ss.NamingStrategy?.GetPropertyName(name,specificName) ?? name;
            }
            return name;
        }

        private static IEnumerable<KeyValuePair<string,JToken>> GetOnlyJToken(JObject value, IEnumerable<string> only)
        {
            var enumerable = only == null ? null : only as string[] ?? only.ToArray();
            
            foreach (var t in value)
            {
                if (enumerable == null || enumerable.Contains(t.Key))
                {
                    yield return t;
                }
            }
        }

        #endregion

        public async Task WriteToAsync(Stream stream)
        {
            if(stream == null)
                throw new ArgumentNullException(nameof(stream));
            
            using(StreamWriter writer = new StreamWriter(stream))
            using(JsonTextWriter jsonWriter = new JsonTextWriter(writer))
            {
                await _json.WriteToAsync(jsonWriter);
            }
        }
    }
}
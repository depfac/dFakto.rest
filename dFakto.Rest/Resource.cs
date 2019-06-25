using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        public T GetField<T>(string fieldName)
        {
            if(!_json.ContainsKey(fieldName))
                return default(T);

            var t = _json[fieldName];
            
            if (t is JObject || t is JArray)
                return t.ToObject<T>();
            
            return t.Value<T>();
        }

        public bool ContainsField(string name)
        {
            if (name == Properties.Links || name == Properties.Embedded)
                return false;
            
            return _json.ContainsKey(name);
        }

        public bool ContainsLink(string name)
        {
            return _links.ContainsKey(name);
        }

        public bool ContainsEmbedded(string name)
        {
            var e = (JObject)_json[Properties.Embedded];
            if(e == null)
                return false;
            return e.ContainsKey(name) ;
        }

        public IEnumerable<Link> GetLinks(string name)
        {
            if (!ContainsLink(name))
            {
                yield break;
            }

            var l = _links[name];
            
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

        public IEnumerable<string> GetLinkNames()
        {
            return _links.Properties().Select(x => x.Name);
        }
        
        public IEnumerable<string> GetEmbeddedNames()
        {
            var e = (JObject)_json[Properties.Embedded];
            if(e == null)
                return new string[0];
            return e.Properties().Select(x => x.Name);
        }
        
        public IEnumerable<string> GetFieldsNames()
        {
            return _json.Properties().Where(x => x.Name != Properties.Links && x.Name != Properties.Embedded).Select(x => x.Name);
        }

        public IEnumerable<Resource> GetEmbedded(string name)
        {
            if(!ContainsEmbedded(name))
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

        public Link GetSelf()
        {
            return GetLinks(Properties.Self).FirstOrDefault();
        }

        #endregion

        #region Modifiers

        public Resource Self(string uri)
        {
            AddLink(Properties.Self, uri);
            return this;
        }
        public Resource Merge<T>(T value) where T : class
        {
            return AddOrReplaceField(string.Empty, value, null);
        }
        public Resource Merge<T>(T value, IEnumerable<string> only) where T : class
        {
            return AddOrReplaceField(string.Empty, value, only);
        }
        public Resource Add(string propertyName, object value, IEnumerable<string> only = null)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            return AddOrReplaceField(propertyName, value, only);
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

            var l = JObject.FromObject(link, _serializer);

            if (!_links.ContainsKey(name))
            {
                _links.Add(name,l);
            }
            else
            {
                if (_links[name].Type == JTokenType.Array)
                {
                    ((JArray) _links[name]).Add(l);
                }
                else
                {
                    JArray array = new JArray();
                    array.Add(_links[name]);
                    array.Add(l);
                    _links[name] = array;
                }
            }

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
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (resources == null)
            {
                throw new ArgumentNullException(nameof(resources));
            }

            if (!_json.ContainsKey(Properties.Embedded))
            {
                _links.Parent.AddAfterSelf(new JProperty(Properties.Embedded,new JObject()));
            }
            var embedded = ((JObject) _json[Properties.Embedded]);

            foreach (var res in resources)
            {
                var l = JObject.FromObject(res, _serializer);

                if (!embedded.ContainsKey(name))
                {
                    embedded.Add(name, l);
                }
                else
                {
                    if (embedded[name].Type == JTokenType.Array)
                    {
                        ((JArray) embedded[name]).Add(l);
                    }
                    else
                    {
                        JArray array = new JArray();
                        array.Add(embedded[name]);
                        array.Add(l);
                        embedded[name] = array;
                    }
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
    }
}
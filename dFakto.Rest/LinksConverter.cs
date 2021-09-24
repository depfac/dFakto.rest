using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace dFakto.Rest
{
    public class LinksConverter : JsonConverter
    {
        private const string HrefPropertyName = "href";
        private const string DeprecationPropertyName = "deprecation";
        private const string LangPropertyName = "lang";
        private const string NamePropertyName = "name";
        private const string ProfilePropertyName = "profile";
        private const string TemplatedPropertyName = "templated";
        private const string TitlePropertyName = "title";
        private const string TypePropertyName = "type";
        private const string RightsPropertyName = "verbs";
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Link link)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(HrefPropertyName);
                writer.WriteValue(link.Href);
                if (!string.IsNullOrWhiteSpace(link.Deprecation))
                {
                    writer.WritePropertyName(DeprecationPropertyName);
                    writer.WriteValue(link.Deprecation);
                }

                if (!string.IsNullOrWhiteSpace(link.Hreflang))
                {
                    writer.WritePropertyName(LangPropertyName);
                    writer.WriteValue(link.Hreflang);
                }

                if (!string.IsNullOrWhiteSpace(link.Name))
                {
                    writer.WritePropertyName(NamePropertyName);
                    writer.WriteValue(link.Name);
                }

                if (!string.IsNullOrWhiteSpace(link.Profile))
                {
                    writer.WritePropertyName(ProfilePropertyName);
                    writer.WriteValue(link.Profile);
                }

                if (link.Templated.HasValue && link.Templated.Value)
                {
                    writer.WritePropertyName(TemplatedPropertyName);
                    writer.WriteValue(link.Templated);
                }

                if (!string.IsNullOrWhiteSpace(link.Title))
                {
                    writer.WritePropertyName(TitlePropertyName);
                    writer.WriteValue(link.Title);
                }

                if (!string.IsNullOrWhiteSpace(link.Type))
                {
                    writer.WritePropertyName(TypePropertyName);
                    writer.WriteValue(link.Type);
                }

                if (link.Rights != Rights.All)
                {
                    writer.WritePropertyName(RightsPropertyName);                
                    writer.WriteStartArray();
                    foreach (var r in link.Rights.ToString().Split(','))
                    {
                        writer.WriteValue(r.Trim());
                    }
                    writer.WriteEndArray();
                }
                    
                writer.WriteEndObject();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var o = JToken.ReadFrom(reader) as JObject;;
            if (o == null)
                return null;

            var r = Rights.All;
            var rightArray = o.Property(RightsPropertyName);
            if (rightArray != null && rightArray.Value is JArray ra)
            {
                foreach (var ri in ra)
                {
                    r |= (Rights) Enum.Parse(typeof(Rights), ri.Value<string>());
                }
            }
            
            return new Link(o.Value<string>(HrefPropertyName))
            {
                Deprecation = o.Value<string>(DeprecationPropertyName),
                Hreflang = o.Value<string>(LangPropertyName),
                Name = o.Value<string>(NamePropertyName),
                Profile = o.Value<string>(ProfilePropertyName),
                Templated = o.Value<bool>(TemplatedPropertyName),
                Title = o.Value<string>(TitlePropertyName),
                Type = o.Value<string>(TypePropertyName),
                Rights = r
            };
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Link);
        }
    }
}
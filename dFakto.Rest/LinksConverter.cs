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
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Link link)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("href");
                writer.WriteValue(link.Href);
                if (!string.IsNullOrWhiteSpace(link.Deprecation))
                {
                    writer.WritePropertyName("deprecation");
                    writer.WriteValue(link.Deprecation);
                }

                if (!string.IsNullOrWhiteSpace(link.Hreflang))
                {
                    writer.WritePropertyName("lang");
                    writer.WriteValue(link.Hreflang);
                }

                if (!string.IsNullOrWhiteSpace(link.Name))
                {
                    writer.WritePropertyName("name");
                    writer.WriteValue(link.Name);
                }

                if (!string.IsNullOrWhiteSpace(link.Profile))
                {
                    writer.WritePropertyName("profile");
                    writer.WriteValue(link.Profile);
                }

                if (link.Templated.HasValue && link.Templated.Value)
                {
                    writer.WritePropertyName("templated");
                    writer.WriteValue(link.Templated);
                }

                if (!string.IsNullOrWhiteSpace(link.Title))
                {
                    writer.WritePropertyName("title");
                    writer.WriteValue(link.Title);
                }

                if (!string.IsNullOrWhiteSpace(link.Type))
                {
                    writer.WritePropertyName("type");
                    writer.WriteValue(link.Type);
                }

                if (link.Rights != Right.All)
                {
                    writer.WritePropertyName("rights");                
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

            var r = Right.All;
            var rightArray = o.Property("rights");
            if (rightArray != null && rightArray.Value is JArray ra)
            {
                foreach (var ri in ra)
                {
                    r |= (Right) Enum.Parse(typeof(Right), ri.Value<string>());
                }
            }
            
            return new Link(o.Value<string>("href"))
            {
                Deprecation = o.Value<string>("deprecation"),
                Hreflang = o.Value<string>("lang"),
                Name = o.Value<string>("name"),
                Profile = o.Value<string>("profile"),
                Templated = o.Value<bool>("templated"),
                Title = o.Value<string>("title"),
                Type = o.Value<string>("type"),
                Rights = r
            };
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Link);
        }
    }
}
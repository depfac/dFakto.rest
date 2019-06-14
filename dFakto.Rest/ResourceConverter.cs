using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace dFakto.Rest
{
    public class ResourceConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var resource = (Resource) value;
            writer.WriteStartObject();
            writer.WritePropertyName("_links");
            writer.WriteStartObject();
            foreach (var link in resource.Links)        
            {
                writer.WritePropertyName(link.Key);
                if (link.Value.Count == 1)
                {
                    JObject.FromObject(link.Value[0],serializer).WriteTo(writer);
                }
                else
                {
                    writer.WriteStartArray();
                    foreach (var l in link.Value)
                    {
                        JObject.FromObject(l,serializer).WriteTo(writer);
                    }
                    writer.WriteEndArray();
                }
            }
            writer.WriteEndObject(); // End links
            
            if (resource.Embedded.Count > 0)
            {
                writer.WritePropertyName("_embedded");
                writer.WriteStartObject();
                foreach (var embedded in resource.Embedded)        
                {
                    writer.WritePropertyName(embedded.Key);
                    if (embedded.Value.Count == 1)
                    {
                        JObject.FromObject(embedded.Value[0],serializer).WriteTo(writer);
                    }
                    else
                    {
                        writer.WriteStartArray();
                        foreach (var l in embedded.Value)
                        {
                            JObject.FromObject(l,serializer).WriteTo(writer);
                        }
                        writer.WriteEndArray();
                    }
                }
                writer.WriteEndObject(); // End embedded
            }

            foreach (var field in resource.Fields)
            {
                writer.WritePropertyName(field.Key);
                JToken.FromObject(field.Value).WriteTo(writer);
            }

            writer.WriteEndObject(); // End
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Resource);
        }
    }
}